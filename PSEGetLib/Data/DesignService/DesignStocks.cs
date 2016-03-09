using System.Collections.Generic;
using PSEGetLib.Data.Common.DataContracts;


namespace PSEGetLib.Data.DesignService
{
    public class DesignStocks : List<Stock>
    {
        public DesignStocks()
        {
            for (int i = 0; i < 9; i++)
            {
                this.Add(CreateDesignStock());
            }
        }
        
        private Stock CreateDesignStock()
        {
            //Random rand = new Random(10);
            var stock = new Stock()
            {
                Symbol = "XXX",
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
