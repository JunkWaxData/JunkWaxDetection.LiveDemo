using Microsoft.ML.OnnxRuntime.Tensors;
using SkiaSharp;

namespace JunkWaxDetection.LiveDemo.ML;

public interface IMLController
{
    /// <summary>
    ///     Initializes the ONNX Model and Labels for Inference by downloading the latest model and labels from GitHub
    /// </summary>
    void Init();

    /// <summary>
    ///     Initializes the ONNX Model and Labels for Inference with the given model and labels
    /// </summary>
    void Init(byte[] onnxModel, List<string> labels);

    /// <summary>
    ///     Runs the ONNX model on the given tensor and returns predictions.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    List<CardPrediction> GetPredictions(Tensor<float> input);

    /// <summary>
    ///     Runs the ONNX model on the given tensor and returns predictions.
    /// </summary>
    /// <param name="image"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    List<CardPrediction> GetPredictions(SKBitmap image);

    /// <summary>
    ///     Resizes the prediction boxes to align with the original image dimensions from the resized image size
    ///     used by the model for inference.
    /// </summary>
    /// <param name="predictions"></param>
    /// <param name="originalImage"></param>
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