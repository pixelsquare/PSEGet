using System.Collections.Generic;

namespace PSEGetLib.DocumentModel
{
    public class SectorItem : StockBase
    {
        public List<SubSectorItem> SubSectors { get; set; }
        public double PercentChange { get; set; }
        public double PointChange { get; set; }

        public SectorItem(PSEDocument ownerDocument) 
            : base(ownerDocument)
        {
            SubSectors = new List<SubSectorItem>();
        }
    }
}
