namespace JunkWaxDetection.LiveDemo
{
    public class AppSettings
    {
        /// <summary>
        ///     The number of Milliseconds between Inference Runs
        /// </summary>
        public int InferenceDelay { get; set; }

        /// <summary>
        ///     The minimum confidence level for a detection to be considered valid
        /// </summary>
        public float DetectionThreshold { get; set; }

        /// <summary>
        ///     The color of the bounding box to be drawn on the Canvas (Hex)
        /// </summary>
        public string BBoxColor { get; set; } = "#000000"; // Black by Default

        /// <summary>
        ///     The required width of images to be fed into the model
        /// </summary>
        public int RequiredWidth { get; set; }

        /// <summary>
        ///     The required height of images to be fed into the model
        /// </summary>
        public int RequiredHeight { get; set; }


        /// <summary>
        ///     Width of the Webcam Preview displayed on the Page
        /// </summary>
        public int PreviewWidth { get; set; }

        /// <summary>
        ///     Height of the Webcam Preview displayed on the Page
        /// </summary>
        public int PreviewHeight { get; set; }

        /// <summary>
        ///     Width of the Cropped area to be used for Inference (Model Requires 3:4)
        /// </summary>
        public int CropWidth { get; set; }

        /// <summary>
        ///     Height of the Cropped area to be used for Inference (Model Requires 3:4)
        /// </summary>
        public int CropHeight { get; set; }

        /// <summary>
        ///     X Coordinate of the Top Left Corner of the Cropped area
        /// </summary>
        public int CropX { get; set; }

        /// <summary>
        ///    Y Coordinate of the Top Left Corner of the Cropped area
        /// </summary>
        public int CropY { get; set; }

        /// <summary>
        ///     The URL of the Model to be used for Inference
        /// </summary>
        public string ModelUrl { get; set; } = string.Empty;

        /// <summary>
        ///    The URL of the Labels for the Model
        /// </summary>
        public string LabelsUrl { get; set; } = string.Empty;

        /// <summary>
        ///     Base URL for Card Lists 
        /// </summary>
        public string CardListBaseUrl { get; set; } = string.Empty;
    }
}
