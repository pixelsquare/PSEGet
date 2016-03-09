using System.Collections.Generic;
using GalaSoft.MvvmLight.Messaging;

namespace PSEGet3.Messages
{
    public class DownloadHistoricalDataParamMessage : MessageBase
    {
        public bool IsSingleFile = false;
        public int NumYears;
        public IEnumerable<string> StockList;
    }
}