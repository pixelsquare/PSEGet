using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Net;
using System.ComponentModel;
using PSEGetLib.Interfaces;
using PSEGetLib.Types;

namespace PSEGetLib
{
    public class DownloadFileInfo
    {
        public string Filename = "";
        public bool Success = false;
        public bool Converted = false;
    }

    public class ReportDownloader : IReportDownloader
    {        
        private Queue downloadQueue = new Queue();
        private WebClient wc = new WebClient();
        private Action<object, string> OnProgressEvent;
        private Action<Exception> OnExceptionEvent;
        private Action OnDownloadCompleteEvent;
        public bool AsyncMode { get; set; }
        public ReportDownloader()
        {
            DownloadedFiles = new List<DownloadFileInfo>();
            AsyncMode = true;
            //FailedDownloadFiles = new List<string>();
        }

        public ReportDownloader(DownloadParams downloadParams, string savePath, AsyncCompletedEventHandler downloadCompletedEvent)
            : this()
        {            
            DownloadParams = downloadParams;
            SavePath = savePath;
            OnReportDownloadCompletedEvent = downloadCompletedEvent;

            wc.DownloadFileCompleted += new AsyncCompletedEventHandler(wc_DownloadFileCompleted);
            wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(wc_DownloadProgressChange);
        }

        public ReportDownloader(DownloadParams downloadParams, string savePath,
            Action<object, string> progressCallback, Action<Exception> exceptionCallback, Action downloadCompleteCallback)
        {
            DownloadParams = downloadParams;
            SavePath = savePath;
            OnProgressEvent = progressCallback;
            OnExceptionEvent = exceptionCallback;
            OnDownloadCompleteEvent = downloadCompleteCallback;
        }

        public DownloadParams DownloadParams
        {
            get;
            set;
        }

        public List<DownloadFileInfo> DownloadedFiles { get; set; }
        //public List<string> FailedDownloadFiles { get; set; }
        public string SavePath
        {
            get;
            set;
        }

        //download completed event
        private void wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            var downloadFileInfo = (from f in DownloadedFiles
                                    where f.Filename == CurrentDownloadFile
                                    select f).Single();
            downloadFileInfo.Success = e.Error == null;

            if (OnReportDownloadCompletedEvent != null)
            {
                OnReportDownloadCompletedEvent(this, e);
            }
                
            // download next in queue
            ProcessQueue();
        }

        //download progress
        private void wc_DownloadProgressChange(object sender, DownloadProgressChangedEventArgs e)
        {
            if (OnDownloadProgressEvent != null)
            {
                OnDownloadProgressEvent(CurrentDownloadFile, e);
            }
        }                

        public void Download()
        {
            try
            {
                if (DownloadParams.ToDate < DownloadParams.FromDate)
                {
                    throw new PSEGetException("Invalid date range.");               
                }

                DownloadedFiles.Clear();
                //this.FailedDownloadFiles.Clear();
                DateTime reportDate = DownloadParams.FromDate;
                do
                {
                    var downloadParams = (DownloadParams)DownloadParams.Clone();
                    downloadParams.FileName = downloadParams.FileName.Replace("%dd", String.Format("{0:00}", reportDate.Day));
                    downloadParams.FileName = downloadParams.FileName.Replace("%mm", String.Format("{0:00}", reportDate.Month));
                    downloadParams.FileName = downloadParams.FileName.Replace("%yyyy", String.Format("{0:00}", reportDate.Year));

                    if (reportDate.DayOfWeek == DayOfWeek.Saturday ||
                        reportDate.DayOfWeek == DayOfWeek.Sunday)
                    {
                        reportDate = reportDate.AddDays(1);
                        continue;
                    }

                    downloadQueue.Enqueue(downloadParams);
                    reportDate = reportDate.AddDays(1);

                } while (reportDate <= DownloadParams.ToDate);

                if (downloadQueue.Count > 0)
                {
                    if (OnStartDownloadProcess != null)
                        OnStartDownloadProcess(this, null);
                }
                else
                {                    
                    throw new PSEGetException("There were no pse reports found in the dates you specified.");
                }            
                ProcessQueue();
            }
            catch(Exception e)
            {
                OnReportDownloadCompletedEvent(this, new AsyncCompletedEventArgs(e, false, null));
                if (downloadQueue.Count == 0)
                    OnDownloadAllCompletedEvent(this, new EventArgs() );
            }
        }

        private void ProcessQueue()
        {
            if (downloadQueue.Count > 0)
            {
                var downloadParams = (DownloadParams)downloadQueue.Dequeue();
                CurrentDownloadFile = SavePath + Helpers.GetDirectorySeparator() + downloadParams.FileName;
                if (AsyncMode)
                    wc.DownloadFileAsync(new Uri(downloadParams.ToString()), CurrentDownloadFile);
                else
                    wc.DownloadFile(new Uri(downloadParams.ToString()), CurrentDownloadFile);
            }
            else
            {
                if (OnDownloadAllCompletedEvent != null)
                {
                    OnDownloadAllCompletedEvent(this, new EventArgs());
                }
            }
        }

        public void Download(DownloadParams downloadParams)
        {
            DownloadParams = downloadParams;
            Download();
        }

        public AsyncCompletedEventHandler OnReportDownloadCompletedEvent
        {
            get;
            set;
        }

        public DownloadProgressChangedEventHandler OnDownloadProgressEvent
        {
            get;
            set;
        }

        public EventHandler OnDownloadAllCompletedEvent
        {
            get;
            set;
        }

        public EventHandler OnStartDownloadProcess
        {
            get;
            set;
        }

        private string _currentDownloadFile;
        public string CurrentDownloadFile 
        { 
            get
            {
                return _currentDownloadFile;
            }

            set 
            {
                _currentDownloadFile = value;

                var downloadFileInfo = new DownloadFileInfo();
                downloadFileInfo.Filename = value;
                DownloadedFiles.Add(downloadFileInfo);
            }
        }

        public bool More { get; set; }
    }
}
