using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using GalaSoft.MvvmLight.Messaging;
using PSEGet3.Messages;
using ComboBox = System.Windows.Controls.ComboBox;
using UserControl = System.Windows.Controls.UserControl;

namespace PSEGet3.View
{
    /// <summary>
    ///     Interaction logic for OutputSettingsView.xaml
    /// </summary>
    public partial class OutputSettingsView : UserControl
    {
        public static readonly DependencyProperty CSVOptionOnlyProperty =
            DependencyProperty.Register("CSVOptionOnly", typeof (bool), typeof (OutputSettingsView));

        public OutputSettingsView()
        {
            InitializeComponent();
            Messenger.Default.Register<ShowFoldersDialogMessage>(this, OnShowFoldersDialogMessage);
        }

        public bool CSVOptionOnly
        {
            get { return (bool) GetValue(CSVOptionOnlyProperty); }
            set
            {
                if (value)
                    radioCSV.IsChecked = value;
                radioAmibroker.IsEnabled = !value;
                radioMetastock.IsEnabled = !value;
                SetValue(CSVOptionOnlyProperty, value);
            }
        }

        private void OnShowFoldersDialogMessage(MessageBase msg)
        {
            if (msg is ShowFoldersDialogMessage)
            {
                var foldersDialog = new FolderBrowserDialog();
                foldersDialog.SelectedPath = (msg as ShowFoldersDialogMessage).SelectedPath;
                foldersDialog.ShowNewFolderButton = true;
                if (foldersDialog.ShowDialog() == DialogResult.OK)
                {
                    Messenger.Default.Send(
                        new ShowFoldersDialogResultMessage(foldersDialog.SelectedPath)
                        );
                }
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void InitComboBox(ComboBox comboBox)
        {
            //comboBox.Items.Add(new ComboBoxItem() { Content = "SYMBOL" });
            //comboBox.Items.Add(new ComboBoxItem() { Content = "DATE" });
            //comboBox.Items.Add(new ComboBoxItem() { Content = "OPEN" });
            //comboBox.Items.Add(new ComboBoxItem() { Content = "HIGH" });
            //comboBox.Items.Add(new ComboBoxItem() { Content = "LOW" });
            //comboBox.Items.Add(new ComboBoxItem() { Content = "CLOSE" });
            //comboBox.Items.Add(new ComboBoxItem() { Content = "VOLUME" });
            //comboBox.Items.Add(new ComboBoxItem() { Content = "NFB/S" });
        }

        private void Combo1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
    }
}