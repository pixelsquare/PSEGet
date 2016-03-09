using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using PSEGetLib.Data.Common.DataContracts;
using PSEGetLib.DocumentModel;

//using System.Windows.Forms;

namespace PSEGetLib.Data.Service
{
    public class PSEGetDataService : IPSEGetDataService
    {
        private readonly SQLiteConnection DbConnection = new SQLiteConnection();

        public PSEGetDataService()
        {
            // Context = new psegetEntities();
            string connString = "data source=" + Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) +
                                "\\pseget3.dat";
            DbConnection.ConnectionString = connString;
        }

        public void GetIndexSummary(Action<IEnumerable<Stock>> getMarketSummaryCallback, DateTime tradeDate)
        {
            DbConnection.Open();
            try
            {
                string sql = @"SELECT C.*, (SELECT D.CLOSE FROM TRADETAB D 
                             WHERE D.SYMBOL = C.SYMBOL AND D.TRADEDATE < C.TRADEDATE 
                             ORDER BY D.TRADEDATE DESC LIMIT 1) AS PREVIOUSCLOSE,
	                    ((
		                (C.CLOSE - (SELECT A.CLOSE FROM TRADETAB A WHERE A.SYMBOL = C.SYMBOL AND A.TRADEDATE < C.TRADEDATE ORDER BY A.TRADEDATE DESC LIMIT 1) ) 
			                /
         	                (SELECT B.CLOSE FROM TRADETAB B WHERE B.SYMBOL = C.SYMBOL AND B.TRADEDATE < C.TRADEDATE ORDER BY B.TRADEDATE DESC LIMIT 1)) * 100
                         ) AS PCTCHG 
                         FROM TRADETAB C WHERE SYMBOL LIKE '^%'
                         AND TRADEDATE = ? AND NOT SYMBOL IN ('^WARRANT', '^SME', '^PREFERRED', '^DEPOSITARY', '^ETF') ";

                var cmd = new SQLiteCommand(sql, DbConnection);
                cmd.Parameters.Add("TRADEDATE", DbType.String).Value = tradeDate.ToString("yyyy-MM-dd");

                SQLiteDataReader reader = cmd.ExecuteReader();
                var l = new List<Stock>();

                while (reader.Read())
                {
                    var marketItem = new Stock();

                    double previousClose = 0;
                    marketItem.Close = Convert.ToDouble(reader["CLOSE"]);
                    if (double.TryParse(reader["PREVIOUSCLOSE"].ToString(), out previousClose))
                        marketItem.PtsChange = marketItem.Close - previousClose;
                    else
                        marketItem.PtsChange = 0;

                    marketItem.Symbol = reader["SYMBOL"].ToString();
                    marketItem.Open = Convert.ToDouble(reader["OPEN"]);
                    marketItem.High = Convert.ToDouble(reader["HIGH"]);
                    marketItem.Low = Convert.ToDouble(reader["LOW"]);
                    marketItem.Volume = Convert.ToUInt64(reader["VOLUME"]);
                    marketItem.PctChg = Math.Round(Convert.ToDouble(reader["PCTCHG"]), 2);
                    if (marketItem.Symbol == "^PSEi")
                    {
                        marketItem.NetForeignBS = GetPSENetforeignBuy(tradeDate);
                    }
                    else
                        marketItem.NetForeignBS = Math.Truncate(Convert.ToDouble(reader["NETFOREIGNBUY"])/1000);
                    l.Add(marketItem);
                }
                getMarketSummaryCallback(l);
            }
            finally
            {
                DbConnection.Close();
            }
        }

        public void GetGainers(Action<IEnumerable<Stock>> getGainers, DateTime tradeDate)
        {
            string sql = @"SELECT C.*, (SELECT D.CLOSE FROM TRADETAB D 
                             WHERE D.SYMBOL = C.SYMBOL AND D.TRADEDATE < C.TRADEDATE 
                             ORDER BY D.TRADEDATE DESC LIMIT 1) AS PREVIOUSCLOSE,
	                    ((
		                (C.CLOSE - (SELECT A.CLOSE FROM TRADETAB A WHERE A.SYMBOL = C.SYMBOL AND A.TRADEDATE < C.TRADEDATE ORDER BY A.TRADEDATE DESC LIMIT 1) ) 
			                /
         	                (SELECT B.CLOSE FROM TRADETAB B WHERE B.SYMBOL = C.SYMBOL AND B.TRADEDATE < C.TRADEDATE ORDER BY B.TRADEDATE DESC LIMIT 1)) * 100
                         ) AS PCTCHG 
                         FROM TRADETAB C WHERE C.TRADEDATE = ? AND C.SYMBOL NOT LIKE '^%' AND C.CLOSE <> 0 
                         AND PCTCHG > 0 ORDER BY PCTCHG DESC LIMIT 10 OFFSET 0";
            DbConnection.Open();
            try
            {
                var cmd = new SQLiteCommand(sql, DbConnection);
                cmd.Parameters.Add("@TRADEDATE", DbType.String).Value = tradeDate.ToString("yyyy-MM-dd");
                SQLiteDataReader reader = cmd.ExecuteReader();
                var result = new List<Stock>();
                while (reader.Read())
                {
                    var stock = new Stock();

                    double previousClose = 0;
                    stock.Close = Convert.ToDouble(reader["CLOSE"]);

                    if (double.TryParse(reader["PREVIOUSCLOSE"].ToString(), out previousClose))
                        stock.PtsChange = stock.Close - previousClose;
                    else
                        stock.PtsChange = 0;
                    stock.Symbol = reader["SYMBOL"].ToString();
                    stock.Open = Convert.ToDouble(reader["OPEN"]);
                    stock.High = Convert.ToDouble(reader["HIGH"]);
                    stock.Low = Convert.ToDouble(reader["LOW"]);

                    stock.Volume = Convert.ToUInt64(reader["VOLUME"]);
                    stock.Value = Convert.ToDouble(reader["VALUE"])/1000;
                    if (stock.Value > 1)
                        stock.Value = Math.Truncate(Math.Round(stock.Value, 2));
                    stock.NetForeignBS = Convert.ToDouble(reader["NETFOREIGNBUY"]);
                    stock.PctChg = Math.Round(Convert.ToDouble(reader["PCTCHG"]), 2);
                        //((stock.Close - previousClose) / previousClose) * 100;

                    result.Add(stock);
                }
                getGainers(result);
            }
            finally
            {
                DbConnection.Close();
            }
        }

        public void GetLosers(Action<IEnumerable<Stock>> getLosers, DateTime tradeDate)
        {
            string sql = @"SELECT C.*, (SELECT D.CLOSE FROM TRADETAB D 
                             WHERE D.SYMBOL = C.SYMBOL AND D.TRADEDATE < C.TRADEDATE 
                             ORDER BY D.TRADEDATE DESC LIMIT 1) AS PREVIOUSCLOSE,
	                    ((
		                (C.CLOSE - (SELECT A.CLOSE FROM TRADETAB A WHERE A.SYMBOL = C.SYMBOL AND A.TRADEDATE < C.TRADEDATE ORDER BY A.TRADEDATE DESC LIMIT 1) ) 
			                /
         	                (SELECT B.CLOSE FROM TRADETAB B WHERE B.SYMBOL = C.SYMBOL AND B.TRADEDATE < C.TRADEDATE ORDER BY B.TRADEDATE DESC LIMIT 1)) * 100
                         ) AS PCTCHG 
                         FROM TRADETAB C WHERE C.TRADEDATE = ? AND C.SYMBOL NOT LIKE '^%' AND C.CLOSE <> 0 
                         AND PCTCHG <= 0 ORDER BY PCTCHG ASC LIMIT 10 OFFSET 0";
            DbConnection.Open();
            try
            {
                var cmd = new SQLiteCommand(sql, DbConnection);
                cmd.Parameters.Add("@TRADEDATE", DbType.String).Value = tradeDate.ToString("yyyy-MM-dd");
                SQLiteDataReader reader = cmd.ExecuteReader();
                var result = new List<Stock>();
                while (reader.Read())
                {
                    var stock = new Stock();

                    double previousClose = 0;
                    stock.Close = Convert.ToDouble(reader["CLOSE"]);
                    if (double.TryParse(reader["PREVIOUSCLOSE"].ToString(), out previousClose))
                        stock.PtsChange = stock.Close - previousClose;
                    else
                        stock.PtsChange = 0;
                    stock.Symbol = reader["SYMBOL"].ToString();
                    stock.Open = Convert.ToDouble(reader["OPEN"]);
                    stock.High = Convert.ToDouble(reader["HIGH"]);
                    stock.Low = Convert.ToDouble(reader["LOW"]);
                    stock.Volume = Convert.ToUInt64(reader["VOLUME"]);
                    stock.Value = Convert.ToDouble(reader["VALUE"])/1000;
                    if (stock.Value > 1)
                        stock.Value = Math.Truncate(Math.Round(stock.Value, 2));
                    stock.NetForeignBS = Convert.ToDouble(reader["NETFOREIGNBUY"]);
                    stock.PctChg = Math.Round(Convert.ToDouble(reader["PCTCHG"]), 2);
                        //((stock.Close - previousClose) / previousClose) * 100;

                    result.Add(stock);
                }
                getLosers(result);
            }
            finally
            {
                DbConnection.Close();
            }
        }

        public void GetMarketActivity(Action<IEnumerable<Stock>> getMarketActivity, DateTime tradeDate)
        {
        }

        public void GetForeignBuy(Action<IEnumerable<Stock>> getForeignBuy, DateTime tradeDate)
        {
            string sql = @"SELECT A.*, (SELECT D.CLOSE FROM TRADETAB D 
                                        WHERE D.SYMBOL = A.SYMBOL AND D.TRADEDATE < A.TRADEDATE 
                                        ORDER BY D.TRADEDATE DESC LIMIT 1) AS PREVIOUSCLOSE
                            FROM TRADETAB A WHERE TRADEDATE = ? AND A.NETFOREIGNBUY > 0
                             AND A.SYMBOL NOT LIKE '^%'
                             ORDER BY A.NETFOREIGNBUY DESC LIMIT 10 OFFSET 0";
            var cmd = new SQLiteCommand(sql, DbConnection);
            cmd.Parameters.Add("@TRADEDATE", DbType.String).Value = tradeDate.ToString("yyyy-MM-dd");

            DbConnection.Open();
            try
            {
                var result = new List<Stock>();
                SQLiteDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var stock = new Stock();

                    double previousClose = 0;
                    stock.Close = Convert.ToDouble(reader["CLOSE"]);
                    if (double.TryParse(reader["PREVIOUSCLOSE"].ToString(), out previousClose))
                        stock.PtsChange = stock.Close - previousClose;
                    else
                        stock.PtsChange = 0;
                    stock.Symbol = reader["SYMBOL"].ToString();
                    stock.Open = Convert.ToDouble(reader["OPEN"]);
                    stock.High = Convert.ToDouble(reader["HIGH"]);
                    stock.Low = Convert.ToDouble(reader["LOW"]);

                    stock.Volume = Convert.ToUInt64(reader["VOLUME"]);
                    stock.Value = Convert.ToDouble(reader["VALUE"])/1000;
                    if (stock.Value > 1)
                        stock.Value = Math.Truncate(Math.Round(stock.Value, 2));
                    stock.NetForeignBS = Math.Round(Convert.ToDouble(reader["NETFOREIGNBUY"])/1000000, 0);
                    //stock.PctChg = Math.Round(Convert.ToDouble(reader["PCTCHG"]), 2); //((stock.Close - previousClose) / previousClose) * 100;
                    stock.MktPercent = Math.Round(Convert.ToDouble(reader["MKTPERCENT"]), 2);
                    result.Add(stock);
                }
                getForeignBuy(result);
            }
            finally
            {
                DbConnection.Close();
            }
        }

        public void GetForeignSell(Action<IEnumerable<Stock>> getForeignSell, DateTime tradeDate)
        {
            string sql = @"SELECT A.*, (SELECT D.CLOSE FROM TRADETAB D 
                                        WHERE D.SYMBOL = A.SYMBOL AND D.TRADEDATE < A.TRADEDATE 
                                        ORDER BY D.TRADEDATE DESC LIMIT 1) AS PREVIOUSCLOSE
                            FROM TRADETAB A WHERE TRADEDATE = ? AND A.NETFOREIGNBUY < 0
                             AND A.SYMBOL NOT LIKE '^%'
                             ORDER BY A.NETFOREIGNBUY ASC LIMIT 10 OFFSET 0";
            var cmd = new SQLiteCommand(sql, DbConnection);
            cmd.Parameters.Add("@TRADEDATE", DbType.String).Value = tradeDate.ToString("yyyy-MM-dd");

            DbConnection.Open();
            try
            {
                var result = new List<Stock>();
                SQLiteDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var stock = new Stock();

                    double previousClose;
                    stock.Close = Convert.ToDouble(reader["CLOSE"]);
                    if (double.TryParse(reader["PREVIOUSCLOSE"].ToString(), out previousClose))
                        stock.PtsChange = stock.Close - previousClose;
                    else
                        stock.PtsChange = 0;
                    stock.Symbol = reader["SYMBOL"].ToString();
                    stock.Open = Convert.ToDouble(reader["OPEN"]);
                    stock.High = Convert.ToDouble(reader["HIGH"]);
                    stock.Low = Convert.ToDouble(reader["LOW"]);

                    stock.Volume = Convert.ToUInt64(reader["VOLUME"]);
                    stock.Value = Convert.ToDouble(reader["VALUE"])/1000;
                    if (stock.Value > 1)
                        stock.Value = Math.Truncate(Math.Round(stock.Value, 2));
                    stock.NetForeignBS = Math.Round(Convert.ToDouble(reader["NETFOREIGNBUY"])/1000000, 0)*-1;
                    stock.MktPercent = Math.Round(Convert.ToDouble(reader["MKTPERCENT"]), 2);
                    result.Add(stock);
                }
                getForeignSell(result);
            }
            finally
            {
                DbConnection.Close();
            }
        }


        public void GetStockList(Action<IEnumerable<String>> getStockListCallback)
        {
            string sql =
                @"SELECT SYMBOL FROM SECTORS WHERE SYMBOL NOT IN ('^SME', '^WARRANT', '^PREFERRED', '^DEPOSITARY', '^ETF') UNION
                           SELECT SYMBOL FROM STOCKS WHERE NOT SYMBOL LIKE '^%' ORDER BY SYMBOL";
            var cmd = new SQLiteCommand(sql, DbConnection);
            DbConnection.Open();
            try
            {
                SQLiteDataReader reader = cmd.ExecuteReader();
                var result = new List<string>();
                while (reader.Read())
                {
                    result.Add(reader[0].ToString());
                }
                getStockListCallback(result);
            }
            finally
            {
                DbConnection.Close();
            }
        }

        public void SaveTradeData(PSEDocument pseDocument)
        {
            var worker = new SaveToDbWorker(DbConnection);
            worker.Execute(pseDocument);
        }

        public void DeleteTradeData(DateTime tradeDate)
        {
            var cmd = new SQLiteCommand("DELETE FROM TRADETAB WHERE TRADEDATE = ?", DbConnection);
            cmd.Parameters.Add("TRADEDATE", DbType.String).Value = tradeDate.ToString("yyyy-MM-dd");
            cmd.ExecuteNonQuery();
        }


        public void GetAllStocks(Action<IEnumerable<Stock>> getAllStocksCallback, DateTime tradeDate)
        {
            string sql = @"SELECT C.*, (SELECT D.CLOSE FROM TRADETAB D 
                             WHERE D.SYMBOL = C.SYMBOL AND D.TRADEDATE < C.TRADEDATE 
                             ORDER BY D.TRADEDATE DESC LIMIT 1) AS PREVIOUSCLOSE,
	                    ((
		                (C.CLOSE - (SELECT A.CLOSE FROM TRADETAB A WHERE A.SYMBOL = C.SYMBOL AND A.TRADEDATE < C.TRADEDATE ORDER BY A.TRADEDATE DESC LIMIT 1) ) 
			                /
         	                (SELECT B.CLOSE FROM TRADETAB B WHERE B.SYMBOL = C.SYMBOL AND B.TRADEDATE < C.TRADEDATE ORDER BY B.TRADEDATE DESC LIMIT 1)) * 100
                         ) AS PCTCHG 
                         FROM TRADETAB C WHERE C.TRADEDATE = ? AND C.SYMBOL NOT LIKE '^%' AND C.CLOSE <> 0  AND C.VALUE > 2000000";

            var cmd = new SQLiteCommand(sql, DbConnection);
            cmd.Parameters.Add("@TRADEDATE", DbType.String).Value = tradeDate.ToString("yyyy-MM-dd");

            DbConnection.Open();
            try
            {
                SQLiteDataReader reader = cmd.ExecuteReader();
                var result = new List<Stock>();
                while (reader.Read())
                {
                    var stock = new Stock();
                    double stockClose = Convert.ToDouble(reader["CLOSE"]);
                    double previousClose;
                    stock.PtsChange = 0;
                    if (double.TryParse(reader["PREVIOUSCLOSE"].ToString(), out previousClose))
                        stock.PtsChange = stockClose - previousClose;

                    stock.Symbol = reader["SYMBOL"].ToString();
                    stock.Volume = Convert.ToUInt64(reader["VOLUME"]);
                    stock.Value = Convert.ToDouble(reader["VALUE"]);
                    if (reader["PCTCHG"].ToString() != "")
                        stock.PctChg = Math.Round(Convert.ToDouble(reader["PCTCHG"]), 2);
                    result.Add(stock);
                }
                getAllStocksCallback(result);
            }
            finally
            {
                DbConnection.Close();
            }
        }

        public void GetMarketActivity(Action<MarketActivityModel> getMarketActivityCallback, DateTime tradeDate)
        {
            string sql = "SELECT * FROM MISCTAB WHERE TRADEDATE = ?";
            var cmd = new SQLiteCommand(sql, DbConnection);
            cmd.Parameters.Add("@TRADEDATE", DbType.String).Value = tradeDate.ToString("yyyy-MM-dd");
            DbConnection.Open();
            try
            {
                SQLiteDataReader reader = cmd.ExecuteReader();
                var marketActivity = new MarketActivityModel();
                if (reader.Read())
                {
                    List<string> l;
                    marketActivity.Advances = Convert.ToInt32(reader["ADVANCE"]);
                    marketActivity.Declines = Convert.ToInt32(reader["DECLINE"]);
                    marketActivity.Unchanged = Convert.ToInt32(reader["UNCHANGED"]);
                    marketActivity.TradedIssues = Convert.ToInt32(reader["TRADEDISSUES"]);
                    marketActivity.NumTrades = Convert.ToInt32(reader["NUMTRADES"]);
                    marketActivity.TotalForeignBuying = Math.Truncate(Convert.ToDouble(reader["FOREIGNBUY"]));
                    marketActivity.TotalForeignSelling = Math.Truncate(Convert.ToDouble(reader["FOREIGNSELL"]));
                    marketActivity.MainBoardCrossVolume = Convert.ToUInt64(reader["CROSSVOLUME"]);
                    marketActivity.MainBoardCrossValue = Convert.ToUInt64(reader["CROSSVALUE"]);
                    marketActivity.CompositeValue = GetPSEiValue(tradeDate);
                    l =
                        reader["BLOCKSALES"].ToString()
                            .Split(new[] {"\n"}, StringSplitOptions.RemoveEmptyEntries)
                            .ToList();
                    foreach (string s in l)
                    {
                        string[] row = s.Split(new[] {' '});
                        var b = new BlockSaleModel();
                        b.Symbol = row[0];
                        b.Price = row[1];
                        b.Volume = row[2];
                        b.Value = row[3];
                        marketActivity.BlockSales.Add(b);
                    }
                }
                getMarketActivityCallback(marketActivity);
            }
            finally
            {
                DbConnection.Close();
            }
        }

        public void PurgeData(DateTime beforeDate)
        {
            string sql = "DELETE FROM TRADETAB WHERE TRADEDATE < ?";
            var cmd = new SQLiteCommand(sql, DbConnection);
            cmd.Parameters.Add("@TRADEDATE", DbType.String).Value = beforeDate.ToString("yyyy-MM-dd");
            DbConnection.Open();
            try
            {
                cmd.ExecuteScalar();

                cmd.CommandText = "VACUUM";
                cmd.ExecuteScalar();
            }
            finally
            {
                DbConnection.Close();
            }
        }

        public bool MarketSummaryExist(DateTime tradeDate)
        {
            string sql = "SELECT COUNT(*) FROM TRADETAB WHERE SYMBOL = '^PSEi' and TRADEDATE = ?";
            var cmd = new SQLiteCommand(sql, DbConnection);
            cmd.Parameters.Add("@TRADEDATE", DbType.String).Value = tradeDate.ToString("yyyy-MM-dd");
            DbConnection.Open();
            try
            {
                object result = cmd.ExecuteScalar();
                int o;
                if (int.TryParse(result.ToString(), out o))
                {
                    return o > 0;
                }
                else
                    return false;
            }
            finally
            {
                DbConnection.Close();
            }
        }

        private double GetPSENetforeignBuy(DateTime tradeDate)
        {
            string sql = "SELECT FOREIGNBUY - FOREIGNSELL FROM MISCTAB WHERE TRADEDATE = ?";
            var cmd = new SQLiteCommand(sql, DbConnection);
            cmd.Parameters.Add("@TRADEDATE", DbType.String).Value = tradeDate.ToString("yyyy-MM-dd");
            object result = cmd.ExecuteScalar();
            if (result != null)
            {
                return Math.Truncate(Convert.ToDouble(result.ToString())/1000);
            }
            return 0;
        }

        private double GetPSEiValue(DateTime tradeDate)
        {
            string sql = "SELECT VALUE FROM TRADETAB WHERE SYMBOL = '^PSEi' AND TRADEDATE = ?";
            var cmd = new SQLiteCommand(sql, DbConnection);
            cmd.Parameters.Add("@TRADEDATE", DbType.String).Value = tradeDate.ToString("yyyy-MM-dd");
            object result = cmd.ExecuteScalar();
            double o;
            if (Double.TryParse(result.ToString(), out o))
            {
                return o;
            }
            return 0;
        }
    }
}