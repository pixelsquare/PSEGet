using System.Windows;

namespace PSEGet3.Windows
{
    /// <summary>
    ///     Description for ErrorWindow.
    /// </summary>
    public partial class MessageWindow : Window
    {
        /// <summary>
        ///     Initializes a new instance of the ErrorWindow class.
        /// </summary>
        public MessageWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}