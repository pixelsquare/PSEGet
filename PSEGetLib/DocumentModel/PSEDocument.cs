using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Specialized;
using System.Collections;
using PSEGetLib.Converters;

namespace PSEGetLib.DocumentModel
{
    public class PSEDocument
    {     
        // constants
        public const string FINANCIAL = "^FINANCIAL";
        public const string INDUSTRIAL = "^INDUSTRIAL";
        public const string HOLDING = "^HOLDING";
        public const string ALLSHARES = "^ALLSHARES";
        public const string MINING_OIL = "^MINING-OIL";
        public const string PROPERTY = "^PROPERTY";
        public const string SERVICE = "^SERVICE";
        public const string PSEI = "^PSEi";
        public const string PREFERRED = "^PREFERRED";
        public const string WARRANT = "^WARRANT";
        public const string SME = "^SME";
        public const string DEPOSITORY_RECEIPTS = "^DEPOSITARY";
        public const string ETF = "^ETF";
        public const string DOLLAR_DONOMINATED_SECURITIES = "^DDS";		       

        public PSEDocument()
        {
            Sectors = new List<SectorItem>();                      
        }

        public void ToCSV(CSVOutputSettings csvOutputSettings)
        {
            Converter.Convert<CSVOutputSettings>(this, csvOutputSettings);
        }

        public void ToAmibroker(AmiOutputSettings amiOutputSettings)
        {
            Converter.Convert<AmiOutputSettings>(this, amiOutputSettings);
        }

        public void ToMetaStock(MetaOutputSettings metaOutputSettings)
        {
            Converter.Convert<MetaOutputSettings>(this, metaOutputSettings);
        }	       

        public SectorItem GetSector(String symbol)
        {
            var sectorItem = (from s in this.Sectors 
                              where s.Symbol == symbol select s)
                              .SingleOrDefault();            
            if (sectorItem == null)
                throw new Exception(symbol + " not found");

            return sectorItem;
        }

        public StockItem GetStock(string symbol)
        {
            foreach (SectorItem sector in Sectors)
            {
                foreach (SubSectorItem subSector in sector.SubSectors)
                {
                    foreach (StockItem stock in subSector.Stocks)
                    {
                        if (stock.Symbol == symbol)
                        {
                            return stock;
                        }
                    }
                }
            }

            return null;
        }

        public List<SectorItem> Sectors
        {
            get;
            set;
        }

        public DateTime TradeDate
        {
			get;
			set;
        }

        public int NumAdvance
        {
            get;
            set;
        }

        public int NumDeclines
        {
            get;
            set;
        }

        public int NumUnchanged
        {
            get;
            set;
        }

        public int NumTraded
        {
            get; set;
        }

        public int NumTrades
        {
            get; set;
        }

        public ulong OddLotVolume
        {
            get;
            set;
        }

        public double OddLotValue
        {
            get;
            set;
        }

        public ulong BlockSaleVolume
        {
            get; set;
        }

        public double BlockSaleValue
        {
            get; set;
        }

        public ulong MainCrossVolume
        {
            get;
            set;
        }

        public double MainCrossValue
        {
            get;
            set;
        }

        public double BondsVolume
        {
            get;
            set;
        }

        public double BondsValue
        {
            get;
            set;
        }

        public string ExchangeNotice
        {
            get;
            set;
        }

        public double TotalForeignBuying
        {
            get;
            set;
        }

        public double TotalForeignSelling
        {
            get;
            set;
        }

        public double NetForeignBuying
        {
            get
            {
                return this.TotalForeignBuying - this.TotalForeignSelling;
            }            
        }

        public string BlockSales
        {
            get;
            set;
        }
    }
}
