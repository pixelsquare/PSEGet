using System;
using System.Collections.Generic;
using System.Linq;
using Gtk;
//using org.apache.pdfbox.pdmodel;
//using org.apache.pdfbox.util;
using PSEGetLib;
using PSEGetLib.DocumentModel;
using PSEGetLib.Converters;
using PSEGetLib.Data.Common;
using PSEGetLib.Data.Service;
using PSEGetLib.Configuration;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using System.Threading;
using System.ComponentModel;

public partial class MainWindow : Gtk.Window
{
	//MonoPSEGetDataService dataService;
	private ConvertMethod convertMethod;
	private CSVOutputSettings csvOutputSettings = new CSVOutputSettings();
	private const string CONF_FILENAME = "PSEGet.conf.xml";
	private string reportFilenameFormat = "";
	private Dictionary<string, string> csvFormatDict = new Dictionary<string, string>();
	private List<string> selectedStocks = new List<string>();
	private ListStore listStocks = new ListStore(typeof(bool), typeof(string));
	
	public MainWindow () : base(Gtk.WindowType.Toplevel)
	{
		Build ();
		listSelectedFiles.Filter = new FileFilter();
		listSelectedFiles.Filter.AddPattern("*pdf");
		notebook2.CurrentPage = 0;
		
		//dataService = new MonoPSEGetDataService();
		//LoadHistoricalStockList();
		edtFrom.Text = DateTime.Today.ToString("MM/dd/yyyy");
		edtTo.Text = DateTime.Today.ToString("MM/dd/yyyy");
		
		GLib.ExceptionManager.UnhandledException += (e) =>{
			MessageDialog msgDlg = new MessageDialog(this, DialogFlags.Modal, MessageType.Error, ButtonsType.Close, e.ExceptionObject.ToString(), null);
			msgDlg.Title = "Error";
		
			if ((ResponseType)msgDlg.Run() == ResponseType.Close)
			{
				msgDlg.Destroy();
			}
			
		};
		InitCSVDict();
		labelStatus.Text = "";
		LoadOutputSettings();
		StreamReader reader = new StreamReader(GetAppPath() + "/README.txt");
		textview1.Buffer.Text = reader.ReadToEnd();
		textview1.Editable = false;
	}
	
	private string GetAppPath()
	{
		return System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
	}
	
	private void InitCSVDict()
	{
		csvFormatDict.Add("SYMBOL", "{S}");
		csvFormatDict.Add("DATE","{D}");
		csvFormatDict.Add("OPEN", "{O}");
		csvFormatDict.Add("HIGH", "{H}");
		csvFormatDict.Add("LOW", "{L}");
		csvFormatDict.Add("CLOSE", "{C}");
		csvFormatDict.Add("VOLUME", "{V}");
		csvFormatDict.Add("NFB/S", "{F}");	
		
	}
	
	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}
	protected virtual void OnButton9Clicked (object sender, System.EventArgs e)
	{
			
	}
	
	protected virtual void OnButton2Clicked (object sender, System.EventArgs e)
	{
		SaveOutputSettings();
		Application.Quit();
	}
	
	
	private void getAllStocksCallback(IEnumerable<string> stocks)
	{
		
		foreach(string s in stocks)
		{
			listStocks.AppendValues(true, s);
			this.selectedStocks.Add(s);
		}
		historicalStockListTreeview.Model = listStocks;
		
		CellRendererToggle chkBox = new CellRendererToggle();
		chkBox.Activatable = true;
		chkBox.Toggled += delegate(object o, ToggledArgs args) {
			TreeIter iter;
			if (listStocks.GetIter(out iter, new TreePath(args.Path)))
			{
				bool old = (bool) listStocks.GetValue(iter, 0);
				listStocks.SetValue(iter, 0, !old);
				if (!old)
					this.selectedStocks.Add((string)listStocks.GetValue(iter, 1));
				else
					this.selectedStocks.Remove((string)listStocks.GetValue(iter, 1));
					
			}
		};
		
		//TreeViewColumn chkColumn = new TreeViewColumn("Sel", chkBox, "toggled");
		//TreeViewColumn descColumn = new TreeViewColumn("Description", new CellRendererText(), "text");
		historicalStockListTreeview.HeadersVisible = false;
		historicalStockListTreeview.AppendColumn("Sel", chkBox, "active", 0);
		historicalStockListTreeview.AppendColumn("Description", new CellRendererText(), "text", 1);
	}
	

//	{
//		//dataService.GetStockList(getAllStocksCallback);	
//	}
	
	protected virtual void OnBtnGiveItClicked (object sender, System.EventArgs e)
	{
		
		try
		{
			switch(convertMethod)
			{
				case ConvertMethod.DownloadAndConvert: 
					DoDownloadAndConvert();
					break;
				case ConvertMethod.ConvertFromFiles: 
					DoConvertFromFiles();
					break;
				case ConvertMethod.DownloadHistoricalData:
					DoDownloadHistoricalData(Convert.ToInt32(edtNumYears.Text));
					break;
			}
			SaveOutputSettings();
		}
		catch(Exception exception)
		{
			labelStatus.Text = "Failed.";
			MessageDialog msgDlg = new MessageDialog(this, DialogFlags.Modal, MessageType.Error, ButtonsType.Close, null);
			msgDlg.Text = exception.Message;
			msgDlg.Title = "Error";
			if ((ResponseType)msgDlg.Run() == ResponseType.Close)
			{
				msgDlg.Destroy();
			};
		}
		
	}
	
	private void ProcessDownloadedFile(object sender, AsyncCompletedEventArgs e)
	{
		ReportDownloader downloader = sender as ReportDownloader;
		if (e.Error != null)
		{
			File.Delete(downloader.CurrentDownloadFile);
			throw new Exception(e.Error.Message);
		}
		else
		{
//			PSEDocument document = Helpers.ConvertReportFile(downloader.CurrentDownloadFile, this.csvOutputSettings);
//			MonoPSEGetDataService dataService = new MonoPSEGetDataService();
//			dataService.SaveTradeData(document);
		}
	}
	
	private void DoDownloadAndConvert()
	{
		DownloadParams param = new DownloadParams();
        param.BaseURL = edtDownloadLink.Text;
        param.FileName = "stockQuotes_%mm%dd%yyyy";
        param.FromDate = Convert.ToDateTime(edtFrom.Text);
        param.ToDate = Convert.ToDateTime(edtTo.Text);
		string savePath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/Reports";
        ReportDownloader downloader = new ReportDownloader(param, savePath, ProcessDownloadedFile);
        downloader.OnReportDownloadCompletedEvent += (s, o) => 
		{
		};
        downloader.OnDownloadAllCompletedEvent += (s, e) => 
		{
			
		};
        downloader.OnDownloadAllCompletedEvent += (s, e) => {
            //MonoPSEGetDataService dataService = new MonoPSEGetDataService();
			//dataService.
        };

        downloader.OnStartDownloadProcess += (s, o) => {
			labelStatus.Text = "Trying...";
		};
        downloader.OnDownloadProgressEvent += (s, o) => {
			
		};
        downloader.Download();
	}
	
	private List<string> getSelectedFiles()
	{
		//this.listSelectedFiles.
		List<string> result = new List<string>();
		foreach(string s in this.listSelectedFiles.Uris)
		{
			result.Add(s.Replace("file://", ""));
		}
		return result;
		//return null;
	}
	
	private void DoConvertFromFiles()
	{
		SetOutputSettings();
		IEnumerable<string> fileList = getSelectedFiles();
		//MonoPSEGetDataService dataService = new MonoPSEGetDataService();
		foreach(string s in fileList)
		{
			labelStatus.Text = "Converting " + s;
			PSEDocument pseDocument = Helpers.ConvertReportFile(s, this.csvOutputSettings);
			//dataService.SaveTradeData(pseDocument);
		}
		labelStatus.Text = "Done.";
		
//		Thread t = new Thread(ConvertFromFileHelper.ConvertFromFiles);
//		ConvertFromFilesParam param = new ConvertFromFilesParam();
//		param.FileList = getSelectedFiles();
//		param.OutputSettings = this.csvOutputSettings;
//		param.threadObject = t;
//		param.OnStartProcess = () =>
//		{	
//			//labelStatus.Text = "Initializing...";
//		};
//		
//		param.BeforeConvertCallback = (reportFileName) =>
//		{
//			// send stuff to progress
//			//labelStatus.Text = "Converting " + reportFileName;
//		};
//		
//		param.ProgressCallback = (reportFilename, pseDocument) =>
//		{
//			MonoPSEGetDataService dataService = new MonoPSEGetDataService();
//			dataService.SaveTradeData(pseDocument);
//			
//		};
//		
//		param.ExceptionCallback = (e) => 
//		{
//			
//		};
//		
//		param.CompletedCallback = (pseDocument) =>
//		{
//			//labelStatus.Text = "Done.";
//		};
//		
//		t.Start(param);
		
	}
	

	
	private void DoDownloadHistoricalData(int numYears)
	{
	    Dictionary<string, string> indexDict = new Dictionary<string, string>();
	    indexDict.Add("^PSEi", "PSE");
	    indexDict.Add("^ALLSHARES","ALL");
	    indexDict.Add("^FINANCIAL","FIN");
	    indexDict.Add("^INDUSTRIAL", "IND");
	    indexDict.Add("^HOLDING", "HDG");
	    indexDict.Add("^PROPERTY", "PRO");
	    indexDict.Add("^SERVICE", "SVC");
	    indexDict.Add("^MINING-OIL", "M-O");
    		
        foreach (string symbol in this.selectedStocks)
        {
            string tmpSymbol = symbol;
			string downloadStr;
            if (tmpSymbol.Contains("^"))
            {
                tmpSymbol = indexDict[symbol];
                downloadStr = "http://www.pse.com.ph/servlet/ChartForPhisixServlet?indexID=%s&years=%f";
            }
            else
                downloadStr = "http://www.pse.com.ph/servlet/PSEChartServlet?securitySymbol=%s&years=%f";
            
            downloadStr = downloadStr.Replace("%s", tmpSymbol).Replace("%f", numYears.ToString());
            this.csvOutputSettings.Filename = symbol + ".csv";

            
            HistoricalDataDownloader downloader = new HistoricalDataDownloader(new Uri(downloadStr));
            downloader.Download();

            HistoricalDataReader reader = downloader.GetReader(symbol);
            
            reader.ToCSV(this.csvOutputSettings);      
            
        }
		
		if (this.selectedStocks.Count > 0)
        	labelStatus.Text = "Done";
		else {
			labelStatus.Text = "No stock symbols selected";
		}
            
	}
	
	private string getCSVFormat()
	{
		string result = csvFormatDict[combo1.ActiveText] +
						"," + csvFormatDict[combo2.ActiveText] + 
						"," + csvFormatDict[combo3.ActiveText] +
						"," + csvFormatDict[combo4.ActiveText] +
						"," + csvFormatDict[combo5.ActiveText] + 
						"," + csvFormatDict[combo6.ActiveText] +
						"," + csvFormatDict[combo7.ActiveText] +
						"," + csvFormatDict[combo8.ActiveText];
		return result;
	}
	
	private void SetOutputSettings()
	{
		//Uri selectedFolder = new Uri(edtOutputDir.CurrentFolderUri);

		csvOutputSettings.DateFormat = edtDateFormat.Text;
		csvOutputSettings.Delimiter = edtDelimiter.Text;
		csvOutputSettings.CSVFormat = this.getCSVFormat();
		csvOutputSettings.OutputDirectory = edtOutputDir.Text; //selectedFolder.AbsolutePath;//edtOutputDir.Uri.Replace("file://", "");
		csvOutputSettings.Filename = reportFilenameFormat;
		csvOutputSettings.UseSectorValueAsVolume = chkIndexAsValue.Active;		
	}
	
	private string GetConfPath()
	{
		return System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/" + CONF_FILENAME;
	
	}
	
	private void SaveOutputSettings()
	{
		string confPath = GetConfPath();
		XmlSerializer xmlSerial = new XmlSerializer(typeof(AppConfiguration));
		TextWriter writer = new StreamWriter(confPath);
		
		AppConfiguration appConf = new AppConfiguration();
		appConf.IndexValueDivisor = (uint)this.csvOutputSettings.SectorVolumeDivider;
		appConf.CSVDateFormat = this.csvOutputSettings.DateFormat;
		appConf.CSVOutputFormat = this.csvOutputSettings.CSVFormat.Replace("{", string.Empty).Replace("}", string.Empty); //this.getCSVFormat();
		appConf.CSVDelimiter = this.csvOutputSettings.Delimiter;
		appConf.IndexValueAsVolume = chkIndexAsValue.Active;
		appConf.OutputFolder = this.csvOutputSettings.OutputDirectory;
		appConf.ReportFilenameFormat = reportFilenameFormat;
		appConf.ReportUrl = edtDownloadLink.Text;
		appConf.DataSource = this.convertMethod;
		
		xmlSerial.Serialize(writer, appConf);
		writer.Flush();
		writer.Close();
		
	}
	
	private void LoadOutputSettings()
	{
		string confPath = GetConfPath();
				
		XmlSerializer xml = new XmlSerializer(typeof(AppConfiguration));
		StreamReader reader = new StreamReader(confPath);
		AppConfiguration conf = (AppConfiguration)xml.Deserialize(reader);
		this.csvOutputSettings.DateFormat = conf.CSVDateFormat;
		
		string[] csvFormat = conf.CSVOutputFormat.Split( new Char[]{','}, StringSplitOptions.RemoveEmptyEntries);
		this.csvOutputSettings.CSVFormat = "";
		foreach(string s in csvFormat)
		{
			if (this.csvOutputSettings.CSVFormat == "")
				this.csvOutputSettings.CSVFormat += "{" + s + "}";	
			else
				this.csvOutputSettings.CSVFormat += ",{" + s + "}";
		}
		
		if (conf.OutputFolder.Trim() == "")
		{
			conf.OutputFolder = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
		}
		
		//this.csvOutputSettings.CSVFormat = conf.CSVOutputFormat;
		this.csvOutputSettings.Delimiter = conf.CSVDelimiter;
		this.csvOutputSettings.UseSectorValueAsVolume = conf.IndexValueAsVolume;
		this.csvOutputSettings.OutputDirectory = conf.OutputFolder;
		this.csvOutputSettings.SectorVolumeDivider = conf.IndexValueDivisor;
		this.csvOutputSettings.Filename = conf.ReportFilenameFormat;
		this.convertMethod = ConvertMethod.ConvertFromFiles;
		
		reportFilenameFormat = conf.ReportFilenameFormat;
		edtDownloadLink.Text = conf.ReportUrl;
		
		
		//Uri outpuFolder = new Uri(conf.OutputFolder);
		edtOutputDir.Text = conf.OutputFolder;//outpuFolder.AbsoluteUri);
		setCSVComboBoxes(conf.CSVOutputFormat);
		expConvertFromFiles.Expanded = true;

//		switch(conf.DataSource)
//		{
//			case ConvertMethod.DownloadAndConvert: expDownloadAndConvert1.Expanded = true;
//				expConvertFromFiles.Expanded = false;
//				expHistoricalData.Expanded = false;
//				break;
//			case ConvertMethod.ConvertFromFiles: expConvertFromFiles.Expanded = true;
//				expDownloadAndConvert1.Expanded = false;
//				expHistoricalData.Expanded = false;
//				break;
//			case ConvertMethod.DownloadHistoricalData: expHistoricalData.Expanded = true;
//				expDownloadAndConvert1.Expanded = false;
//				expConvertFromFiles.Expanded = false;
//				break;
//		}
		
		reader.Close();
	}
	
	private int getComboIndex(string csvFieldSymbol)
	{
		int result = 0;
		foreach(KeyValuePair<string, string> pair in csvFormatDict)
		{
			if (pair.Value == csvFieldSymbol)
				return result;
			result++;
		}
		return result;
	}
	
	private void setCSVComboBoxes(string csvFormat)
	{
		string[] arr = csvFormat.Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries);
		int i = 0;
		
		foreach(string s in arr)
		{
			int x = getComboIndex("{" + s +"}");
			switch(i)
			{
				case 0: combo1.Active = x;
						break;
				case 1: combo2.Active = x;
						break;
				case 2: combo3.Active = x;
						break;
				case 3: combo4.Active = x;
						break;
				case 4: combo5.Active = x;
						break;
				case 5: combo6.Active = x;
						break;
				case 6: combo7.Active = x;
						break;
				case 7: combo8.Active = x;
						break;
								
			}
			i++;
		}
	}
	
	protected virtual void btnSelectAll_Click (object sender, System.EventArgs e)
	{
		this.selectedStocks.Clear();
		listStocks.Foreach((TreeModel, path, iter) =>
		{
			listStocks.SetValue(iter, 0, true);
			this.selectedStocks.Add((string)listStocks.GetValue(iter, 1));
			return false;
		});
	}
	
	protected virtual void btnUnselectAll_Click (object sender, System.EventArgs e)
	{
		this.selectedStocks.Clear();
		listStocks.Foreach((TreeModel, path, iter) =>
		{
			listStocks.SetValue(iter, 0, false);
			return false;
		});
	}
	
	protected virtual void notebook_ChangeCurrentPage (object o, Gtk.ChangeCurrentPageArgs args)
	{
		
	}
	
	protected virtual void OnExpDownloadAndConvert1Activated (object sender, System.EventArgs e)
	{
		Expander expander = (Expander)sender;
		
		if (expander == expDownloadAndConvert1)
		{
			if (expander.Expanded)
			{
				convertMethod = ConvertMethod.DownloadAndConvert;
				expConvertFromFiles.Expanded = false;
				expHistoricalData.Expanded = false;
			}
			else
			{
				
			}
		}
		
		else if (expander == expConvertFromFiles)
		{
			if (expander.Expanded)
			{
				convertMethod = ConvertMethod.ConvertFromFiles;
				expDownloadAndConvert1.Expanded = false;
				expHistoricalData.Expanded = false;
			}
			else
			{
				
			}
		}
		else if (expander == expHistoricalData)
		{
			if (expander.Expanded)	
			{
				convertMethod = ConvertMethod.DownloadHistoricalData;
				expDownloadAndConvert1.Expanded = false;
				expConvertFromFiles.Expanded = false;
			}
			else
			{
				
			}
		}
	}
		
}

