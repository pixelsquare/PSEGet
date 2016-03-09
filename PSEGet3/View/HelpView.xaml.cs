using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace PSEGet3.View
{
    /// <summary>
    ///     Interaction logic for HelpView.xaml
    /// </summary>
    public partial class HelpView : UserControl
    {
        public HelpView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            TextRange textRange;
            FileStream fileStream;
            string readMeFile = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\README.rtf";
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);

            if (File.Exists(readMeFile))
            {
                textRange = new TextRange(richTextBox1.Document.ContentStart, richTextBox1.Document.ContentEnd);
                using (fileStream = new FileStream(readMeFile, FileMode.OpenOrCreate))
                {
                    textRange.Load(fileStream, DataFormats.Rtf);
                }
            }
        }
    }
}