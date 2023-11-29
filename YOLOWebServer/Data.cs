using MyYOLOApi;

namespace YOLOWebServer
{
    public class Detected
    {
    }

    public class HtmlWritter : IWriter
    {
        public void PrintText(string text)
        {
        }
    }

    public class WebFileManager: IFileManager
    {

    }

    public record RequestedData
    {
        public byte[] Image { get; init; }
        public int Width { get; init; }
        public int Height { get; init; }
        
        public RequestedData(byte[] data)
        {
            Image = data;
/*            Width = width;
            Height = height;*/
        }
    }
}
