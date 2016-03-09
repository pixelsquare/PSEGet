using System;
using System.Collections.Generic;
using PSEGetLib.Data.Common.DataContracts;
using PSEGetLib.DocumentModel;

namespace PSEGetLib.Data.Service
{
    public interface IPSEGetDataService
    {
        void GetIndexSummary(
            Action<IEnumerable<Stock>> getMarketSummaryCallback,
            DateTime tradeDate);

        void GetGainers(
            Action<IEnumerable<Stock>> getGainers,
            DateTime tradeDate);

        void GetLosers(
            Action<IEnumerable<Stock>> getLosers,
            DateTime tradeDate);            

        void GetMarketActivity(
            Action<IEnumerable<Stock>> getMarketActivity,
            DateTime tradeDate);        

        void GetForeignBuy(
            Action<IEnumerable<Stock>> getForeignBuy,
            DateTime tradeDate);

        void GetForeignSell(
            Action<IEnumerable<Stock>> getForeignSell,
            DateTime tradeDate);

        void GetStockList(Action<IEnumerable<String>> getStockListCallback);

        void SaveTradeData(PSEDocument pseDocument);

        void DeleteTradeData(DateTime tradeDate);

        void GetAllStocks(Action<IEnumerable<Stock>> getAllStocksCallback, DateTime tradeDate);

        void GetMarketActivity(Action<MarketActivityModel> getMarketActivityCallback, DateTime tradeDate);

        void PurgeData(DateTime beforeDate);

        bool MarketSummaryExist(DateTime tradeDate);
    }
}
