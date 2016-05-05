using System;
using PSEGetLib.DocumentModel;
using System.Reflection;

namespace PSEGetLib.Converters
{
    public static class Converter
    {
        public static void Convert<T>(PSEDocument pseDocument, T outputSettings)
        {
            if (outputSettings is CSVOutputSettings)
            {
                var converter = new CSVConverter();
                converter.Execute(pseDocument, outputSettings as CSVOutputSettings);
            }
            else if (outputSettings is AmiOutputSettings)
            {
                var converter = new AmibrokerConverter();
                converter.Execute(pseDocument, outputSettings as AmiOutputSettings);
            }
            else if (outputSettings is MetaOutputSettings)
            {
                var metaOutputSettings = outputSettings as MetaOutputSettings;
                CSVOutputSettings csvOutputSettings = new CSVOutputSettings();
                csvOutputSettings.DateFormat = "yyyymmdd";
                csvOutputSettings.CSVFormat = "{S},M,{D},{O},{H},{L},{C},{V},{F}";
                csvOutputSettings.Delimiter = ",";
                csvOutputSettings.Filename = "tmp.csv";
                csvOutputSettings.SectorVolumeDivider = metaOutputSettings.SectorVolumeDivider;
                csvOutputSettings.OutputDirectory = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                csvOutputSettings.UseSectorValueAsVolume = metaOutputSettings.UseSectorValueAsVolume;

                CSVConverter csvConverter = new CSVConverter();
                csvConverter.Execute(pseDocument, csvOutputSettings);

                string csvFile = csvOutputSettings.OutputDirectory + "\\" + csvOutputSettings.Filename;
                MetastockConverter converter = new MetastockConverter(pseDocument, csvFile, metaOutputSettings);
                converter.Execute(pseDocument, metaOutputSettings);
            }
            else
                throw new Exception("Unsupported setting type.");

        }
    }
}
