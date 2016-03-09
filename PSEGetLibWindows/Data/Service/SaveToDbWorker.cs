using System;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using PSEGetLib.DocumentModel;

namespace PSEGetLib.Data.Service
{
    public class SaveToDbWorker : ISaveToDbWorker
    {
        private readonly SQLiteConnection DbConnection;
        private SQLiteTransaction DbTransaction;
        private double pseiValue;

        public SaveToDbWorker(SQLiteConnection dbConnection)
        {
            DbConnection = dbConnection;
        }

        public void Execute(PSEDocument pseDocument)
        {
            pseiValue = (from q in pseDocument.Sectors
                where q.Symbol == "^PSEi"
                select q).FirstOrDefault().Value;

            DbConnection.Open();
            try
            {
                DbTransaction = DbConnection.BeginTransaction();
                try
                {
                    DeleteTradeData(pseDocument.TradeDate);

                    foreach (SectorItem sectorItem in pseDocument.Sectors)
                    {
                        DoDbInsert(sectorItem);
                        foreach (SubSectorItem subSectorItem in sectorItem.SubSectors)
                        {
                            foreach (StockItem stockItem in subSectorItem.Stocks)
                            {
                                DoDbInsert(stockItem);
                            }
                        }
                    }

                    InsertMiscData(pseDocument);

                    DbTransaction.Commit();
                }
                catch
                {
                    DbTransaction.Rollback();
                    throw;
                }
            }
            finally
            {
                DbConnection.Close();
            }
        }

        private void InsertMiscData(PSEDocument pseDocument)
        {
            string sql =
                "INSERT INTO MISCTAB VALUES(NULL, @TRADEDATE, @ADVANCE, @DECLINE, @UNCHANGED, @TRADEDISSUES, @NUMTRADES, @FOREIGNBUY, @FOREIGNSELL, @CROSSVOLUME, @CROSSVALUE, @BLOCKSALES)";
            var cmd = new SQLiteCommand(sql, DbConnection, DbTransaction);
            cmd.Parameters.Add("@TRADEDATE", DbType.String).Value = pseDocument.TradeDate.ToString("yyyy-MM-dd");
            cmd.Parameters.Add("@ADVANCE", DbType.Int32).Value = pseDocument.NumAdvance;
            cmd.Parameters.Add("@DECLINE", DbType.Int32).Value = pseDocument.NumDeclines;
            cmd.Parameters.Add("@UNCHANGED", DbType.Int32).Value = pseDocument.NumUnchanged;
            cmd.Parameters.Add("@TRADEDISSUES", DbType.Int32).Value = pseDocument.NumTraded;
            cmd.Parameters.Add("@NUMTRADES", DbType.Int32).Value = pseDocument.NumTrades;
            cmd.Parameters.Add("@FOREIGNBUY", DbType.UInt64).Value = Math.Truncate(pseDocument.TotalForeignBuying);
            cmd.Parameters.Add("@FOREIGNSELL", DbType.UInt64).Value = Math.Truncate(pseDocument.TotalForeignSelling);
            cmd.Parameters.Add("@CROSSVOLUME", DbType.UInt64).Value = pseDocument.MainCrossVolume;
            cmd.Parameters.Add("@CROSSVALUE", DbType.UInt64).Value = Math.Truncate(pseDocument.MainCrossValue);
            cmd.Parameters.Add("@BLOCKSALES", DbType.String).Value = pseDocument.BlockSales;
            cmd.ExecuteNonQuery();
        }

        private void DeleteTradeData(DateTime tradeDate)
        {
            var cmd = new SQLiteCommand("DELETE FROM TRADETAB WHERE TRADEDATE = ?", DbConnection, DbTransaction);
            cmd.Parameters.Add("TRADEDATE", DbType.String).Value = tradeDate.ToString("yyyy-MM-dd");
            cmd.ExecuteNonQuery();
        }

        private int getSectorId(string sectorSymbol)
        {
            var cmd = new SQLiteCommand("SELECT SECTORID FROM SECTORS WHERE SYMBOL = ?", DbConnection, DbTransaction);
            cmd.Parameters.Add("@SYMBOL", DbType.String).Value = sectorSymbol;
            object result = cmd.ExecuteScalar();
            if (result == null)
            {
                // insert new sector in table
                cmd.CommandText = "SELECT COUNT(*) + 1 FROM SECTORS";
                result = cmd.ExecuteScalar();

                cmd.CommandText = "INSERT INTO SECTORS VALUES(@SECTORID, @SYMBOL)";
                cmd.Parameters.Add("@SECTORID", DbType.Int32).Value = Convert.ToInt32(result.ToString());
                cmd.Parameters.Add("@SYMBOL", DbType.String).Value = sectorSymbol;
                cmd.ExecuteNonQuery();
            }
            return Convert.ToInt32(result.ToString());
        }

        private void UpdateOrInsertStockData(StockItem stockItem)
        {
            string sql = "SELECT COUNT(*) FROM STOCKS WHERE SYMBOL = ?";
            var cmd = new SQLiteCommand(sql, DbConnection, DbTransaction);
            cmd.Parameters.Add("@SYMBOL", DbType.String).Value = stockItem.Symbol;

            object result = cmd.ExecuteScalar();
            if (result.ToString() == "0")
            {
                cmd.CommandText = "INSERT INTO STOCKS VALUES(@SYMBOL, @DESCRIPTION)";
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@SYMBOL", DbType.String).Value = stockItem.Symbol;
                cmd.Parameters.Add("@DESCRIPTION", DbType.String).Value = stockItem.Description;
                cmd.ExecuteNonQuery();
            }
        }

        private void DoDbInsert(StockBase stockBase)
        {
            string sql =
                "INSERT INTO TRADETAB VALUES (NULL, @TRADEDATE, @SYMBOL, @OPEN, @HIGH, @LOW, @CLOSE, @VOLUME, @VALUE, @NFB, @MKTPCT, @SECTORID)";
            var cmd = new SQLiteCommand(sql, DbConnection, DbTransaction);
            cmd.Parameters.Add("@TRADEDATE", DbType.String).Value =
                stockBase.OwnerDocument.TradeDate.ToString("yyyy-MM-dd");
            cmd.Parameters.Add("@SYMBOL", DbType.String).Value = stockBase.Symbol;
            cmd.Parameters.Add("@OPEN", DbType.Double).Value = stockBase.Open;
            cmd.Parameters.Add("@HIGH", DbType.Double).Value = stockBase.High;
            cmd.Parameters.Add("@LOW", DbType.Double).Value = stockBase.Low;
            cmd.Parameters.Add("@CLOSE", DbType.Double).Value = stockBase.Close;
            cmd.Parameters.Add("@VOLUME", DbType.Double).Value = stockBase.Volume;
            cmd.Parameters.Add("@VALUE", DbType.Double).Value = stockBase.Value;
            cmd.Parameters.Add("@NFB", DbType.Double).Value = stockBase.NetForeignBuy;
            cmd.Parameters.Add("@MKTPCT", DbType.Double).Value = (stockBase.Value/pseiValue)*100;

            if (stockBase is StockItem)
            {
                cmd.Parameters.Add("@SECTORID", DbType.Int32).Value =
                    getSectorId(((StockItem) stockBase).OwnerSector.Symbol);
                UpdateOrInsertStockData(stockBase as StockItem);
            }
            else
            {
                cmd.Parameters.Add("@SECTORID", DbType.Object).Value = getSectorId(stockBase.Symbol);
            }
            cmd.ExecuteNonQuery();
        }
    }
}