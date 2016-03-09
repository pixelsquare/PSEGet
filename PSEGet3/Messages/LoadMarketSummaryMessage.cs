using System;
using GalaSoft.MvvmLight.Messaging;

namespace PSEGet3.Messages
{
    public class LoadMarketSummaryMessage : MessageBase
    {
        public DateTime TradeDate { get; set; }
    }
}