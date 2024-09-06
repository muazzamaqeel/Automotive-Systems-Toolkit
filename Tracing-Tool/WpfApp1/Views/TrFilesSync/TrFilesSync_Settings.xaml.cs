using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.Windows;
using WpfApp1.Views.TrFilesSync; // or the correct namespace where SettingsWindow is defined



namespace WpfApp1.Views.TrFilesSync
{

    public partial class TrFilesSync_Settings : Window
    {
        private string authTokenPath = "authToken.txt"; // Relative path

        public TrFilesSync_Settings()
        {
            InitializeComponent();
            LoadAuthToken();
        }

        private void LoadAuthToken()
        {
            if (File.Exists(authTokenPath))
            {
                AuthTokenTextBox.Text = File.ReadAllText(authTokenPath);
            }
            else
            {
                MessageBox.Show("Auth Token file not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            File.WriteAllText(authTokenPath, AuthTokenTextBox.Text);
            MessageBox.Show("Auth Token saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }
    }
}
