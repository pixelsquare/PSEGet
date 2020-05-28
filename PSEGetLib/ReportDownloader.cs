using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
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
        private int downloadCount = 0;
        private List<string> errorLogs = new List<string>();
        public bool AsyncMode { get; set; }
        public ReportDownloader()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
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

            wc.DownloadFileCompleted += wc_DownloadFileCompleted;
            wc.DownloadProgressChanged += wc_DownloadProgressChange;
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

                errorLogs.Clear();

                if(File.Exists("logs.txt"))
                {
                    errorLogs.AddRange(File.ReadAllLines("logs.txt"));
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

                    bool hasReport = File.Exists(Path.Combine(SavePath, Path.GetFileName(downloadParams.FileName)));

                    if (reportDate.DayOfWeek == DayOfWeek.Saturday ||
                        reportDate.DayOfWeek == DayOfWeek.Sunday || hasReport)
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

                downloadCount = downloadQueue.Count;
                ProcessQueue();
            }
            catch(Exception e)
            {
                OnReportDownloadCompletedEvent?.Invoke(this, new AsyncCompletedEventArgs(e, false, null));
                if (downloadQueue.Count == 0)
                    OnDownloadAllCompletedEvent(this, new EventArgs() );
            }
        }

        private void ProcessQueue()
        {
            if (downloadQueue.Count > 0)
            {
                var downloadParams = (DownloadParams)downloadQueue.Dequeue();
                int count = downloadCount - downloadQueue.Count;

                CurrentDownloadFile = SavePath + Helpers.GetDirectorySeparator() + downloadParams.FileName;

                if (AsyncMode)
                    wc.DownloadFileAsync(new Uri(downloadParams.ToString()), CurrentDownloadFile);
                else
                {
                    try
                    {
                        float progress = count > 0 ? ((float)count / (float)downloadCount) * 100f : 0f;
                        Console.Write($"Downloading {Path.GetFileNameWithoutExtension(CurrentDownloadFile)} [{count}/{downloadCount}] ({progress.ToString("0")}%) ... ");
                        wc.DownloadFile(new Uri(downloadParams.ToString()), CurrentDownloadFile);

                        // since we are in blocking mode we have to set success explicitly
                        DownloadedFiles.Last().Success = true;
                        Console.WriteLine($"SUCCESS!");
                    }
                    catch (WebException ex)
                    {
                        Console.WriteLine($"\nERROR: {ex.Message} ({Path.GetFileNameWithoutExtension(CurrentDownloadFile)})");
                        
                        if(string.IsNullOrEmpty(errorLogs.Where(x => x.Contains(Path.GetFileNameWithoutExtension(CurrentDownloadFile))).FirstOrDefault()))
                        {
                            errorLogs.Add(Path.GetFileNameWithoutExtension(CurrentDownloadFile) + " - " + ex.Message);
                        }
                    }

                    ProcessQueue();
                }
            }
            else
            {
                if (OnDownloadAllCompletedEvent != null)
                {
                    OnDownloadAllCompletedEvent(this, new EventArgs());
                }

                errorLogs = errorLogs.OrderBy(x => x).ToList();
                File.WriteAllLines("logs.txt", errorLogs.ToArray());
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
