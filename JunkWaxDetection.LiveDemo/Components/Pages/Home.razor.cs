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
using MatchType = JunkWaxDetection.LiveDemo.CardList.Enums.MatchType;

namespace JunkWaxDetection.LiveDemo.Components.Pages
{
    public partial class Home
    {
        /// <summary>
        ///     JS Interop for communication with the browser for webcam and canvas operations
        /// </summary>
        [Inject]
        private IJSRuntime _jsRuntime { get; set; }

        /// <summary>
        ///     App settings for Configuration Values
        /// </summary>
        [Inject]
        private IOptions<AppSettings> _appSettings { get; set; }

        /// <summary>
        ///     ML Controller for Inference of images captured from the Webcam Feed
        /// </summary>
        [Inject]
        private IMLController _mlController { get; set; }

        /// <summary>
        ///     OCR Controller for extracting text from the cropped image from the object bounding box
        /// </summary>
        [Inject]
        private IOCRController _ocrController { get; set; }

        /// <summary>
        ///     Card List Controller for searching through card sets from Junk Wax Data GitHub Repository
        /// </summary>
        [Inject]
        private ICardListController _cardListController { get; set; }

        /// <summary>
        ///    Reference to the video element
        /// </summary>
        protected ElementReference VideoRef;

        /// <summary>
        ///   Reference to the canvas element that overlays the video for bounding boxes and labels
        /// </summary>
        protected ElementReference CanvasRef;

        /// <summary>
        ///      State to track if Inference is currently Running 
        /// </summary>
        protected bool IsInferenceRunning;

        /// <summary>
        ///     Timer used to invoke the inference cycle
        /// </summary>
        private Timer _inferenceTimer;

        /// <summary>
        ///     Text to be rendered to the left of the WebCam Preview
        /// </summary>
        private string _leftText = string.Empty;

        /// <summary>
        ///     Text to be rendered to the right of the WebCam Preview
        /// </summary>
        private readonly string _rightText =
            "Live Demo powered by ASP.NET Blazor +\n" +
            "the JunkWaxDetection Object Detection\n" + 
            "model through ML.NET!\n\n" +
            "The JunkWaxDetection Objection Detection\n" + 
            "model (ONNX + TF) is available on Github!\n\n" + 
            "Webcam preview cropped at 3:4 ratio to\n" +
            "simulate portrait mode images captured by\n" + 
            "a cell phone, as the JunkWaxDetection model\n" +
            "is optimized for Mobile applications.";

        private CardSearchResult _currentCardSearchResult = new();
        protected override async Task OnInitializedAsync()
        {
            // Initialize the ML Controller
            _mlController.Init();
        }

        /// <summary>
        ///     After the page has rendered, interact with the browser to start the webcam and draw the webcam overlay
        /// </summary>
        /// <param name="firstRender"></param>
        /// <returns></returns>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {

            //Setup the initial page state by starting the webcam and drawing the overlay
            if (firstRender)
            {
                // Start the webcam (JS interop)
                await _jsRuntime.InvokeVoidAsync("interop.startWebcam", VideoRef);

                // Draw the overlay cropping the webcam to 3:4 aspect ratio
                await _jsRuntime.InvokeVoidAsync("interop.updateOverlay", CanvasRef, _appSettings.Value.CropX, _appSettings.Value.CropWidth, _appSettings.Value.PreviewWidth, _appSettings.Value.PreviewHeight, null, null, _leftText, _rightText);
            }
        }

        /// <summary>
        ///     Utilized by the button to toggle the Inference Cycle
        /// </summary>
        private async Task ToggleInferenceAsync()
        {
            if (IsInferenceRunning)
            {
                await StopInferenceAsync();
            }
            else
            {
                await StartInferenceAsync();
            }
        }

        /// <summary>
        ///     Begins a timer to run the Inference Cycle
        /// </summary>
        private async Task StartInferenceAsync()
        {
            IsInferenceRunning = true;
            // Start a timer that calls RunInferenceCycle at a configured interval.
            _inferenceTimer = new Timer(async _ => await RunInferenceCycle(), null, 0, _appSettings.Value.InferenceDelay);
        }

        /// <summary>
        ///     Kills the Inference Cycle
        /// </summary>
        /// <returns></returns>
        private async Task StopInferenceAsync()
        {
            IsInferenceRunning = false;
            _inferenceTimer?.Dispose();
        }

        /// <summary>
        ///     Runs the Inference Cycle by:
        ///
        ///     1. Capturing the current frame from the video as a base64 encoded JPEG (cropped by 3:4 aspect ratio)
        ///     2. Running inference on the image captured (model is expecting 3:4 aspect ratio images)
        ///     3. Filtering predictions above the threshold
        ///     4. Drawing the bounding boxes on the overlay canvas
        ///     5. Updating the side text with the detected card information
        /// </summary>
        /// <returns></returns>
        private async Task RunInferenceCycle()
        {
            try
            {
                // Capture the current frame from the video as a base64 string.
                // (JS interop function "captureFrame" returns the PNG data as a base64 string without header.)
                var base64Image = await _jsRuntime.InvokeAsync<string>("interop.captureCroppedFrame", VideoRef, _appSettings.Value.CropX, _appSettings.Value.CropY,
                    _appSettings.Value.CropWidth, _appSettings.Value.CropHeight);

                using var original = SKBitmap.Decode(Convert.FromBase64String(base64Image));

                // Run the inference.
                var predictions = _mlController.GetPredictions(original);

                // Filter predictions above the threshold.
                var validPredictions = predictions.Where(p => p.Confidence >= _appSettings.Value.DetectionThreshold).ToList();

                //Nothing found? Bail.
                if (validPredictions.Count == 0)
                {
                    _currentCardSearchResult = new(); // Clear the card search result if there was one because we didn't find anything
                    await _jsRuntime.InvokeVoidAsync("interop.updateOverlay", CanvasRef, _appSettings.Value.CropX, _appSettings.Value.CropWidth, _appSettings.Value.PreviewWidth, _appSettings.Value.PreviewHeight, null, null, string.Empty, _rightText);
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

                if(_currentCardSearchResult.Match is MatchType.None or MatchType.StartsWith)
                    GetCardInfo(original, bestPrediction);

                // Call JS to draw the bounding boxes onto the overlay canvas and update the side text
                SetLeftText(bestPrediction, _currentCardSearchResult.Card);
                await _jsRuntime.InvokeVoidAsync("interop.updateOverlay", CanvasRef, _appSettings.Value.CropX, _appSettings.Value.CropWidth, _appSettings.Value.PreviewWidth, _appSettings.Value.PreviewHeight, boxes, _appSettings.Value.BBoxColor, _leftText, _rightText);

                // Request a UI update.
                //await InvokeAsync(StateHasChanged);
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
        private void GetCardInfo(SKBitmap image, CardPrediction prediction)
        {
            //Crop the SKImage of the best prediction from the original image and run OCR (we'll use a Canvas to crop the image)
            var croppedImage = new SKBitmap((int)prediction.BoundingBox.Width, (int)prediction.BoundingBox.Height);
            using (var canvas = new SKCanvas(croppedImage))
            {
                canvas.DrawBitmap(image, new SKRect(prediction.BoundingBox.Left, prediction.BoundingBox.Top, prediction.BoundingBox.Right, prediction.BoundingBox.Bottom), new SKRect(0, 0, croppedImage.Width, croppedImage.Height));
            }

            var ocrText = _ocrController.ExtractText(croppedImage);

            var results = new List<CardSearchResult>();

            foreach (var t in ocrText)
            {
                if (_cardListController.CardSearch(prediction.Label, t, out var searchResult))
                {
                    results.Add(searchResult);
                }
            }

            //Always return any exact matches first
            if (results.Any(x => x.Match == MatchType.Exact))
            {
                _currentCardSearchResult = results.First(x => x.Match == MatchType.Exact);
                return;
            }


            //We'll settle for partial matches if no exact matches are found
            if (results.Any(x => x.Match == MatchType.StartsWith))
            {
                _currentCardSearchResult = results.First(x => x.Match == MatchType.StartsWith);
                return;
            }

            //Default it to none if we didn't find anything
            _currentCardSearchResult = new();
        }

        /// <summary>
        ///     Generates the text to be displayed on the side of the Webcam Preview showing the
        ///     set and card that are detected
        /// </summary>
        /// <param name="prediction"></param>
        /// <param name="card"></param>
        /// <returns></returns>
        private void SetLeftText(CardPrediction prediction, Card card)
        {
            var output = new StringBuilder();
            output.AppendLine("Detected Card:");
            output.AppendLine("-------------------------");
            output.AppendLine($"{prediction.Label.Replace("|", " ")}");
            output.AppendLine($"#{card.Number} - {card.Name}");

            if(card.Attributes != null && card.Attributes.Contains("RC"))
                output.AppendLine("(Rookie Card)");

            _leftText = output.ToString();
        }
    }
}
