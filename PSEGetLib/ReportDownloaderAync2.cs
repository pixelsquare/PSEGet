using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSEGetLib.Interfaces;
using System.Net;
using System.IO;

namespace PSEGetLib.Service
{
    public class ReportDownloaderAsync : IReportDownloader
    {
        public DownloadParams DownloadParams
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public string SavePath
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public void Download()
        {
            throw new NotImplementedException();
        }

        private async Task<byte[]> DownloadData(string url)
        {
            var tcs = new TaskCompletionSource<byte[]>();
            HttpWebRequest request = WebRequest.CreateHttp(url);
            try
            {
                HttpWebResponse response = (HttpWebResponse)(await request.GetResponseAsync());
                Stream stream = response.GetResponseStream();
                using (MemoryStream ms = new MemoryStream())
                {
                    await stream.CopyToAsync(ms);
                    tcs.SetResult(ms.ToArray());
                }
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
            return await tcs.Task;
        }

    }
}