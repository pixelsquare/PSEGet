using System;
using GalaSoft.MvvmLight.Messaging;

namespace PSEGet3.Messages
{
    public class DownloadAndConvertParamMessage : MessageBase
    {
        public Uri DownloadURI { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}