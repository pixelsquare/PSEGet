using System;
using System.Net;
using System.Text;

namespace PSEGetLib
{
	public class HistoricalDataDownloader
	{
		
		public HistoricalDataDownloader (Uri downloadUri)
		{
			this._downloadUri = downloadUri;
			
			string[] uriParts = this._downloadUri.Query.Split(new char[]{'&'}, StringSplitOptions.None);
													
			this._symbol = uriParts[0].Split(new char[]{'='}, StringSplitOptions.RemoveEmptyEntries)[1];
			this._numYears = Convert.ToInt32(uriParts[1].Split(new char[]{'='}, StringSplitOptions.RemoveEmptyEntries)[1]);
            
		}
		
		private Uri _downloadUri;
		public Uri DownloadUri
		{
			get
			{
				return this._downloadUri;
			}
			set
			{
				this._downloadUri = value;
			}
				
		}
		
		private string _downloadedData;
		public string DownloadedData
		{
			get
			{
				return this._downloadedData;
			}
		}
		
		private string _symbol;
		public string Symbol
		{
			get
			{
				return this._symbol;
			}
		}
		private int _numYears;
		public int NumYears
		{
			get
			{
				return this._numYears;
			}
		}
		
					
		public HistoricalDataReader GetReader()
		{
			var reader = new HistoricalDataReader(this._symbol);
			reader.Data = this.DownloadedData;
			return reader;
		}

        public HistoricalDataReader GetReader(string symbolAlias)
        {
            HistoricalDataReader reader = new HistoricalDataReader(symbolAlias);
            reader.Data = this.DownloadedData;
            return reader;
        }

        public void DownloadAsync(Action<string> completedHandler)
        {
            this._downloadedData = string.Empty;
            WebClient wc = new WebClient();
            wc.DownloadDataCompleted += (o, e) =>
            {
                if (e.Error != null)
                {
                    throw new Exception(e.Error.Message);                     
                }
                this._downloadedData = ASCIIEncoding.ASCII.GetString(e.Result);
                completedHandler(this.DownloadedData);
            };
            
            wc.DownloadDataAsync(this._downloadUri);
            	
        }
       
		public void Download()
		{
            this._downloadedData = string.Empty;
			WebClient wc = new WebClient();
			byte[] result = wc.DownloadData(this._downloadUri);
			this._downloadedData = ASCIIEncoding.ASCII.GetString(result);	
			
			/*wc.DownloadDataCompleted += delegate(object sender, DownloadDataCompletedEventArgs e) 
			{
				if (e.Error != null)
				{									
					throw e.Error;
				}
				else
				{
					this._downloadedData = System.Text.ASCIIEncoding.ASCII.GetString(e.Result);	
					downloadCallback(this._downloadedData);
				}
			};	*/
		}
			
	}
}

