using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Win32;
using PSEGet3.Messages;
using PSEGet3.Windows;

namespace PSEGet3.View
{
    /// <summary>
    ///     Interaction logic for ConverterView.xaml
    /// </summary>
    public partial class ConverterView : UserControl
    {
        public ConverterView()
        {
            InitializeComponent();
            Messenger.Default.Register<ShowBrowseDialogMessage>(this, OnReceiveMessage);
            Messenger.Default.Register<ShowProgressDialogMessage>(this, OnRcvShowProgressMessage);
        }

        private void OnRcvShowProgressMessage(MessageBase message)
        {
            if (message is ShowProgressDialogMessage)
            {
                var prog = new ProgressDialog();
                prog.Owner = Application.Current.MainWindow;
                prog.Show();
            }
        }

        private void OnReceiveMessage(MessageBase message)
        {
            if (message is ShowBrowseDialogMessage)
            {
                var openDialog = new OpenFileDialog();
                openDialog.Multiselect = true;
                openDialog.DefaultExt = "pdf";
                openDialog.Filter = "PDF Files (*.pdf)|*.pdf";
                if (openDialog.ShowDialog(Window.GetWindow(this)) == true)
                {
                    List<string> selectedFiles = openDialog.FileNames.ToList();
                    Messenger.Default.Send(new BrowseDialogResultMessage(selectedFiles));
                }
            }
        }
    }
}