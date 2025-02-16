using Microsoft.Extensions.Options;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SkiaSharp;

namespace JunkWaxDetection.LiveDemo.ML
{
    /// <summary>
    ///     This class is responsible for loading the ONNX model and running inference on images.
    ///
    ///     You first must Init() the model and which will download the latest model and labels from GitHub
    /// </summary>
    /// <param name="appSettings"></param>
    /// <param name="httpClient"></param>
    public class MLController(IOptions<AppSettings> appSettings, HttpClient httpClient) : IMLController
    {
        /// <summary>
        ///    Specifies if the Model has been Initialized and the Inference Session is ready for use
        /// </summary>
        public bool Initialized;

        /// <summary>
        ///     The ONNX Inference Session to be used
        /// </summary>
        private InferenceSession? _session;

        /// <summary>
        ///     The Labels for the Model to be used for Inference
        /// </summary>
        private List<string> _labels = [];

        /// <summary>
        ///     Initializes the ONNX Model and Labels for Inference by downloading the latest model and labels from GitHub
        /// </summary>
        public void Init()
        {
            //Kick off Downloads of the Model + Labels from GitHub in Parallel
            var modelTask = httpClient.GetByteArrayAsync(appSettings.Value.ModelUrl);
            var labelTask = httpClient.GetStringAsync(appSettings.Value.LabelsUrl);

            //Wait for Downloads to Finish
            Task.WaitAll(modelTask, labelTask);

            //Load the latest Model + Labels from GitHub
            _session = new InferenceSession(modelTask.Result);
            _labels = (labelTask.Result)
                .Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries).ToList();

            Initialized = true;
        }

        /// <summary>
        ///     Initializes the ONNX Model and Labels for Inference with the given model and labels
        /// </summary>
        /// <param name="onnxModel"></param>
        /// <param name="labels"></param>
        public void Init(byte[] onnxModel, List<string> labels)
        {
            _session = new InferenceSession(onnxModel);
            _labels = labels;
            Initialized = true;
        }

        /// <summary>
        ///     Runs the loaded ONNX model on the given image and returns predictions
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public List<CardPrediction> GetPredictions(SKBitmap image)
        {
            //Resize the Image to the Required Dimensions
            var resized = ResizeImage(image);

            //Get DenseTensor for the Resized Image
            var tensor = GetTensorForImage(resized);

            //Run Inference on the Image
            var predictions = GetPredictions(tensor);

            //Adjust the Bounding Boxes to the Original Image Size
            var boxes = ResizePredictionBoxes(predictions, image);

            return boxes;
        }


        /// <summary>
        ///     Runs the ONNX model on the given tensor and returns predictions.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public List<CardPrediction> GetPredictions(Tensor<float> input)
        {
            if(!Initialized)
                throw new InvalidOperationException("Session not Initialized");

            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("image_tensor", input)
            };

            using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = _session.Run(inputs);
            var resultsDict = results.ToDictionary(x => x.Name, x => x);
            var detectedBoxes = resultsDict["detected_boxes"].AsTensor<float>();
            var detectedClasses = resultsDict["detected_classes"].AsTensor<long>();
            var detectedScores = resultsDict["detected_scores"].AsTensor<float>();

            var numBoxes = detectedClasses.Length;
            var output = new List<CardPrediction>();

            for (var i = 0; i < numBoxes; i++)
            {
                var score = detectedScores[0, i];
                var classId = detectedClasses[0, i];
                var x = detectedBoxes[0, i, 0];
                var y = detectedBoxes[0, i, 1];
                var x2 = detectedBoxes[0, i, 2];
                var y2 = detectedBoxes[0, i, 3];

                // Convert normalized coordinates (for a 320x320 image) to pixel coordinates.
                x *= appSettings.Value.RequiredWidth;
                y *= appSettings.Value.RequiredHeight;
                x2 *= appSettings.Value.RequiredWidth;
                y2 *= appSettings.Value.RequiredHeight;

                var rect = new SKRect(x, y, x2, y2);
                output.Add(new CardPrediction(_labels[(int)classId], score, rect));
            }

            return output;
        }

        /// <summary>
        ///     Resizes the prediction boxes to align with the original image dimensions from the resized image size
        ///     used by the model for inference.
        /// </summary>
        /// <param name="predictions"></param>
        /// <param name="originalImage"></param>
        public List<CardPrediction> ResizePredictionBoxes(List<CardPrediction> predictions, SKBitmap originalImage)
        {
            // Draw bounding boxes for each valid detection on the overlay.
            // (We need to convert the 320x320 bounding box coordinates back to the full overlay coordinates.)
            var boxes = predictions.Select(prediction =>
            {
                var x = (prediction.BoundingBox.Left / appSettings.Value.RequiredWidth * appSettings.Value.CropWidth);
                var y =  (prediction.BoundingBox.Top / appSettings.Value.RequiredHeight * appSettings.Value.CropHeight);
                var w = ((prediction.BoundingBox.Right - prediction.BoundingBox.Left) / appSettings.Value.RequiredWidth * appSettings.Value.CropWidth);
                var h = ((prediction.BoundingBox.Bottom - prediction.BoundingBox.Top) / appSettings.Value.RequiredHeight * appSettings.Value.CropHeight);
                return new CardPrediction(prediction.Label, prediction.Confidence, new SKRect(x, y, x + w, y + h));
            }).ToList();

            return boxes;
        }

        /// <summary>
        ///     Resizes an SKBitmap to the required dimensions for the model to run inference
        /// </summary>
        /// <param name="originalImage"></param>
        /// <returns></returns>
        public SKBitmap ResizeImage(SKBitmap originalImage)
        {
            // Resize the cropped image to required size for the ONNX model
            var resized = new SKBitmap(appSettings.Value.RequiredWidth, appSettings.Value.RequiredHeight);
            using var canvas = new SKCanvas(resized);
            canvas.DrawBitmap(originalImage, new SKRect(0, 0, appSettings.Value.RequiredWidth, appSettings.Value.RequiredHeight));

            return resized;
        }

        /// <summary>
        ///     Creates a DenseTensor from an SKBitmap image (RGB format).
        /// </summary>
        public Tensor<float> GetTensorForImage(SKBitmap image)
        {
            var tensor = new DenseTensor<float>([1, 3, appSettings.Value.RequiredHeight, appSettings.Value.RequiredWidth]);
            for (var y = 0; y < appSettings.Value.RequiredHeight; y++)
            {
                for (var x = 0; x < appSettings.Value.RequiredWidth; x++)
                {
                    var pixel = image.GetPixel(x, y);
                    tensor[0, 0, y, x] = pixel.Red;
                    tensor[0, 1, y, x] = pixel.Green;
                    tensor[0, 2, y, x] = pixel.Blue;
                }
            }
            return tensor;
        }
    }
}
