using System.Windows;
using MO_TERMINAL;  // Make sure this is included

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
            TracingWindow tracingWindow = new TracingWindow();
            tracingWindow.Show();

            // Close the current Dashboard window
            this.Close();
        }
    }
}
