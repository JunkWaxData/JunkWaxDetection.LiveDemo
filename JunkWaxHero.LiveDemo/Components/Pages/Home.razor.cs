using System.Diagnostics;
using System.Text;
using JunkWaxDetection.LiveDemo.CardList;
using JunkWaxDetection.LiveDemo.Enums;
using JunkWaxDetection.LiveDemo.ML;
using JunkWaxDetection.LiveDemo.OCR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using SkiaSharp;

namespace JunkWaxDetection.LiveDemo.Components.Pages
{
    public partial class Home
    {
        /// <summary>
        ///     JS Interop for communication with the browser for webcam and canvas operations
        /// </summary>
        [Inject] private IJSRuntime _jsRuntime { get; set; }

        /// <summary>
        ///     App settings for Configuration Values
        /// </summary>
        [Inject] private IOptions<AppSettings> _appSettings { get; set; }

        [Inject] private IMLController _mlController { get; set; }

        [Inject] private IOCRController _ocrController { get; set; }

        [Inject] private ICardListController _cardListController { get; set; }

        /// <summary>
        ///    Reference to the video element
        /// </summary>
        protected ElementReference videoRef;

        /// <summary>
        ///   Reference to the canvas element that overlays the video for bounding boxes and labels
        /// </summary>
        protected ElementReference canvasRef;

        /// <summary>
        ///      State to track if Inference is currently Running 
        /// </summary>
        protected bool isInferenceRunning;

        /// <summary>
        ///     Timer used to invoke the inference cycle
        /// </summary>
        protected Timer inferenceTimer;

        protected override async Task OnInitializedAsync()
        {
            _mlController.Init();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                // Start the webcam (JS interop)
                await _jsRuntime.InvokeVoidAsync("interop.startWebcam", videoRef);

                //
                await _jsRuntime.InvokeVoidAsync("interop.updateOverlay", canvasRef, _appSettings.Value.CropX, _appSettings.Value.CropWidth, _appSettings.Value.PreviewWidth, _appSettings.Value.PreviewHeight, null, null);
            }
        }

        void ToggleInference()
        {
            if (isInferenceRunning)
            {
                StopInference();
            }
            else
            {
                StartInference();
            }
        }

        void StartInference()
        {
            isInferenceRunning = true;
            // Start a timer that calls RunInferenceCycle at a configured interval.
            inferenceTimer = new Timer(async _ => await RunInferenceCycle(), null, 0, _appSettings.Value.InferenceDelay);
        }

        void StopInference()
        {
            isInferenceRunning = false;
            inferenceTimer?.Dispose();
            // Retain the last results.
            InvokeAsync(StateHasChanged);
        }

        async Task RunInferenceCycle()
        {
            try
            {
                // Capture the current frame from the video as a base64 string.
                // (JS interop function "captureFrame" returns the PNG data as a base64 string without header.)
                var base64Image = await _jsRuntime.InvokeAsync<string>("interop.captureCroppedFrame", videoRef, _appSettings.Value.CropX, _appSettings.Value.CropY,
                    _appSettings.Value.CropWidth, _appSettings.Value.CropHeight);

                using var original = SKBitmap.Decode(Convert.FromBase64String(base64Image));

                // Run the inference.
                var predictions = _mlController.GetPredictions(original);

                // Filter predictions above the threshold.
                var validPredictions = predictions.Where(p => p.Confidence >= _appSettings.Value.DetectionThreshold).ToList();

                //Nothing found? Bail.
                if (validPredictions.Count == 0)
                {
                    await _jsRuntime.InvokeVoidAsync("interop.updateOverlay", canvasRef, _appSettings.Value.CropX, _appSettings.Value.CropWidth, _appSettings.Value.PreviewWidth, _appSettings.Value.PreviewHeight, null, null);
                    await InvokeAsync(StateHasChanged);
                    return;
                }

                //We need to convert the boxes returned from the MLController to an array that can be consumed by the JS Interop
                var boxes = validPredictions.Select(prediction => new
                {
                    label = prediction.Label,
                    confidence = prediction.Confidence,
                    x = prediction.BoundingBox.Left + _appSettings.Value.CropX, //Add the cropX to the left coordinate to adjust for the cropped area
                    y = prediction.BoundingBox.Top + _appSettings.Value.CropY, //Add the cropY to the top coordinate to adjust for the cropped area
                    width = prediction.BoundingBox.Width,
                    height = prediction.BoundingBox.Height
                }).ToArray();

                //Get Best Prediction and paint card info on Webcam Preview
                var bestPrediction = validPredictions.OrderByDescending(p => p.Confidence).First();

                var card = GetCardInfo(original, bestPrediction);

                // Call JS to draw the bounding boxes onto the overlay canvas and update the side text
                await _jsRuntime.InvokeVoidAsync("interop.updateOverlay", canvasRef, _appSettings.Value.CropX, _appSettings.Value.CropWidth, _appSettings.Value.PreviewWidth, _appSettings.Value.PreviewHeight, boxes, _appSettings.Value.BBoxColor);
                await DrawSideText(GetSideText(bestPrediction, card), TextArea.Left);

                // Request a UI update.
                await InvokeAsync(StateHasChanged);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during inference: {ex.Message}");
            }
        }


        /// <summary>
        ///    Uses the OCR Controller to extract text from the cropped image that was detected by the model
        /// </summary>
        /// <param name="image"></param>
        /// <param name="prediction"></param>
        /// <returns></returns>
        private Card GetCardInfo(SKBitmap image, CardPrediction prediction)
        {
            //Crop the SKImage of the best prediction from the original image and run OCR (we'll use a Canvas to crop the image)
            var croppedImage = new SKBitmap((int)prediction.BoundingBox.Width, (int)prediction.BoundingBox.Height);
            using (var canvas = new SKCanvas(croppedImage))
            {
                canvas.DrawBitmap(image, new SKRect(prediction.BoundingBox.Left, prediction.BoundingBox.Top, prediction.BoundingBox.Right, prediction.BoundingBox.Bottom), new SKRect(0, 0, croppedImage.Width, croppedImage.Height));
            }

            var ocrText = _ocrController.ExtractText(croppedImage);

            foreach (var t in ocrText)
            {
                var playerResult = _cardListController.HasPlayer(prediction.Label, t);

                if (!playerResult.foundPlayer)
                    continue;

                return playerResult.card;
            }

            return new Card();
        }

        /// <summary>
        ///     Generates the test to be displayed on the side of the Webcam Preview showing the
        ///     set and card that are detected
        /// </summary>
        /// <param name="prediction"></param>
        /// <param name="card"></param>
        /// <returns></returns>
        private string GetSideText(CardPrediction prediction, Card card)
        {
            var output = new StringBuilder();

            output.AppendLine($"{prediction.Label.Replace("|", " ")}");
            output.AppendLine($"#{card.Number} - {card.Name}");

            return output.ToString();
        }

        /// <summary>
        ///     Draws text on the side of the preview image
        /// </summary>
        /// <param name="textToDraw"></param>
        /// <param name="textArea"></param>
        /// <returns></returns>
        private async Task DrawSideText(string textToDraw, TextArea textArea)
        {
            var area = Enum.GetName(textArea)?.ToLower();

            if (area == null)
                return;

            await _jsRuntime.InvokeVoidAsync("interop.drawSideText",
                canvasRef,          // The overlay canvas element
                Enum.GetName(textArea)?.ToLower(),             // Indicate left side
                textToDraw,// The text to display
                _appSettings.Value.CropX,              // cropX from your configuration
                _appSettings.Value.CropWidth,          // cropWidth from your configuration
                _appSettings.Value.PreviewWidth,       // previewWidth from your configuration
                _appSettings.Value.PreviewHeight);     // previewHeight from your configuration
        }
    }
}
