using System;
using System.Collections.Generic;
using PSEGetLib.Data.Service;
using PSEGetLib.Data.Common.DataContracts;
using PSEGetLib.DocumentModel;

namespace PSEGetLib.Data.DesignService
{
    public class MarketSummaryDesign : IPSEGetDataService
    {
        public void GetIndexSummary(Action<IEnumerable<Stock>> getMarketSummaryCallback, DateTime tradeDate)
        {
           // getMarketSummaryCallback(new DesignStocks());
        }

        public void GetGainers(Action<IEnumerable<Stock>> getGainers, DateTime tradeDate)
        {
            throw new NotImplementedException();
        }

        public void GetLosers(Action<IEnumerable<Stock>> getLosers, DateTime tradeDate)
        {
            throw new NotImplementedException();
        }


        public void GetMarketActivity(Action<IEnumerable<Stock>> getMarketActivity, DateTime tradeDate)
        {
            throw new NotImplementedException();
        }

        public void GetForeignBuy(Action<IEnumerable<Stock>> getForeignBuy, DateTime tradeDate)
        {
            throw new NotImplementedException();
        }

        public void GetForeignSell(Action<IEnumerable<Stock>> getForeignSell, DateTime tradeDate)
        {
            throw new NotImplementedException();
        }

        public void GetStockList(Action<IEnumerable<String>> getStockListCallback)
        {
            List<string> stockList = new List<string>();
            stockList.Add("MEG");
            stockList.Add("FLI");
            stockList.Add("AC");
            stockList.Add("TEL");
            stockList.Add("GLO");
            stockList.Add("GEO");
        }

        public void SaveTradeData(PSEDocument pseDocument)
        {
        }

        public void DeleteTradeData(DateTime tradeDate)
        {
        }


        public void GetAllStocks(Action<IEnumerable<Stock>> getAllStocksCallback, DateTime tradeDate)
        {
            throw new NotImplementedException();
        }

        public void GetMarketActivity(Action<MarketActivityModel> getMarketActivityCallback, DateTime tradeDate)
        {
        }


        public void PurgeData(DateTime beforeDate)
        {
            throw new NotImplementedException();
        }

        public bool MarketSummaryExist(DateTime tradeDate)
        {
            return false;
        }
    }
}
