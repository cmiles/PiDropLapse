namespace PiDropSimpleSensorReport
{
    public class PiDropSimpleSensorReportSettings
    {
        public string DropboxAccessToken { get; set; } = string.Empty;
        public string FileIdentifierName { get; set; } = "Sensor";
        public bool UseBmp280Sensor { get; set; } = true;
    }
}