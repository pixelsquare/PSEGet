using PSEGetLib.DocumentModel;
using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace PSEGetLib.Converters
{
    public class MetastockConverter : IDataConverter<MetaOutputSettings>
    {
        public MetastockConverter()
        {
        }

        public MetastockConverter(PSEDocument pseDocument, string metaAsciiFile, MetaOutputSettings outputSettings)
        {
            MetaAsciiFile = metaAsciiFile;
            OutputSettings = outputSettings;
            PSEDocument = pseDocument;
        }

        public string MetaAsciiFile
        {
            get;
            set;
        }

        public PSEDocument PSEDocument { get; set; }

        public MetaOutputSettings OutputSettings
        {
            get;
            set;
        }

        private string getConvertParams()
        {
            if (OutputSettings.UseSingleDirectory)
            {
                return string.Format("-f \"{0}\" -r r -o \"{1}\" --ignoreOpenInterest=no --quite", MetaAsciiFile, OutputSettings.OutputDirectory);
            }
            else
                return string.Format("-f \"{0}\" -r r -t -o \"{1}\" --ignoreOpenInterest=no --quite", MetaAsciiFile, OutputSettings.OutputDirectory);
        }

        public void Execute()
        {
            string asc2msPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\asc2ms.exe";
            string convertParam = getConvertParams();
            Process p = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo(asc2msPath, convertParam);
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            p.StartInfo = startInfo;
            
           
            p.Start();
        }


    }
}
