using System.Windows;
using MO_TERMINAL; 
using WpfApp1.Views.TrFilesSync;  
using WpfApp1.Views.V2XVersionCheck;



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
            MO_TERMINAL.TracingWindow TW_obj = new MO_TERMINAL.TracingWindow();
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

        private void V2XVersionCheck_Click(object sender, RoutedEventArgs e)
        {
            // Open the TracingWindow
            V2XVersionCheck_MainWindow V2X_obj = new V2XVersionCheck_MainWindow();
            V2X_obj.Show();

            // Close the current Dashboard window
            this.Close();
        }


    }
}
