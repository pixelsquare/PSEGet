using System;
using System.Collections.Generic;
using System.Linq;
using PSEGetLib.Converters;
using PSEGetLib.DocumentModel;
using System.IO;
using System.Threading;
using LateBindingHelper;
using System.Diagnostics;
using PSEGetLib.Interfaces;

namespace PSEGetLib
{
    static class Variables
    {
        public static int DownloadedCount;
        public static List<Thread> threadList = new List<Thread>();
    }

    public static class Helpers
    {
        //private static OutputSettings _outputSettings;

        public static PSEDocument ConvertReportFile(string fullFilePath, OutputSettings outputSettings)
        {
            //var doc = PDDocument.load(fullFilePath);
            ////PSEDocument pd = new PSEDocument();

            //var stripper = new PDFTextStripper();
            //string pdfText = stripper.getText(doc).TrimEnd();

            IPdfService pdfService = new PdfTextSharpService();
            string pdfText = pdfService.ExtractTextFromPdf(fullFilePath);

            var reader = new PSEReportReader(pdfText);
            var document = new PSEDocument();
            reader.Fill(document);

            if (outputSettings is CSVOutputSettings)
            {
                (outputSettings as CSVOutputSettings).Filename = Path.GetFileNameWithoutExtension(fullFilePath) + ".csv";
                document.ToCSV(outputSettings as CSVOutputSettings);
            }

            if (outputSettings is AmiOutputSettings)
            {
                document.ToAmibroker(outputSettings as AmiOutputSettings);
            }

            if (outputSettings is MetaOutputSettings)
                document.ToMetaStock(outputSettings as MetaOutputSettings);

            return document;
        }

		public static string GetDirectorySeparator()
		{
			if (HostOS.determineHostEnviroment() == HostOS.HostEnviroment.Windows)
				return "\\";
			else 
				return "/";	
			
		}

        public static string GetRuturnChar()
        {
            if (HostOS.determineHostEnviroment() == HostOS.HostEnviroment.Windows)
                return "\r";
            else
                return "\n";	
        }

        private static bool _isAmibrokerInstalled;
        public static bool IsAmibrokerInstalled()
        {
            try
            {
                if (!_isAmibrokerInstalled)
                {
                    Process[] localByName = Process.GetProcessesByName("broker");
                    if (localByName.Any())
                        _isAmibrokerInstalled = true;
                    else
                    {
                        IOperationInvoker amiInvoker = BindingFactory.CreateAutomationBinding("Broker.Application");
                        try
                        {
                            _isAmibrokerInstalled = amiInvoker != null;
                        }
                        finally
                        {
                            if (amiInvoker != null)
                                amiInvoker.Method("Quit").Invoke();
                        }
                    }
                }
                return _isAmibrokerInstalled;
            }
            catch
            {
                return false;
            }
                        
        }

    }
    
}
