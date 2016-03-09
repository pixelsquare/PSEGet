using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Xml.Serialization;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using PSEGet3.Messages;
using PSEGetLib;
using PSEGetLib.Configuration;
using PSEGetLib.Converters;

namespace PSEGet3.ViewModel
{
    public class CSVField
    {
        public CSVField(string fieldId, string fieldDescription)
        {
            FieldId = fieldId;
            FieldDescription = fieldDescription;
        }

        public string FieldId { get; set; }
        public string FieldDescription { get; set; }

        public override string ToString()
        {
            return FieldDescription;
        }
    }

    public class CSVFields : List<CSVField>
    {
        public CSVFields()
        {
            //S,D,O,H,L,C,V,F
            Add(new CSVField("S", "SYMBOL"));
            Add(new CSVField("D", "DATE"));
            Add(new CSVField("O", "OPEN"));
            Add(new CSVField("H", "HIGH"));
            Add(new CSVField("L", "LOW"));
            Add(new CSVField("C", "CLOSE"));
            Add(new CSVField("V", "VOLUME"));
            Add(new CSVField("F", "NFB/S"));
            // extra for Value
            Add(new CSVField("E", "VALUE"));
        }
    }

    /// <summary>
    ///     This class contains properties that a View can data bind to.
    ///     <para>
    ///         Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    ///     </para>
    ///     <para>
    ///         You can also use Blend to data bind with the tool's support.
    ///     </para>
    ///     <para>
    ///         See http://www.galasoft.ch/mvvm/getstarted
    ///     </para>
    /// </summary>
    public class OutputSettingsViewModel : ViewModelBase
    {
        private string _csvField1;
        private string _csvField2;
        private string _csvField3;
        private string _csvField4;
        private string _csvField5;
        private string _csvField6;
        private string _csvField7;
        private string _csvField8;
        private string _dateFormat;
        private string _delimiter;
        private string _format;
        private bool _indexValueAsVolume;
        private uint _indexValueDivisor;
        private bool _msSingleDirectory;
        private string _outputLocation;
        private OutputTo _outputTo;

        /// <summary>
        ///     Initializes a new instance of the OutputSettingsViewModel class.
        /// </summary>
        public OutputSettingsViewModel()
        {
            if (IsInDesignMode)
            {
                // Code runs in Blend --> create design time data.
            }
            else
            {
                // Code runs "for real": Connect to service, etc...
                // register to recieve messages
                Messenger.Default.Register<GetConvertParamMessage>(this, ReceiveGetParamMessage);
                Messenger.Default.Register<SaveOutputSettingsMessage>(this, OnReceiveSaveOutputSettingsMessage);
                Messenger.Default.Register<ShowFoldersDialogResultMessage>(this, OnRcvShowFoldersDialogResultMessage);

                RegisterCommands();

                var conf = new AppConfiguration();
                var serializer = new XmlSerializer(typeof (AppConfiguration));

                string xmlFilename = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\PSEGet.conf.xml";
                TextReader reader = new StreamReader(xmlFilename);
                conf = (AppConfiguration) serializer.Deserialize(reader);


                //S,D,O,H,L,C,V,F
                string[] csvFields = conf.CSVOutputFormat.Split(',');
                if (csvFields.Length == 0)
                    csvFields = "S,D,O,H,L,C,V,F".Split(',');

                CSVField1 = csvFields[0];
                CSVField2 = csvFields[1];
                CSVField3 = csvFields[2];
                CSVField4 = csvFields[3];
                CSVField5 = csvFields[4];
                CSVField6 = csvFields[5];
                CSVField7 = csvFields[6];
                CSVField8 = csvFields[7];

                OutputTo = OutputTo.Amibroker;
                OutputTo = conf.TargetOutput;
                OutputLocation = conf.OutputFolder; // ConfigurationManager.AppSettings["OutputLocation"];
                DateFormat = conf.CSVDateFormat; // ConfigurationManager.AppSettings["DateFormat"];
                Delimiter = conf.CSVDelimiter; // ConfigurationManager.AppSettings["CSVDelimiter"];
                Format = conf.CSVOutputFormat; // ConfigurationManager.AppSettings["CSVFormat"];
                DataConvertMethod = conf.DataSource;
                MSSingleDirectory = conf.MetastockSingleDirectory;
                    //ConfigurationManager.AppSettings["MetaStock_SingleDirectory"] == "Y";
                IndexValueAsVolume = conf.IndexValueAsVolume;
                    // ConfigurationManager.AppSettings["IndexValueAsVolume"] == "Y";

                //defaults
                if (OutputLocation == "")
                    OutputLocation = Path.GetDirectoryName(Application.ExecutablePath);

                if (DateFormat == "")
                    DateFormat = "MM/DD/YYYY";

                if (Delimiter == "")
                    Delimiter = ",";

                IndexValueDivisor = conf.IndexValueDivisor == 0 ? 1000 : conf.IndexValueDivisor;
                ReportFilenameFormat = conf.ReportFilenameFormat == null
                    ? "stockQuotes_%mm%dd%yyyy"
                    : conf.ReportFilenameFormat;
            }
        }

        public string ReportFilenameFormat { get; protected set; }

        public RelayCommand OpenFoldersDialogCommand { get; private set; }

        public RelayCommand<SelectionChangedEventArgs> ComboSelectionChangedCommand { get; private set; }

        public string CSVField1
        {
            get { return _csvField1; }
            set
            {
                if (_csvField1 == value)
                    return;
                _csvField1 = value;
                RaisePropertyChanged("CSVField1");
            }
        }

        public string CSVField2
        {
            get { return _csvField2; }
            set
            {
                if (_csvField2 == value)
                    return;
                _csvField2 = value;
                RaisePropertyChanged("CSVField2");
            }
        }

        public string CSVField3
        {
            get { return _csvField3; }
            set
            {
                if (_csvField3 == value)
                    return;
                _csvField3 = value;
                RaisePropertyChanged("CSVField3");
            }
        }

        public string CSVField4
        {
            get { return _csvField4; }
            set
            {
                if (_csvField4 == value)
                    return;
                _csvField4 = value;
                RaisePropertyChanged("CSVField4");
            }
        }

        public string CSVField5
        {
            get { return _csvField5; }
            set
            {
                if (_csvField5 == value)
                    return;
                _csvField5 = value;
                RaisePropertyChanged("CSVField5");
            }
        }

        public string CSVField6
        {
            get { return _csvField6; }
            set
            {
                if (_csvField6 == value)
                    return;
                _csvField6 = value;
                RaisePropertyChanged("CSVField6");
            }
        }

        public string CSVField7
        {
            get { return _csvField7; }
            set
            {
                if (_csvField7 == value)
                    return;
                _csvField7 = value;
                RaisePropertyChanged("CSVField7");
            }
        }

        public string CSVField8
        {
            get { return _csvField8; }
            set
            {
                if (_csvField8 == value)
                    return;
                _csvField8 = value;
                RaisePropertyChanged("CSVField8");
            }
        }

        public OutputTo OutputTo
        {
            get { return _outputTo; }
            set
            {
                //if (this._outputTo == value)
                //    return;
                if (value == OutputTo.Amibroker && !Helpers.IsAmibrokerInstalled())
                    return;
                _outputTo = value;
                RaisePropertyChanged("OutputTo");
            }
        }

        public string OutputLocation
        {
            get { return _outputLocation; }
            set
            {
                if (_outputLocation == value)
                    return;

                _outputLocation = value;
                RaisePropertyChanged("OutputLocation");
            }
        }

        public string DateFormat
        {
            get { return _dateFormat; }
            set
            {
                if (_dateFormat == value)
                    return;
                _dateFormat = value;
                RaisePropertyChanged("DateFormat");
            }
        }

        public string Delimiter
        {
            get { return _delimiter; }
            set
            {
                if (_delimiter == value)
                    return;
                _delimiter = value;
                RaisePropertyChanged("Delimiter");
            }
        }

        public string Format
        {
            get
            {
                _format = GetCSVFormat();
                _format = _format.Replace("}", string.Empty);
                _format = _format.Replace("{", string.Empty);
                return _format;
            }
            set
            {
                if (_format == value)
                    return;
                _format = value;
                RaisePropertyChanged("Format");
            }
        }

        public bool MSSingleDirectory
        {
            get { return _msSingleDirectory; }
            set
            {
                if (_msSingleDirectory == value)
                    return;
                _msSingleDirectory = value;
                RaisePropertyChanged("MSSingleDirectory");
            }
        }

        public bool IndexValueAsVolume
        {
            get { return _indexValueAsVolume; }
            set
            {
                if (_indexValueAsVolume == value)
                    return;
                _indexValueAsVolume = value;
                RaisePropertyChanged("IndexValueAsVolume");
            }
        }

        public uint IndexValueDivisor
        {
            get { return _indexValueDivisor; }
            set
            {
                if (_indexValueDivisor == value)
                {
                    return;
                }
                _indexValueDivisor = value;
                RaisePropertyChanged("IndexValueDivisor");
            }
        }

        public ConvertMethod DataConvertMethod { get; set; }

        private void RegisterCommands()
        {
            ComboSelectionChangedCommand = new RelayCommand<SelectionChangedEventArgs>(
                p => MessageBox.Show(p.AddedItems[0].ToString()));

            OpenFoldersDialogCommand = new RelayCommand(OnOpenFoldersDialogCommand);
        }

        private void OnRcvShowFoldersDialogResultMessage(MessageBase msg)
        {
            if (msg is ShowFoldersDialogResultMessage)
            {
                OutputLocation = (msg as ShowFoldersDialogResultMessage).SelectedPath;
            }
        }


        private void OnReceiveSaveOutputSettingsMessage(MessageBase msg)
        {
            if (msg is SaveOutputSettingsMessage)
            {
                ConfigurationManager.AppSettings["OutputLocation"] = OutputLocation;
                ConfigurationManager.AppSettings["DateFormat"] = DateFormat;
                ConfigurationManager.AppSettings["CSVDelimiter"] = Delimiter;
                ConfigurationManager.AppSettings["CSVFormat"] = Format;
                ConfigurationManager.AppSettings["MetaStock_SingleDirectory"] = MSSingleDirectory ? "Y" : "N";
                ConfigurationManager.AppSettings["IndexValueAsVolume"] = IndexValueAsVolume ? "Y" : "N";

                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                config.Save(ConfigurationSaveMode.Minimal);
                ConfigurationManager.RefreshSection("appSettings");
            }
        }

        private void ReceiveGetParamMessage(MessageBase message)
        {
            OutputSettings outputSettings = null;
            switch (OutputTo)
            {
                case OutputTo.CSV:
                    outputSettings = new CSVOutputSettings();
                    var csvOutputSettings = outputSettings as CSVOutputSettings;
                    csvOutputSettings.CSVFormat = GetCSVFormat();
                    csvOutputSettings.DateFormat = DateFormat;
                    csvOutputSettings.Delimiter = Delimiter;
                    csvOutputSettings.OutputDirectory = OutputLocation;
                    csvOutputSettings.UseSectorValueAsVolume = IndexValueAsVolume;
                    csvOutputSettings.SectorVolumeDivider = IndexValueDivisor;
                    csvOutputSettings.Filename = ReportFilenameFormat;
                    break;
                case OutputTo.Amibroker:
                    outputSettings = new AmiOutputSettings();
                    outputSettings.UseSectorValueAsVolume = IndexValueAsVolume;
                    outputSettings.SectorVolumeDivider = IndexValueDivisor;
                    (outputSettings as AmiOutputSettings).DatabaseDirectory = OutputLocation;
                    break;
                case OutputTo.Metastock:
                    outputSettings = new MetaOutputSettings();
                    ((MetaOutputSettings) outputSettings).OutputDirectory = OutputLocation;
                    ((MetaOutputSettings) outputSettings).UseSingleDirectory = MSSingleDirectory;
                    break;
            }

            Messenger.Default.Send(new OutputSettingsMessage {DataOutputSettings = outputSettings});
        }

        private string GetCSVFormat()
        {
            string result = "{" + CSVField1 + "}";
            result += ",{" + CSVField2 + "}";
            result += ",{" + CSVField3 + "}";
            result += ",{" + CSVField4 + "}";
            result += ",{" + CSVField5 + "}";
            result += ",{" + CSVField6 + "}";
            result += ",{" + CSVField7 + "}";
            result += ",{" + CSVField8 + "}";

            return result;
        }

        private void SaveSettings()
        {
        }

        private void OnOpenFoldersDialogCommand()
        {
            Messenger.Default.Send(
                new ShowFoldersDialogMessage {SelectedPath = OutputLocation}
                );
        }

        public override void Cleanup()
        {
            // Clean own resources if needed

            base.Cleanup();
        }
    }
}