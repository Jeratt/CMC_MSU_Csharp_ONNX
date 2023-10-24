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
using System.Xml.Linq;
using System.Windows.Media;
using ImageSharp.WpfImageSource;
using ImageVault;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats;
using static System.Windows.Forms.DataFormats;

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

    public interface IImageManager
    {
        void SetSource(string source);
    }

    public class ViewData: ViewModelBase, IDataErrorInfo
    {
        private static bool errorReported = false;

        private ModelManager? modelManager;

        private readonly IErrorReporter errorReporter;

        private readonly IFolderManager folderManager;

        public TestWriterMM Writer { get; set; }

        private FileManagerMM fmmm;

        private string modelPath = "https://storage.yandexcloud.net/dotnet4/tinyyolov2-8.onnx";

        private YOLOVault vault;

        static CancellationTokenSource cts = new CancellationTokenSource();

        public AsyncRelayCommand ChooseNewDirectoryCommand { get; private set; }

        public ICommand CancelDetectionCommand { get; private set; }

        public ICommand ClearVaultCommand { get; private set; }

        public ObservableCollection<Detected> DetectedImages { get; private set; }

        public string Error {
            get { return "Sorry, unhandled error occured"; }
        }

        public string this[string columnName]
        {
            get { return "Not implemented yet"; }
        }

        public static readonly List<string> ImageExtensions = new List<string> { ".JPG", ".JPEG"};

        public ViewData(IErrorReporter errorReporter, IFolderManager folderManager)
        {
            this.errorReporter = errorReporter;
            this.folderManager = folderManager;
            this.Writer = new TestWriterMM();
            this.fmmm = new FileManagerMM();
            this.vault = new YOLOVault();

            this.CancelDetectionCommand = new RelayCommand(() => { CancelDetectionFunction(this); });
            this.ClearVaultCommand = new RelayCommand(() => { ClearVault(this); });
            this.ChooseNewDirectoryCommand = new AsyncRelayCommand(async _ =>
            {
                await ChooseNewDirectoryAsync();
            } );

            DetectedImages = new ObservableCollection<Detected>();
;
            modelManager = new ModelManager(modelPath, fmmm, Writer);

            this.Init();
        }

        private void ClearVault(object sender)
        {
            this.DetectedImages.Clear();
            try
            {
                this.vault.ClearVault();
            }
            catch(Exception ex)
            {
                this.errorReporter.reportError(ex.Message);
            }
        }

        private void CancelDetectionFunction(object sender)
        {
            try
            {
                ViewData.cts.Cancel();
            }
            catch(Exception x)
            {
                return;
            }
        }

        private async Task ChooseNewDirectoryAsync()
        {
            ViewData.cts = new CancellationTokenSource();
            ViewData.errorReported = false;
            string path = this.folderManager.openFolder();
            string name;
            string final_name;
            Image<Rgb24> img;
            List<Tuple<Task<List<ObjectBox>>, string, Image<Rgb24>>> lst_t = new List<Tuple<Task<List<ObjectBox>>, string, Image<Rgb24>>>();
            List<Detected> detected_copy = new List<Detected>();
            List<ObjectBox> lob_awaited;
            DetectedImages.Clear();
            foreach (var filename in System.IO.Directory.GetFiles(path))
            {
                // if filename is image
                if (ViewData.ImageExtensions.Contains(Path.GetExtension(filename).ToUpperInvariant()))
                {
                    if (this.vault.CheckFile(filename))
                    { continue; }

                    img = SixLabors.ImageSharp.Image.Load<Rgb24>(filename);
                    try
                    {
                        name = Path.GetFileNameWithoutExtension(filename);
                        //this.ProcessAsync(img, filename);
                        lst_t.Add(new Tuple<Task<List<ObjectBox>>, string, Image<Rgb24>>(modelManager.PredictAsync(img, ViewData.cts.Token, name), filename, img));
                    }
                    catch (Exception x)
                    {
                        if (!ViewData.errorReported)
                        {
                            this.errorReporter.reportError(x.Message);
                            ViewData.errorReported = true;
                        }
                        continue;
                    }
                }
            }
            //await Task.Delay(3000);
            foreach (var lob in lst_t)
            {
                final_name = Path.GetFileNameWithoutExtension(lob.Item2) + "final.jpg";
                try
                {
                    lob_awaited = await lob.Item1;
                    foreach (var ob in lob_awaited)
                    {
                        var oriImage = lob.Item3;
                        var finalImage = this.modelManager.GetFinal(oriImage, lob_awaited);
                        DetectedImages.Add(new Detected(finalImage, ob.Class, oriImage, ob.Confidence));
                    }
                }
                catch (Exception x)
                {
                    if (!ViewData.errorReported)
                    {
                        this.errorReporter.reportError(x.Message);
                        ViewData.errorReported = true;
                    }
                    continue;
                }
            }
            RaisePropertyChanged(nameof(DetectedImages));
            this.UpdateVault();
        }

        private void UpdateVault()
        {
            List<SerializableDetected> lst = new List<SerializableDetected>();
            byte[] detectedImage, oriImage;
            foreach (var item in this.DetectedImages)
            {
                detectedImage = item.DetectedImage.ToByteArray();
                oriImage = item.OriPic.ToByteArray();
                lst.Add(new SerializableDetected(detectedImage, oriImage, item.ClassName, item.Confidence, item.OriPic.Width, item.OriPic.Height));
            }
            this.vault.UpdateVault(lst);
        }

        public void Init()
        {
            this.DetectedImages.Clear();
            List<SerializableDetected>? stored = this.vault.LoadFromVault();
            if (stored is null)
                return;
            Image<Rgb24> detectedIm, oriIm;
            foreach (var item in stored)
            {
                detectedIm = SixLabors.ImageSharp.Image.LoadPixelData<Rgb24>(item.DetectedImage, 416, 416);
                //detectedIm = ImageDecoder.Decode(item.DetectedImage);
                oriIm = SixLabors.ImageSharp.Image.LoadPixelData<Rgb24>(item.OriPic, (int)item.Width, (int)item.Height);
                this.DetectedImages.Add(new Detected(detectedIm, item.ClassName, oriIm, item.Confidence));
            }
            RaisePropertyChanged(nameof(DetectedImages));
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

    public class TestWriterMM : IWriter
    {
        public void PrintText(string text)
        {
        }
    }

    class FileManagerMM: IFileManager
    {
        public bool CheckIfExists(string path) => File.Exists(path);

        public void WriteBytes(string path, byte[] bytes) => File.WriteAllBytes(path, bytes);
    }

    public record Detected
    {
        public ImageSharpImageSource<Rgb24> DetectedImage { get; init; }

        // public Bitmap DetectedImage { get; init; }

        public ImageSharpImageSource<Rgb24> OriPic { get; init; }

        // public Bitmap OriPic { get; init; }

        public string ClassName { get; init; }

        public double Confidence { get; init; }

        public Detected(Image<Rgb24> image, int className, Image<Rgb24> oriPic, double confidence)
        {
            DetectedImage = new ImageSharpImageSource<Rgb24>(image);
            OriPic = new ImageSharpImageSource<Rgb24>(oriPic);
            ClassName = ModelManager.labels[className];
            Confidence = confidence;
        }

        public Detected(Image<Rgb24> image, string className, Image<Rgb24> oriPic, double confidence)
        {
            DetectedImage = new ImageSharpImageSource<Rgb24>(image);
            OriPic = new ImageSharpImageSource<Rgb24>(oriPic);
            ClassName = className;
            Confidence = confidence;
        }
    }

    class YOLOVault : Vault
    {
        public override bool CheckFile(string path)
        {
/*            if (File.Exists(path))
            {
                return true;
            }*/
            return false;
        }

        public override void ClearVault()
        {
            File.Delete(this.path);
        }

        public override List<SerializableDetected>? LoadFromVault()
        {
            if (!File.Exists(this.path))
            {
                return null;
            }
            var detected = JsonConvert.DeserializeObject<List<SerializableDetected>>(File.ReadAllText(this.path));
            return detected;
        }

        public override void UpdateVault(List<SerializableDetected> lst)
        {
            if (!File.Exists(this.path))
            {
                File.Create(this.path);
            }
            string filename = Path.GetFileNameWithoutExtension(this.path);
            string dir = Path.GetDirectoryName(this.path);
            //string dir = Environment.CurrentDirectory;
            string final_name = dir + filename + "_backup.json";
            File.Copy(this.path, final_name);
            try
            {
                string s = JsonConvert.SerializeObject(lst);
                using (StreamWriter outputFile = new StreamWriter(this.path))
                {
                    outputFile.WriteLine(s);
                }
            }
            catch(Exception ex)
            {
                File.Copy(final_name, this.path);
                File.Delete(final_name);
            }
            if (File.Exists(final_name))
                File.Delete(final_name);
        }
    }
}