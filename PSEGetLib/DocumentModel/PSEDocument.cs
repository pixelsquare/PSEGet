using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Specialized;
using System.Collections;
using PSEGetLib.Converters;
using System.Reflection;

namespace PSEGetLib.DocumentModel
{
    public class PSEDocument
    {
        private delegate void ToCSVDelegate(PSEDocument pseDocument, CSVOutputSettings csvOutputSettings);
        private delegate void ToAmibrokerDelegate(PSEDocument pseDocument, AmiOutputSettings amiOutputSettings);
        private delegate void ToMetaStockDelegate(PSEDocument pseDocument, MetaOutputSettings metaOutputSettings);

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

        private Hashtable _sectorHash = new Hashtable();

        //fields
        private List<SectorItem> _sectors;
        private DateTime _tradeDate;
        private int _numAdvance;
        private int _numDeclines;
        private int _numUnchanged;
        //private int _numTraded;
        //private int _numTrades;
        private ulong _oddLotVolume;
        private double _oddLotValue;
        private ulong _mainCrossVolume;
        private double _mainCrossValue;
        private double _bondsVolume;
        private double _bondsValue;
        private StringCollection _exchangeNotice;
        private double _totalForeignBuying;
        private double _totalForeignSelling;
        private string _blockSales;

        public PSEDocument()
        {
            _sectors = new List<SectorItem>();
            _exchangeNotice = new StringCollection();

            //this.InitSectors();
        }

        public void ToCSV(CSVOutputSettings csvOutputSettings)
        {
            var converter = new CSVConverter();
            converter.Execute(this, csvOutputSettings);
        }

        public void ToAmibroker(AmiOutputSettings amiOutputSettings)
        {
            var converter = new AmibrokerConverter();
            converter.Execute(this, amiOutputSettings);
        }

        public void ToMetaStock(MetaOutputSettings metaOutputSettings)
        {
            CSVOutputSettings csvOutputSettings = new CSVOutputSettings();
            csvOutputSettings.DateFormat = "yyyymmdd";
            csvOutputSettings.CSVFormat = "{S},M,{D},{O},{H},{L},{C},{V},{F}";
            csvOutputSettings.Delimiter = ",";
            csvOutputSettings.Filename = "tmp.csv";
            csvOutputSettings.SectorVolumeDivider = metaOutputSettings.SectorVolumeDivider;
            csvOutputSettings.OutputDirectory = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            csvOutputSettings.UseSectorValueAsVolume = metaOutputSettings.UseSectorValueAsVolume;
            
            CSVConverter csvConverter = new CSVConverter();
            csvConverter.Execute(this, csvOutputSettings);

            string csvFile = csvOutputSettings.OutputDirectory + "\\" + csvOutputSettings.Filename; 
            MetastockConverter converter = new MetastockConverter(this, csvFile, metaOutputSettings);
            converter.Execute(this, metaOutputSettings);
        }

        private void InitSectors()
        {
            var _reportChangeDate1 = new DateTime(2013, 11, 6);
            var _reportChangeDate2 = new DateTime(2013, 12, 3);

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

            this.Sectors.Add(new SectorItem(this) { Symbol = PSEI });
            //-
            this.Sectors.Add(new SectorItem(this) { Symbol = ALLSHARES });
            this.Sectors.Add(new SectorItem(this) { Symbol = SME });
            this.Sectors.Add(new SectorItem(this) { Symbol = ETF }); 

            this.Sectors.Add(new SectorItem(this) { Symbol = PREFERRED });
            this.Sectors.Add(new SectorItem(this) { Symbol = DEPOSITORY_RECEIPTS });
            this.Sectors.Add(new SectorItem(this) { Symbol = WARRANT });   
                    
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

            /*var stockItem = from stock in
                                (from subsector in (from sectorItem in this.Sectors select sectorItem) select subsector)
                            where stock.Symbol == symbol
                            select stock;
            foreach (var s in stockItem)
            {
                return s.SubSectors[0].Stocks[0];
            }
            throw new Exception(symbol + " not found");*/

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
            get { return _sectors; }
            set { _sectors = value; }
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
            get { return _numAdvance; }
            set { _numAdvance = value; }
        }

        public int NumDeclines
        {
            get { return _numDeclines; }
            set { _numDeclines = value; }
        }

        public int NumUnchanged
        {
            get { return _numUnchanged; }
            set { _numUnchanged = value; }
        }

        public int NumTraded
        {
            //get { return _numTraded; }
            //set { _numTraded = value; }
            get; set;
        }

        public int NumTrades
        {
            //get { return _numTrades; }
            //set { _numTrades = value; }
            get; set;
        }

        public ulong OddLotVolume
        {
            get { return _oddLotVolume; }
            set { _oddLotVolume = value; }
        }

        public double OddLotValue
        {
            get { return _oddLotValue; }
            set { _oddLotValue = value; }
        }

        public ulong MainCrossVolume
        {
            get { return _mainCrossVolume; }
            set { _mainCrossVolume = value; }
        }

        public double MainCrossValue
        {
            get { return _mainCrossValue; }
            set { _mainCrossValue = value; }
        }

        public double BondsVolume
        {
            get { return _bondsVolume; }
            set { _bondsVolume = value; }
        }

        public double BondsValue
        {
            get { return _bondsValue; }
            set { _bondsValue = value; }
        }

        public StringCollection ExchangeNotice
        {
            get { return _exchangeNotice; }
            set { _exchangeNotice = value; }
        }

        public double TotalForeignBuying
        {
            get { return _totalForeignBuying; }
            set { _totalForeignBuying = value; }
        }

        public double TotalForeignSelling
        {
            get { return _totalForeignSelling; }
            set { _totalForeignSelling = value; }
        }

        public double NetForeignBuying
        {
            get
            {
                return this._totalForeignBuying - this._totalForeignSelling;
            }            
        }

        public string BlockSales
        {
            get { return _blockSales; }
            set { _blockSales = value; }
        }
    }
}
