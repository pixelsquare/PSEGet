using System;
using PSEGetLib.Converters;
using PSEGetLib.DocumentModel;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading;
using System.Linq;

namespace PSEGetLib
{
    public class DownloadAndConvertParams
    {
        public Uri DownloadUri;
        public DateTime FromDate; 
        public DateTime ToDate;
        public string SavePath;
        public OutputSettings OutputSettings;
        public AsyncCompletedEventHandler ReportDownloadedHandler; 
        public EventHandler DownloadAllCompletedHandler;
        public Action<object, PSEDocument> ConvertCompleteCallback;
        public EventHandler OnStartDownloadProcess;
        public DownloadProgressChangedEventHandler DownloadProgressHandler;
        public Action<string> BeforeConvertCallback;
        public Action<PSEDocument> ProcessCompletedCallback;
        public Action<object, Exception> ConvertErrorHandler;
        public Thread threadObject;
    }

    public static class DownloadAndConvertHelper
    {
        private static OutputSettings _outputSettings;
        private static Action<string> BeforeConvertCallback;
        private static Action<object, PSEDocument> ConvertCompleteCallback;
        private static PSEDocument document = null;
        private static Action<PSEDocument> ProcessCompletedCallback;
        private static Action<object, Exception> ConvertErrorHandler;

        public static void DownloadAndConvert(object downloadAndConvertObj)
        {
            var downloadAndConvertParams = (DownloadAndConvertParams)downloadAndConvertObj;
            DownloadAndConvertHelper.DownloadAndConvertAsync(downloadAndConvertParams);
        }        

        public static void DownloadAndConvertAsync(DownloadAndConvertParams downloadAndConvertParams)
        {
            var param = new DownloadParams();
            document = null;

            param.BaseURL = downloadAndConvertParams.DownloadUri.ToString();
            param.FileName = "stockQuotes_%mm%dd%yyyy";
            param.FromDate = downloadAndConvertParams.FromDate;
            param.ToDate = downloadAndConvertParams.ToDate;

            DownloadAndConvertHelper._outputSettings = downloadAndConvertParams.OutputSettings;
            ConvertCompleteCallback = downloadAndConvertParams.ConvertCompleteCallback;
            BeforeConvertCallback = downloadAndConvertParams.BeforeConvertCallback;
            ProcessCompletedCallback = downloadAndConvertParams.ProcessCompletedCallback;
            ConvertErrorHandler = downloadAndConvertParams.ConvertErrorHandler;

            var downloader = new ReportDownloader(param, downloadAndConvertParams.SavePath, ProcessDownloadedFile);
            downloader.OnReportDownloadCompletedEvent += downloadAndConvertParams.ReportDownloadedHandler;
            downloader.OnDownloadAllCompletedEvent += downloadAndConvertParams.DownloadAllCompletedHandler;
            downloader.OnDownloadAllCompletedEvent += (s, e) => {
                downloadAndConvertParams.ProcessCompletedCallback(document);
            };

            downloader.OnStartDownloadProcess += downloadAndConvertParams.OnStartDownloadProcess;
            downloader.OnDownloadProgressEvent += downloadAndConvertParams.DownloadProgressHandler;
            downloader.Download();
        }

        private static void ProcessDownloadedFile(object sender, AsyncCompletedEventArgs e)
        {
            ReportDownloader downloader = sender as ReportDownloader;
            if (e.Error != null)
            {
                if (downloader.CurrentDownloadFile != null)
                {                    
                    File.Delete(downloader.CurrentDownloadFile);                    
                }
            }
            else
            {
                // convert the downloaded files
                //foreach (var downloadedFile in downloader.DownloadedFiles)
                //for (int i = 0; i < downloader.DownloadedFiles.Count; i++)
                //{
                if (downloader.DownloadedFiles.Count == 0)
                    return;

                var downloadedFile = (from x in downloader.DownloadedFiles 
                                        where x.Filename == downloader.CurrentDownloadFile
                                            select x).First(); //DownloadedFiles[i];                    

                try
                {
                    if (downloadedFile == null)
                        throw new Exception("Unable to find file to convert.");

                    BeforeConvertCallback(downloadedFile.Filename);
                    document = Helpers.ConvertReportFile(downloadedFile.Filename, DownloadAndConvertHelper._outputSettings);
                    downloadedFile.Converted = true;
                    ConvertCompleteCallback(downloader, document);
                }
                catch (Exception ex)
                {
                    downloadedFile.Converted = false;
                    ConvertErrorHandler(downloader, ex);
                }
                //}
            }
        }
    }
}
