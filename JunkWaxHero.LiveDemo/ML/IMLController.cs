using Microsoft.ML.OnnxRuntime.Tensors;
using SkiaSharp;

namespace JunkWaxDetection.LiveDemo.ML;

public interface IMLController
{
    void Init();

    void Init(byte[] onnxModel, List<string> labels);

    /// <summary>
    /// Runs the ONNX model on the given tensor and returns predictions.
    /// Each prediction is a Tuple of (label, score, bounding box in 320x320 coordinates).
    /// </summary>
    List<CardPrediction> GetPredictions(Tensor<float> input);

    List<CardPrediction> GetPredictions(SKBitmap image);

    List<CardPrediction> ResizePredictionBoxes(List<CardPrediction> predictions,
        SKBitmap originalImage);

    /// <summary>
    ///     Resizes an SKBitmap to the required dimensions for the model to run inference
    /// </summary>
    /// <param name="originalImage"></param>
    /// <returns></returns>
    SKBitmap ResizeImage(SKBitmap originalImage);

    /// <summary>
    ///     Creates a DenseTensor from an SKBitmap image (RGB format).
    /// </summary>
    Tensor<float> GetTensorForImage(SKBitmap image);
}