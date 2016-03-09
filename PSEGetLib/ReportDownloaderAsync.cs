using System;
using System.Collections.Generic;
using PSEGetLib.Interfaces;
using System.Collections;
using PSEGetLib.Types;
using System.Net;

namespace PSEGetLib
{
    public class ReportDownloaderAsync : IReportDownloader
    {
        public ReportDownloaderAsync()
        {
            DownloadedFiles = new List<string>();
        }

        public ReportDownloaderAsync(DownloadParams downloadParams)
            : base()
        {
            DownloadParams = downloadParams;
        }

        public DownloadParams DownloadParams { get; set; }
        public List<string> DownloadedFiles { get; set; }
        public string SavePath { get; set; }

        public void Download()
        {
            if (DownloadParams.ToDate < DownloadParams.FromDate)
                throw new PSEGetException("Invalid date range.");

            DownloadedFiles.Clear();
            downloadQueue.Clear();                       

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
               
            }
            else
            {
                throw new PSEGetException("Nothing to download");
            }

            ProcessQueue();
        }

        private Queue downloadQueue = new Queue();
        private void ProcessQueue()
        {
            var client = new WebClient();
            var downloadParams = (DownloadParams)downloadQueue.Dequeue();
            var currentDownloadFile = SavePath + Helpers.GetDirectorySeparator() + downloadParams.FileName;
            

        }
    }
}
