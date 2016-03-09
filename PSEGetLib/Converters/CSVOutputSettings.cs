using System;

namespace PSEGetLib.Converters
{
    public sealed class CSVOutputSettings : OutputSettings, ICloneable
    {
        public string OutputDirectory { get; set; }

        private string dateFormat;
        public string DateFormat { 
            get
            {
                return this.dateFormat;
            }
            set
            {
                value = value.ToLower();
                value = value.Replace("mm", "MM");
                this.dateFormat = value;
            } 
        }

        private string _csvFormat;
        public string CSVFormat 
        { 
                get
                {
                    return this._csvFormat;
                }
                set
                {
                    /*value = value.Replace("S", "{S}");
                    value = value.Replace("N", "{N}");
                    value = value.Replace("D", "{D}");
                    value = value.Replace("O", "{O}");
                    value = value.Replace("H", "{H}");
                    value = value.Replace("L", "{L}");
                    value = value.Replace("C", "{C}");
                    value = value.Replace("V", "{V}");
                    value = value.Replace("F", "{F}");*/
                    this._csvFormat = value;
                } 
        }
        public string Delimiter { get; set; }
        public string Filename { get; set; }

        public override string ToString()
        {
            return string.Format("Directory: {0}, Date Format: {1}, CSVFormat: {2}, Delimiter: {3}",
                OutputDirectory, DateFormat, CSVFormat, Delimiter);
        }
		
		public const string STOCK_SYMBOL = "{S}";
		public const string DESCRIPTION_SYMBOL = "{N}";
		public const string DATE_SYMBOL = "{D}";
		public const string OPEN_SYMBOL = "{O}";
		public const string HIGH_SYMBOL = "{H}";
		public const string LOW_SYMBOL = "{L}";
		public const string CLOSE_SYMBOL = "{C}";
		public const string VOLUME_SYMBOL = "{V}";
		public const string NFBS_SYMBOL = "{F}";
        public const string VALUE_SYMBOL = "{E}";

        public object Clone()
        {
            CSVOutputSettings result = new CSVOutputSettings();
            result.CSVFormat = this.CSVFormat;
            result.Delimiter = this.Delimiter;
            result.DateFormat = this.DateFormat;
            result.Filename = this.Filename;
            result.OutputDirectory = this.OutputDirectory;
            result.UseSectorValueAsVolume = this.UseSectorValueAsVolume;
            return result;
        }
    } 
}
