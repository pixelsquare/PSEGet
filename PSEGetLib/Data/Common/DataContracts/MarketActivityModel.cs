using System.Collections.Generic;

namespace PSEGetLib.Data.Common.DataContracts
{    
    public class MarketActivityModel
    {
        public MarketActivityModel()
        {
            BlockSales = new List<BlockSaleModel>();
        }

        public int Advances { get; set; }
        public int Declines { get; set; }
        public int Unchanged { get; set; }
        public int TradedIssues { get; set; }
        public int NumTrades { get; set; }
        public double CompositeValue { get; set; }
        public double TotalForeignBuying { get; set; }
        public double TotalForeignSelling { get; set; }
        public ulong MainBoardCrossVolume { get; set; }
        public ulong MainBoardCrossValue { get; set; }
        public List<BlockSaleModel> BlockSales { get; set; }
    }
}
