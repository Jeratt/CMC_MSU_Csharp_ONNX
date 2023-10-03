using MyYOLOApi;
using System.Windows.Input;
using CommunityToolkit.Mvvm;
using CommunityToolkit.Mvvm.Input;
using System.Reflection.Metadata.Ecma335;
using System.Drawing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;

namespace ViewModel
{
    public interface IErrorReporter
    {
        void reportError(string message);
    }

    public interface IFolderManager
    {
        string openFolder();
    }

    public class ViewData
    {
        private ModelManager? modelManager;

        private readonly IErrorReporter errorReporter;

        private readonly IFolderManager folderManager;

        private WriterMM writer;

        private FileManagerMM fmmm;

        private string modelPath = "https://storage.yandexcloud.net/dotnet4/tinyyolov2-8.onnx";

        public ICommand ChooseNewDirectoryCommand { get; private set; }

        public ICommand CancelDetectionCommand { get; private set; }

        public List<Detected> DetectedImages { get; private set; }

        public static readonly List<string> ImageExtensions = new List<string> { ".JPG" };

        public ViewData(IErrorReporter errorReporter, IFolderManager folderManager)
        {
            this.errorReporter = errorReporter;
            this.folderManager = folderManager;
            this.writer = new WriterMM();
            this.fmmm = new FileManagerMM();

            this.CancelDetectionCommand = new RelayCommand(CancelDetectionFunction);
            this.ChooseNewDirectoryCommand = new RelayCommand(ChooseNewDirectory);

            DetectedImages = new List<Detected>();
;
            modelManager = new ModelManager(modelPath, fmmm, writer);
        }

        private void CancelDetectionFunction()
        {
            throw new NotImplementedException();
        }

        private void ChooseNewDirectory()
        {
            string path = this.folderManager.openFolder();
            Image<Rgb24> img;
            foreach(var filename in System.IO.Directory.GetFiles(path))
            {
                // if filename is image
                if (ViewData.ImageExtensions.Contains(Path.GetExtension(filename).ToUpperInvariant()))
                {
                    img = SixLabors.ImageSharp.Image.Load<Rgb24>(filename);
                }
            }
        }
    }

    class WriterMM : IWriter
    {
        public string Message { get; protected set; }

        public WriterMM()
        {
            Message = string.Empty;
        }
        public void PrintText(string text)
        {
            Message = text;
        }
    }

    class FileManagerMM: IFileManager
    {
        public bool CheckIfExists(string path) => File.Exists(path);

        public void WriteBytes(string path, byte[] bytes) => File.WriteAllBytes(path, bytes);
    }

    public class Detected
    {
        public Bitmap Image { get; protected set; }

        public string ClassName { get; protected set; }

        public double Confdidence { get; protected set; }

        public Detected(Bitmap image, string className, double confdidence)
        {
            Image = image;
            ClassName = className;
            Confdidence = confdidence;
        }
    }
}