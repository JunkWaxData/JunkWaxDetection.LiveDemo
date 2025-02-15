using OpenCvSharp;
using Sdcb.PaddleOCR;
using Sdcb.PaddleOCR.Models.Local;
using SkiaSharp;

namespace JunkWaxDetection.LiveDemo.OCR
{
    public class OCRController : IOCRController, IDisposable
    {
        private PaddleOcrAll _paddleOcr;

        public OCRController()
        {
            _paddleOcr = new(LocalFullModels.EnglishV4)
            {
                AllowRotateDetection = true,
                Enable180Classification = false,
            };
        }

        /// <summary>
        ///    Extracts text from the given image using OCR
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public List<string> ExtractText(SKBitmap bitmap)
        {
            try
            {
                // Encode the SKBitmap into a PNG (or another supported format)
                using var skData = bitmap.Encode(SKEncodedImageFormat.Png, 100);
                var encodedData = skData.ToArray();

                // Decode the encoded image into a Mat.
                using var mat = Cv2.ImDecode(encodedData, ImreadModes.Color);

                var result = _paddleOcr.Run(mat);
                return result.Text.Split('\n').ToList();
            }
            catch (Exception e)
            {
                //If the exception happens, it's most likely in the unmanaged code. 
                //We'll clean up the resource and new it up
                _paddleOcr?.Dispose();
                _paddleOcr = new(LocalFullModels.EnglishV4)
                {
                    AllowRotateDetection = true,
                    Enable180Classification = false,
                };

                return [];
            }
        }

        public void Dispose()
        {
            _paddleOcr?.Dispose();
        }

    }
}
