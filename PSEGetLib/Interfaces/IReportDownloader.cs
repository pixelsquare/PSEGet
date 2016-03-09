namespace PSEGetLib.Interfaces
{
    public interface IReportDownloader
    {
        DownloadParams DownloadParams { get; set; }
        //List<string> DownloadedFiles { get; set; }
        string SavePath { get; set; }
        void Download();
    }
}
