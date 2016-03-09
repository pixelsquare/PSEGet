using System.Collections.Generic;

namespace PSEGetLib.DocumentModel
{
    // sub sectors contains the stock list
    public class SubSectorItem
    {
        public string Name { get; set; }
        public List<StockItem> Stocks { get; set; }

        public SubSectorItem()
        {
            Stocks = new List<StockItem>();
        }
    }
}
