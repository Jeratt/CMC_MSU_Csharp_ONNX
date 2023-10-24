namespace ImageVault
{
    public abstract class Vault
    {
        protected string path;

        public Vault(string path = "vault.json")
        {
            this.path = path;
        }

        public abstract void ClearVault();
        public abstract void UpdateVault(List<SerializableDetected> lst);
        public abstract bool CheckFile(string path);
        public abstract List<SerializableDetected>? LoadFromVault();
    }

    public interface IVaultManager
    {
        void ClearVault();

        void UpdateVault();

        bool CheckFile();
    }

    public record SerializableDetected
    {
        public byte[] DetectedImage { get; init; }

        public byte[] OriPic { get; init; }

        public string ClassName { get; init; }

        public double Confidence { get; init; }

        public double Width { get; init; }

        public double Height { get; init; }

        public SerializableDetected(byte[] detectedImage, byte[] oriImage, string className, double confidence, double width, double height)
        {
            DetectedImage = detectedImage;
            OriPic = oriImage;
            ClassName = className;
            Confidence = confidence;
            Width = width;
            Height = height;
        }
    }
}