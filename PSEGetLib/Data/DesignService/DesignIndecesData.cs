using System.Collections.Generic;
using PSEGetLib.Data.Common.DataContracts;

namespace PSEGetLib.Data.DesignService
{
    public class DesignIndecesData : List<Stock>
    {
        public DesignIndecesData()
        {            
            Add(CreateIndexData("^PSEi"));
            Add(CreateIndexData("^ALLSHARES"));
            Add(CreateIndexData("^FINANCIAL"));
            Add(CreateIndexData("^HOLDING"));
            Add(CreateIndexData("^INDUSTRIAL"));
            Add(CreateIndexData("^MINING-OIL"));
            Add(CreateIndexData("^PROPERTY"));
            Add(CreateIndexData("^SERVICE"));
        }

        private Stock CreateIndexData(string symbol)
        {
            var stock = new Stock()
            {
                Symbol = symbol,
                Open = 12345.67,
                High = 12345.67,
                Low = 12345.67,
                Close = 12345.67,
                NetForeignBS = 12345.67,
                PctChg = 0.1,
                PtsChange = 123.45,
                Volume = 123456789,
                Value = 1234567.99
            };
            return stock;
        }
    }
}
