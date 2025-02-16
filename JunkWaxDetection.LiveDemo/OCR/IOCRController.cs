using SkiaSharp;

namespace JunkWaxDetection.LiveDemo.OCR;

public interface IOCRController
{
    /// <summary>
    ///    Extracts text from the given image using OCR
    /// </summary>
    /// <param name="image"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    List<string> ExtractText(SKBitmap image);
}