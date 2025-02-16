using SkiaSharp;

namespace JunkWaxDetection.LiveDemo.ML
{
    /// <summary>
    ///     Represents a prediction for a detected card within the image
    /// </summary>
    /// <param name="label"></param>
    /// <param name="confidence"></param>
    /// <param name="boundingBox"></param>
    public class CardPrediction(string label, float confidence, SKRect boundingBox)
    {
        /// <summary>
        ///     The label from the model for the detected card
        /// </summary>
        public string Label { get; set; } = label;

        /// <summary>
        ///     The confidence of the model for the detected card
        ///
        ///     Range: 0.0 - 1.0
        /// </summary>
        public float Confidence { get; set; } = confidence;

        /// <summary>
        ///     The bounding box for the detected card within the image
        /// </summary>
        public SKRect BoundingBox { get; set; } = boundingBox;
    }
}
