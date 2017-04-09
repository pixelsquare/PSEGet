using System;
using System.Collections.Generic;
using System.Linq;
using PSEGetLib.Converters;
using PSEGetLib.DocumentModel;
using System.IO;
using System.Threading;
using PSEGetLib.Interfaces;
using Microsoft.Practices.ServiceLocation;

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

        public static PSEDocument LoadFromReportFile(string fullFilePath, OutputSettings outputSettings)
        {
            //var doc = PDDocument.load(fullFilePath);
            ////PSEDocument pd = new PSEDocument();

            //var stripper = new PDFTextStripper();
            //string pdfText = stripper.getText(doc).TrimEnd();

            IPdfService pdfService = ServiceLocator.Current.GetInstance<IPdfService>(); 
            string pdfText = pdfService.ExtractTextFromPdf(fullFilePath);

            
            var document = new PSEDocument();
			var reader = new PSEReportReader2();
			reader.Fill(document, pdfText);

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
    }
    
}
