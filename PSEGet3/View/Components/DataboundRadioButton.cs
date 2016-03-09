using System.Windows;
using System.Windows.Controls;

namespace PSEGet3.View.Components
{
    public class DataBoundRadioButton : RadioButton
    {
        protected override void OnChecked(RoutedEventArgs e)
        {
            //System.Windows.Forms.MessageBox.Show(e.ToString());
            // Do nothing. This will prevent IsChecked from being manually set and overwriting the binding.
        }

        protected override void OnToggle()
        {
            IsChecked = !IsChecked;
            // Do nothing. This will prevent IsChecked from being manually set and overwriting the binding.
        }
    }
}