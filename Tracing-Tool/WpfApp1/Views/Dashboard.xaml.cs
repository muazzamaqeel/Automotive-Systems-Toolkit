using System.Windows;
using MO_TERMINAL;
using WpfApp1.Views.TrFilesSync;  // Make sure this is included

namespace WpfApp1.Views
{
    public partial class Dashboard : Window
    {
        public Dashboard()
        {
            InitializeComponent();
        }

        private void TracingButton_Click(object sender, RoutedEventArgs e)
        {
            // Open the TracingWindow
            TracingWindow TW_obj = new TracingWindow();
            TW_obj.Show();

            // Close the current Dashboard window
            this.Close();
        }

        private void TrFilesButton_Click(object sender, RoutedEventArgs e)
        {
            // Open the TracingWindow
            TrFilesSync_MainWindow TRF_obj = new TrFilesSync_MainWindow();
            TRF_obj.Show();

            // Close the current Dashboard window
            this.Close();
        }

        
    }
}
