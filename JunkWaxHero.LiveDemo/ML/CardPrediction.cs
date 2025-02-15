using SkiaSharp;

namespace JunkWaxDetection.LiveDemo.ML
{
    public class CardPrediction(string label, float confidence, SKRect boundingBox)
    {
        public string Label { get; set; } = label;

        public float Confidence { get; set; } = confidence;

        public SKRect BoundingBox { get; set; } = boundingBox;
    }
}
