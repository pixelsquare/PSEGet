using System;
using System.Collections.Generic;
using PSEGetLib.Converters;
using System.Threading;
using PSEGetLib.DocumentModel;

namespace PSEGetLib
{
    public class ConvertFromFilesParam
    {
        public IEnumerable<string> FileList;
        public OutputSettings OutputSettings;
        public Action OnStartProcess;
        public Action<string> BeforeConvertCallback;
        public Action<string, PSEDocument> ProgressCallback;
        public Action<Exception> ExceptionCallback;
        public Action<PSEDocument> CompletedCallback;
        public Thread threadObject;
    }

    public static class ConvertFromFileHelper
    {
        public static void ConvertFromFiles(object convertFromFilesParamObj)
        {           
            ConvertFromFilesParam convertFromFilesParams = convertFromFilesParamObj as ConvertFromFilesParam;            
            try
            {                
                if (convertFromFilesParams.OnStartProcess != null)
                    convertFromFilesParams.OnStartProcess();

                PSEDocument pseDocument = null;
                foreach (string reportFile in convertFromFilesParams.FileList)
                {
                    if (convertFromFilesParams.BeforeConvertCallback != null)
                        convertFromFilesParams.BeforeConvertCallback(reportFile);

                    pseDocument = Helpers.LoadFromReportFile(reportFile, convertFromFilesParams.OutputSettings);

                    if (convertFromFilesParams.ProgressCallback != null)
                        convertFromFilesParams.ProgressCallback(reportFile, pseDocument);                    
                }
                if (convertFromFilesParams.CompletedCallback != null)
                    convertFromFilesParams.CompletedCallback(pseDocument);
            }
            catch (Exception e)
            {
                if (convertFromFilesParams.ExceptionCallback != null)
                    convertFromFilesParams.ExceptionCallback(e);
            }

        }
    }


}
