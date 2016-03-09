using System;
using System.Windows.Controls;

namespace PSEGet3.View
{
    /// <summary>
    ///     Interaction logic for MarketSummaryView.xaml
    /// </summary>
    public partial class MarketSummaryView : UserControl
    {
        public MarketSummaryView()
        {
            InitializeComponent();
        }

        public DateTime TradeDate
        {
            get
            {
                if (tradeDate.SelectedDate.HasValue)
                    return (DateTime) tradeDate.SelectedDate;
                return DateTime.Today;
            }
            set { tradeDate.SelectedDate = value; }
        }
    }
}