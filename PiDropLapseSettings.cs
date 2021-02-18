namespace PiDropLapse
{
    public class PiDropLapseSettings
    {
        public string DropboxAccessToken { get; set; } = string.Empty;
        public int ExposureCompensation { get; set; }
        public string FileIdentifierName { get; set; } = "Drop";
        public int Iso { get; set; } = 0;
        public int LongEdgeResolution { get; set; } = 1280;
        public int Rotation { get; set; } = 0;
        public int ShutterSpeed { get; set; } = 0;
    }
}