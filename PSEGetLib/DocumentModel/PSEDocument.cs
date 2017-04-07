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
        //private delegate void ToCSVDelegate(PSEDocument pseDocument, CSVOutputSettings csvOutputSettings);
        //private delegate void ToAmibrokerDelegate(PSEDocument pseDocument, AmiOutputSettings amiOutputSettings);
        //private delegate void ToMetaStockDelegate(PSEDocument pseDocument, MetaOutputSettings metaOutputSettings);

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

        private Hashtable _sectorHash = new Hashtable();
        private DateTime _tradeDate;

        public PSEDocument()
        {
            Sectors = new List<SectorItem>();
            ExchangeNotice = new StringCollection();            
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

        private void InitSectors()
        {
            var _reportChangeDate1 = new DateTime(2013, 11, 6);
            var _reportChangeDate2 = new DateTime(2013, 12, 3);
            var _reportChangeDate3 = new DateTime(2017, 4, 4);

            // this is the order in which the sectors appears on the report
            this._sectorHash.Add(0, FINANCIAL);
            this._sectorHash.Add(1, INDUSTRIAL);
            this._sectorHash.Add(2, HOLDING);
            this._sectorHash.Add(3, PROPERTY);
            this._sectorHash.Add(4, SERVICE);
            this._sectorHash.Add(5, MINING_OIL);
            if (DateTime.Compare(_tradeDate, _reportChangeDate1) >= 0)
            {                
                this._sectorHash.Add(6, PSEI);
                this._sectorHash.Add(7, ALLSHARES);
                this._sectorHash.Add(8, PREFERRED);
                this._sectorHash.Add(9, DEPOSITORY_RECEIPTS);
                this._sectorHash.Add(10, WARRANT);
                this._sectorHash.Add(11, SME);
                if (DateTime.Compare(_tradeDate, _reportChangeDate2) >= 0)
                    this._sectorHash.Add(12, ETF);
                if (DateTime.Compare(_tradeDate, _reportChangeDate3) >= 0)
                    this._sectorHash.Add(13, DOLLAR_DONOMINATED_SECURITIES);
            }
            else
            {
                this._sectorHash.Add(6, PSEI);
                this._sectorHash.Add(7, ALLSHARES);
                this._sectorHash.Add(8, PREFERRED);
                this._sectorHash.Add(9, WARRANT);
                this._sectorHash.Add(10, SME);
                this._sectorHash.Add(11, DEPOSITORY_RECEIPTS);
            }

            // this is the order in which the sectors are printed in the sectory summary section
            // of the report
            this.Sectors.Add(new SectorItem(this) { Symbol = FINANCIAL });
            this.Sectors.Add(new SectorItem(this) { Symbol = INDUSTRIAL });
            this.Sectors.Add(new SectorItem(this) { Symbol = HOLDING });
            this.Sectors.Add(new SectorItem(this) { Symbol = PROPERTY });
            this.Sectors.Add(new SectorItem(this) { Symbol = SERVICE });
            this.Sectors.Add(new SectorItem(this) { Symbol = MINING_OIL });
            this.Sectors.Add(new SectorItem(this) { Symbol = SME });
            this.Sectors.Add(new SectorItem(this) { Symbol = ETF });
            this.Sectors.Add(new SectorItem(this) { Symbol = PSEI });
            this.Sectors.Add(new SectorItem(this) { Symbol = ALLSHARES });
            //-

            this.Sectors.Add(new SectorItem(this) { Symbol = PREFERRED });
            this.Sectors.Add(new SectorItem(this) { Symbol = DEPOSITORY_RECEIPTS });
            this.Sectors.Add(new SectorItem(this) { Symbol = WARRANT });
            this.Sectors.Add(new SectorItem(this) { Symbol = DOLLAR_DONOMINATED_SECURITIES });
                    
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

        public SectorItem GetSector(int index)
        {
            return this.GetSector(this._sectorHash[index].ToString());
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
            get { return _tradeDate; }
            set 
            { 
                _tradeDate = value;

                // sector order is dependent on the trade date
                InitSectors(); 
            }
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

        public StringCollection ExchangeNotice
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
