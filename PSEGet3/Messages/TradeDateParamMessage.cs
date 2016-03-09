using System;
using GalaSoft.MvvmLight.Messaging;

namespace PSEGet3.Messages
{
    public class TradeDateParamMessage : MessageBase
    {
        public TradeDateParamMessage(DateTime tradeDate)
        {
            TradeDate = tradeDate;
        }

        public DateTime TradeDate { get; set; }
    }
}