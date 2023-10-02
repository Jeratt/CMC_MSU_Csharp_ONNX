using MyYOLOApi;
using System.Windows.Input;
using CommunityToolkit.Mvvm;
using CommunityToolkit.Mvvm.Input;
using System.Reflection.Metadata.Ecma335;
using System.Drawing;

namespace ViewModel
{
    public interface IErrorReporter
    {
        void reportError(string message);
    }

    public class ViewData
    {
        private ModelManager? modelManager;

        private readonly IErrorReporter errorReporter;

        private WriterMM writer;

        private FileManagerMM fmmm;

        private string modelPath = "https://storage.yandexcloud.net/dotnet4/tinyyolov2-8.onnx";

        public ICommand ChooseNewDirectoryCommand { get; private set; }

        public ICommand CancelDetectionCommand { get; private set; }

        public List<Detected> DetectedImages { get; private set; }

        public ViewData(IErrorReporter errorReporter)
        {
            this.errorReporter = errorReporter;
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
            throw new NotImplementedException();
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