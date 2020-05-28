namespace PSEGetLib.DocumentModel
{
	public class StockBase
	{
		public StockBase(PSEDocument ownerDocument)
		{
			OwnerDocument = ownerDocument;
		}
		public PSEDocument OwnerDocument { get; set; }

		public string Symbol { get; set; }

		public string Description { get; set; }

        public string FullName { get; set; } = "";

        public string Currency { get; set; } = "PHP";

        public string Market { get; set; } = "PSEi";

        public bool IsFavourite { get; set; } = false;

        public bool IsIndex { get; set; } = false;

		public double Open { get; set; }

		public double High { get; set; }

		public double Low { get; set; }

		public double Close { get; set; }

		public ulong Volume { get; set; }

		public double Value { get; set; }

		public double NetForeignBuy
		{
			get; set;
		}
	}
}
