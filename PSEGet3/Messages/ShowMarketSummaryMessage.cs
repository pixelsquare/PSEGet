using System;
using GalaSoft.MvvmLight.Messaging;

namespace PSEGet3.Messages
{
    public class ShowMarketSummaryMessage : MessageBase
    {
        public DateTime TradeDate { get; set; }
    }
}