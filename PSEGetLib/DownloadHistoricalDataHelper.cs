using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using PSEGetLib.Converters;

namespace PSEGetLib
{
    public class HistoricalDownloadParams
    {
        public IEnumerable<string> stockList;
        public int NumYears;
        public CSVOutputSettings OutputSettings;
        public Action StartDownloadProcess;
        public Action<string> BeforeStockDataDownloadCallback;
        public Action<string> AfterStockDataDownloadCallback;
        public Action DownloadAllCompleteCallback;
        public Action<Exception> OnExceptionCallback;
    }

    class DownloadWorker
    {
        private static object threadLock = new object();        
        public void DownloadStockHistoryData(object downloadParams)
        {
            HistoricalDownloadParams _downloadParams = downloadParams as HistoricalDownloadParams;
            IEnumerable<string> stocks = _downloadParams.stockList as IEnumerable<string>;
            string downloadStr;
            int numYears = _downloadParams.NumYears;
            CSVOutputSettings outputSettings = _downloadParams.OutputSettings;

            Dictionary<string, string> indexDict = new Dictionary<string, string>();
            indexDict.Add("^PSEi", "PSE");
            indexDict.Add("^ALLSHARES","ALL");
            indexDict.Add("^FINANCIAL","FIN");
            indexDict.Add("^INDUSTRIAL", "IND");
            indexDict.Add("^HOLDING", "HDG");
            indexDict.Add("^PROPERTY", "PRO");
            indexDict.Add("^SERVICE", "SVC");
            indexDict.Add("^MINING-OIL", "M-O");
            try
            {
                foreach (string symbol in stocks)
                {
                    string tmpSymbol = symbol;

                    if (tmpSymbol.Contains("^"))
                    {
                        tmpSymbol = indexDict[symbol];
                        downloadStr = "http://www2.pse.com.ph/servlet/ChartForPhisixServlet?indexID=%s&years=%f";
                    }
                    else
                        downloadStr = "http://www2.pse.com.ph/servlet/PSEChartServlet?securitySymbol=%s&years=%f";
                    
                    downloadStr = downloadStr.Replace("%s", tmpSymbol).Replace("%f", numYears.ToString());
                    outputSettings.Filename = symbol + ".csv";

                    // before download callback
                    if (_downloadParams.BeforeStockDataDownloadCallback != null)
                    {
                        _downloadParams.BeforeStockDataDownloadCallback(symbol);
                    }

                    HistoricalDataDownloader downloader = new HistoricalDataDownloader(new Uri(downloadStr));
                    downloader.Download();

                    HistoricalDataReader reader = downloader.GetReader(symbol);
                    lock (DownloadWorker.threadLock)
                    {
                        reader.ToCSV(outputSettings);
                        Variables.DownloadedCount++;
                        
                        // after download
                        if (_downloadParams.AfterStockDataDownloadCallback != null)
                            _downloadParams.AfterStockDataDownloadCallback(symbol);
                    }
                    
                }
            }
            catch (Exception e)
            {
                //throw new Exception(e.Message);
                if (_downloadParams.OnExceptionCallback != null)
                    _downloadParams.OnExceptionCallback(e);

                // abort threads
                foreach (Thread t in Variables.threadList)
                {
                    if (t.IsAlive)
                        t.Abort();
                }

            }
        }
    }

    public static class DownloadHistoricalDataHelper
    {
        private static void DoDownloadHistoricalData(IEnumerable<string> stockList, 
            int numYears, CSVOutputSettings outputSettings, 
            Action<string> BeforeDownload, Action<string> AfterDownload, Action<Exception> ExceptionCallback)
        {
            // initialize parameters
            HistoricalDownloadParams downloadParams = new HistoricalDownloadParams();
            downloadParams.stockList = stockList;
            downloadParams.NumYears = numYears;
            downloadParams.OutputSettings = outputSettings.Clone() as CSVOutputSettings;
            downloadParams.BeforeStockDataDownloadCallback = BeforeDownload;
            downloadParams.AfterStockDataDownloadCallback = AfterDownload;
            downloadParams.OnExceptionCallback = ExceptionCallback;

            // start the thread
            DownloadWorker worker = new DownloadWorker();

            Thread t = new Thread(worker.DownloadStockHistoryData);
            Variables.threadList.Add(t);
            t.Start(downloadParams);
            //worker.DownloadStockHistoryData(downloadParams);
        }

        public static void DownloadAndConvertHistoricalData(object downloadParams)
        {      
            Variables.DownloadedCount = 0; // track downloads
            Variables.threadList.Clear();
            HistoricalDownloadParams _downloadParams = downloadParams as HistoricalDownloadParams;

            if (_downloadParams.StartDownloadProcess != null)
                _downloadParams.StartDownloadProcess();

            List<string> tmpList = new List<string>();
            tmpList.AddRange(_downloadParams.stockList.ToArray());

            const int NUM_THREADS = 5;
            int stockCount = tmpList.Count;
            int threadCount = stockCount / NUM_THREADS;
            if (threadCount == 0)
                threadCount = 1;
            int startIndex = 0;
            int adder = 0;

            for (int i = 0; i < threadCount; i++)
            {
                // add the remainder of the stocks in the list                
                if (threadCount == i + 1)
                    adder = stockCount % NUM_THREADS;

                int downloadItemCount = (stockCount >= NUM_THREADS) ? NUM_THREADS + adder : adder;
                // take the items
                List<string> l = new List<string>();
                l.AddRange(tmpList.Take(downloadItemCount).ToArray());
                DoDownloadHistoricalData(l, _downloadParams.NumYears, _downloadParams.OutputSettings,
                    _downloadParams.BeforeStockDataDownloadCallback, _downloadParams.AfterStockDataDownloadCallback,
                    _downloadParams.OnExceptionCallback);

                // remove added items
                tmpList.RemoveRange(startIndex, downloadItemCount);
            }

            // wait for everything to complete
            while (true)
            {
                int count = (from t in Variables.threadList
                             where t.IsAlive == true
                             select t).Count();
                if (count == 0)
                {
                    if (_downloadParams.DownloadAllCompleteCallback != null)
                        _downloadParams.DownloadAllCompleteCallback();
                    break;
                }
            }
        }
    }
}
