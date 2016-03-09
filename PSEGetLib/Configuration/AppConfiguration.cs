namespace PSEGetLib.Configuration
{
    public enum OutputTo { CSV, Amibroker, Metastock };
    public enum ConvertMethod { DownloadAndConvert, ConvertFromFiles, DownloadHistoricalData };
    public class AppConfiguration
    {
        public double FormTop { get; set; }
        public double FormLeft { get; set; }
        public string OutputFolder { get; set; }
        public string CSVOutputFormat { get; set; }
        public string CSVDateFormat { get; set; }
        public string CSVDelimiter { get; set; }
        public ConvertMethod DataSource { get; set; }
        public bool IndexValueAsVolume { get; set; }
        public OutputTo TargetOutput { get; set; }
        public string ReportUrl { get; set; }
        public bool MetastockSingleDirectory { get; set; }
        public uint IndexValueDivisor { get; set; }
        public string ReportFilenameFormat { get; set; }
    }
}
