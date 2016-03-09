using System;
using System.Collections.Generic;
using PSEGetLib.Converters;
using System.IO;
using System.Globalization;

namespace PSEGetLib
{
	public class HistoricalDataReader
	{
		public HistoricalDataReader (string symbol)
		{
			this._symbol = symbol;
		}
		
		private string _symbol;
		public string Symbol
		{
			get
			{
				return this._symbol;
			}
			set
			{
				this._symbol = value;
			}
				
		}
		
		private string _data;
		public string Data
		{
			get
			{
				return this._data;	
			}
			set
			{
				this._data = value;
			}			
		}	
		
		private List<string> openList = new List<string>();
		private List<string> highList = new List<string>();
		private List<string> lowList = new List<string>();
		private List<string> closeList = new List<string>();
		private List<string> volumeList = new List<string>();
		private List<string> dateList = new List<string>();
			
		private void SetLists()
		{
			//
			// data format: separated by ";"
			// details: separated by "|"
			const int OPEN_INDEX = 0;
			const int HIGH_INDEX = 1;
			const int LOW_INDEX = 2;
			const int CLOSE_INDEX = 3;
			const int VOLUME_INDEX = 4;
			const int DATE_INDEX = 5;
			
			char[] split1Delimiter = new char[]{';'};
			char[] split2Delimiter = new char[] {'|'};
			string[] split1 = this._data.Split(split1Delimiter, StringSplitOptions.RemoveEmptyEntries);			
			
			if (split1.Length != 6)
            {
                string errorData = "";
                if (split1.Length > 0)
                    errorData = split1[0].Length >= 100 ? split1[0].Substring(1, 100) : "Erroneous data";
                throw new Exception("Data size mismatch for " + this._symbol + ". You can try again. Data: " + errorData);
            }
			
			string[] split2 = split1[OPEN_INDEX].Split(split2Delimiter, StringSplitOptions.RemoveEmptyEntries); 			
			this.openList.AddRange(split2);
			
			split2 = split1[HIGH_INDEX].Split(split2Delimiter, StringSplitOptions.RemoveEmptyEntries);
			this.highList.AddRange(split2);
			
			split2 = split1[LOW_INDEX].Split(split2Delimiter, StringSplitOptions.RemoveEmptyEntries);	
			this.lowList.AddRange(split2);
			
			split2 = split1[CLOSE_INDEX].Split(split2Delimiter, StringSplitOptions.RemoveEmptyEntries);
			this.closeList.AddRange(split2);
			
			split2 = split1[VOLUME_INDEX].Split(split2Delimiter, StringSplitOptions.RemoveEmptyEntries);
			this.volumeList.AddRange(split2);
			
			split2 = split1[DATE_INDEX].Split(split2Delimiter, StringSplitOptions.RemoveEmptyEntries);
			this.dateList.AddRange(split2);
		}
			
		private DateTime getTradeDate(string csvDate)
		{
			//Debug.WriteLine(csvDate);
            DateTimeFormatInfo dfi = new DateTimeFormatInfo();
            dfi.DateSeparator = "-";
            dfi.ShortDatePattern = "yyyy-MM-dd";
            //dfi.
            string tmpDate = csvDate.Substring(0, 4) + "-" + csvDate.Substring(4, 2) +"-"+ csvDate.Substring(6, 2);
			DateTime tmp = Convert.ToDateTime(tmpDate, dfi);
			return tmp;
		
		}
		
		public double getTradeVolume(string csvVolume)
		{
			double tmp = Convert.ToDouble(csvVolume);
			return tmp;
		}
		
		public void ToCSV(CSVOutputSettings outputSettings)
		{
			List<string> csvList = new List<string>();
			SetLists();
			for(int i = 0; i < this.openList.Count; i++)
			{
				string csvLine = outputSettings.CSVFormat.Replace(",", outputSettings.Delimiter);								
				string tradeDate = getTradeDate(dateList[i]).ToString(outputSettings.DateFormat);
                double volume = getTradeVolume(volumeList[i]);
                if (this._symbol.Contains("^"))
                    volume = volume / outputSettings.SectorVolumeDivider;
                
				string tradeVolume = volume.ToString("0");
				
				csvLine = csvLine.Replace(CSVOutputSettings.STOCK_SYMBOL, this._symbol);
	            csvLine = csvLine.Replace(CSVOutputSettings.DATE_SYMBOL, tradeDate);
	            csvLine = csvLine.Replace(CSVOutputSettings.OPEN_SYMBOL, openList[i]);
	            csvLine = csvLine.Replace(CSVOutputSettings.HIGH_SYMBOL, highList[i]);
	            csvLine = csvLine.Replace(CSVOutputSettings.LOW_SYMBOL, lowList[i]);
	            csvLine = csvLine.Replace(CSVOutputSettings.CLOSE_SYMBOL, closeList[i]);
	            csvLine = csvLine.Replace(CSVOutputSettings.VOLUME_SYMBOL, tradeVolume);
	            csvLine = csvLine.Replace(CSVOutputSettings.NFBS_SYMBOL, "0");
				
				csvList.Add(csvLine);
			}			
			string fileName = outputSettings.OutputDirectory + Helpers.GetDirectorySeparator() + 
								outputSettings.Filename;
			this.SaveToFile(fileName, csvList);
		}
		
		private void SaveToFile(string fileName, IEnumerable<string> csvList)
		{			 
            using (StreamWriter writer = new StreamWriter(fileName))
            {
                try
                {
                    foreach (string item in csvList)
                    {
                        writer.WriteLine(item);
                    }
                }
                finally
                {
                    writer.Flush();
                    writer.Close();
                }
            };			
		}
		
	}
}

