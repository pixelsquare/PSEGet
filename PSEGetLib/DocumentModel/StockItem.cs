namespace PSEGetLib.DocumentModel
{
    public class StockItem : StockBase
    {
        public StockItem(PSEDocument ownerDocument, SectorItem ownerSector)
            : base(ownerDocument)
        {
            OwnerSector = ownerSector;
        }

        public SectorItem OwnerSector { get; set; }

        //private string _subSectorName;
        private double _bid;
        private double _ask;

        public double Bid
        {
            get { return _bid; }
            set { _bid = value; }
        }

        public double Ask
        {
            get { return _ask; }
            set { _ask = value; }
        }
    }
}
