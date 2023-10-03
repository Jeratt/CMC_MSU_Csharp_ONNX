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
using System.ComponentModel;
using System.Collections.ObjectModel;

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

    public class ViewData: ViewModelBase, IDataErrorInfo
    {
        private ModelManager? modelManager;

        private readonly IErrorReporter errorReporter;

        private readonly IFolderManager folderManager;

        public WriterMM Writer { get; set; }

        private FileManagerMM fmmm;

        private string modelPath = "https://storage.yandexcloud.net/dotnet4/tinyyolov2-8.onnx";

        static CancellationTokenSource cts = new CancellationTokenSource();

        public ICommand ChooseNewDirectoryCommand { get; private set; }

        public ICommand CancelDetectionCommand { get; private set; }

        public ObservableCollection<Detected> DetectedImages { get; private set; }

        public string Message { 
            get { return this.Writer.Message; }
            //protected set { this.Writer.Message = value; } 
        }

        public string Error {
            get { return "Sorry, unhandled error occured"; }
        }

        public string this[string columnName]
        {
            get { return "Not implemented yet"; }
        }

        public static readonly List<string> ImageExtensions = new List<string> { ".JPG" };

        public ViewData(IErrorReporter errorReporter, IFolderManager folderManager)
        {
            this.errorReporter = errorReporter;
            this.folderManager = folderManager;
            this.Writer = new WriterMM();
            this.fmmm = new FileManagerMM();

            this.CancelDetectionCommand = new RelayCommand(CancelDetectionFunction);
            this.ChooseNewDirectoryCommand = new RelayCommand(ChooseNewDirectory);

            DetectedImages = new ObservableCollection<Detected>();
;
            modelManager = new ModelManager(modelPath, fmmm, Writer);
        }

        public void CancelDetectionFunction()
        {
            throw new NotImplementedException();
        }

        public async void ChooseNewDirectory()
        {
            string path = this.folderManager.openFolder();
            Image<Rgb24> img;
            Image<Rgb24> final;
            List<ObjectBox> lob;
            DetectedImages.Clear();
            foreach (var filename in System.IO.Directory.GetFiles(path))
            {
                // if filename is image
                if (ViewData.ImageExtensions.Contains(Path.GetExtension(filename).ToUpperInvariant()))
                {
                    img = SixLabors.ImageSharp.Image.Load<Rgb24>(filename);
                    try
                    {
                        lob = await modelManager.PredictAsync(img, ViewData.cts.Token);
                    }
                    catch(Exception x)
                    {
                        this.errorReporter.reportError(x.Message);
                        continue;
                    }
                    foreach(var ob in lob)
                    {
                        final = SixLabors.ImageSharp.Image.Load<Rgb24>("final.jpg");
                        DetectedImages.Add(new Detected(final, ob.Class.ToString(), img, ob.Confidence));
                    }
                }
                RaisePropertyChanged(nameof(DetectedImages));
            }
        }
    }

    public class WriterMM : IWriter
    {
        public string Message { get; set; }

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

    public record Detected
    {
        public Image<Rgb24> Image { get; init; }

        public Image<Rgb24> OriPic { get; init; }

        public string ClassName { get; init; }

        public double Confidence { get; init; }

        public Detected(Image<Rgb24> image, string className, Image<Rgb24> oriPic, double confidence)
        {
            Image = image;
            OriPic = oriPic;
            ClassName = className;
            Confidence = confidence;
        }
    }
}