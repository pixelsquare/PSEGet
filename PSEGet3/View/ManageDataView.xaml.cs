using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace PSEGet3.View
{
    /// <summary>
    ///     Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class ManageDataView : UserControl
    {
        public ManageDataView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            Process.Start(Path.GetDirectoryName(assembly.Location) + @"\Reports");
        }
    }
}