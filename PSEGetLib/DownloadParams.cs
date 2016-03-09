using System;

namespace PSEGetLib
{
    public class DownloadParams : ICloneable
    {
        private string baseURL;
        private string fileName;
        private DateTime fromDate;
        private DateTime toDate;

        public string BaseURL
        {
            get
            {
                return baseURL;
            }
            set
            {
                if (value.Substring(value.Length - 1, 1) != "/")
                    value += "/";
                baseURL = value;
            }
        }

        public string FileName
        {
            get
            {
                return fileName;
            }

            set
            {
                fileName = value;
                if (!fileName.Contains(".pdf"))
                    fileName = fileName + ".pdf";
            }
        }

        public DateTime FromDate
        {
            get { return fromDate; }
            set { fromDate = value; }
        }

        public DateTime ToDate
        {
            get { return toDate; }
            set { toDate = value; }
        }

        public override string ToString()
        {
            return BaseURL + FileName;
        }

        public object Clone()
        {
            DownloadParams downloadParams = new DownloadParams();
            downloadParams.BaseURL = baseURL;
            downloadParams.FromDate = fromDate;
            downloadParams.ToDate = toDate;
            downloadParams.FileName = FileName;

            return downloadParams;
        }
    }
}
