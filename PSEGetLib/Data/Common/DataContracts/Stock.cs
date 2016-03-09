namespace PSEGetLib.Data.Common.DataContracts
{
    public class Stock
    {
        public string Symbol { get; set; }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
        public double PtsChange { get; set; }
        public double PctChg { get; set; }
        public ulong Volume { get; set; }
        public double Value { get; set; }
        public double NetForeignBS { get; set; }
        public double MktPercent { get; set; }
    }
}
