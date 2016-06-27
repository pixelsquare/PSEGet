using System;
using System.IO;
using PSEGetLib;
using PSEGetLib.Interfaces;
using PSEGetLib.Service;
using PSEGetLib.DocumentModel;
using PSEGetLib.Converters;

namespace pseget
{
    class Program
    {
        static string _targetPath;
        static string _outputPath;
        static string _outputFormat;
        static string _dateFormat;

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                DisplayHelp();
                return;
            }

            Initialize();
            try
            {
                WorkIt();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static void WorkIt()
        {
            if (_targetPath == null)
            {
                Console.WriteLine("Error: Unspecified PSE report file path.");
                return;
            }

            var pseDocument = new PSEDocument();
            IPdfService pdfService = new PdfTextSharpService();
            if (_targetPath.Contains("http:"))
            {
                IReportDownloader downloader = new ReportDownloader();
                downloader.DownloadParams.BaseURL = _targetPath;
                downloader.DownloadParams.FileName = "stockQuotes_%mm%dd%yyyy";
                downloader.SavePath = Environment.CurrentDirectory + "\\Reports";
                downloader.Download();
            }
            else
            {
                IPSEReportReader reader = new PSEReportReader(pdfService.ExtractTextFromPdf(_targetPath));
                reader.Fill(pseDocument);
            }

            if (_outputFormat.Contains("csv"))
            {
                string[] csvParam = _outputFormat.Split(':');
                string csvFormat = string.Empty;
                if (csvParam.Length == 2)
                {
                    csvFormat = csvParam[1];
                    csvFormat = csvFormat.Replace("S", "{S}");
                    csvFormat = csvFormat.Replace("D", "{D}");
                    csvFormat = csvFormat.Replace("O", "{O}");
                    csvFormat = csvFormat.Replace("H", "{H}");
                    csvFormat = csvFormat.Replace("L", "{L}");
                    csvFormat = csvFormat.Replace("C", "{C}");
                    csvFormat = csvFormat.Replace("V", "{V}");
                    csvFormat = csvFormat.Replace("F", "{F}");
                }
                else
                    csvFormat = "{S},{D},{O},{H},{L},{C},{V},{F}";
               
                var csvOutputSettings = new CSVOutputSettings();
                csvOutputSettings.CSVFormat = csvFormat;
                csvOutputSettings.DateFormat = _dateFormat;
                csvOutputSettings.Delimiter = ",";
                csvOutputSettings.Filename = Path.GetFileName(_targetPath).Replace("pdf", "csv");
                csvOutputSettings.OutputDirectory = _outputPath;
                csvOutputSettings.UseSectorValueAsVolume = true;
                csvOutputSettings.SectorVolumeDivider = 1000;

                pseDocument.ToCSV(csvOutputSettings);
            }
            else if (_outputFormat.Contains("ami"))
            {
                IAmibrokerService amiService = new AmibrokerService();
                if (!amiService.IsAmibrokerInstalled())
                {
                    throw new Exception("Error: Amibroker is not installed on this machine.");
                }

                string[] amiParam = _outputFormat.Split(':');
                if (amiParam.Length < 2)
                {
                    throw new Exception("Error: Unspecified Amibroker database folder.");
                }
                string amiDatabaseFolder = amiParam[1];

                var amiOutputSettings = new AmiOutputSettings();
                amiOutputSettings.DatabaseDirectory = amiDatabaseFolder;
                amiOutputSettings.SectorVolumeDivider = 1000;
                amiOutputSettings.UseSectorValueAsVolume = true;

                pseDocument.ToAmibroker(amiOutputSettings);
            }
        }            

        static void Initialize()
        {
            _targetPath = GetParamValue("-t");
            _outputPath = GetParamValue("-o");
            if (_outputPath == null)
            {
                _outputPath = Environment.CurrentDirectory;
            }

            _outputFormat = GetParamValue("-f");
            if (_outputFormat == null)
            {
                _outputFormat = "csv:S,D,O,H,L,C,V,F";
            }

            _dateFormat = GetParamValue("-d");
            if (_dateFormat == null)
            {
                _dateFormat = "MM/DD/YYYY";
            }
        }

        static string GetParamValue(string param)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == param)
                {
                    if (i + 1 < args.Length)
                        return args[i + 1];
                }
            }

            return null;
        }        

        static void DisplayHelp()
        {
            Console.WriteLine("PSEGet Console. (c) 2016 Arnold Diaz.");
            Console.WriteLine("\tUsage: pseget -t [<from: date to: date> | local path] -o [output path] -f [csv:<format> | ami] -d [date format]");
            Console.WriteLine("\t-t [url:<from: date to: date> | local path]");
            Console.WriteLine("\t\tPSE Report File Path. \n");
            Console.WriteLine("\t-o [output path]");
            Console.WriteLine("\t\tOptional. Output Path. Defaults to executable path.\n");
            Console.WriteLine("\t-f [csv:<format> | ami:<database path>]");
            Console.WriteLine("\t\tOptional. Target Output. Defaults to CSV.");
            Console.WriteLine("\t\tCSV Optional <format> defaults to S,D,O,H,L,C,V,F\n");            
            Console.WriteLine("\t-d [date format]");
            Console.WriteLine("\t\tOptional Date Format. Defaults to MM/dd/yyyy.\n");
            Console.WriteLine("Example 1 (Download)       : pseget -t http://www.pse.com.ph/resource/dailyquotationreport/file/:from:06/20/2016 to:06/24/2016 ");
            Console.WriteLine("Example 2 (From file)     : pseget -t c:\\myreports\\stockQuotes_552016.pdf");
            Console.WriteLine("Example 3 (Typical)       : pseget -t http://www.pse.com.ph/resource/dailyquotationreport/file/:from:06/20/2016 to:06/24/2016 -o c:\\myfolder\\");
            Console.WriteLine("Example 4 (CSV)           : pseget -t c:\\myreports\\stockQuotes_552016.pdf -o c:\\myfolder\\ -f csv:S,D,C");
            Console.WriteLine("Example 5 (Amibroker)     : pseget -t c:\\myreports\\stockQuotes_552016.pdf -f ami:\"c:\\program files\\\"");
        }
    }
}
