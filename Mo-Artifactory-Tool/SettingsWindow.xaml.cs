using System.IO;
using System.Windows;

namespace Mo_Artifactory_Tool
{
    public partial class SettingsWindow : Window
    {
        private string authTokenPath = "authToken.txt"; // Relative path

        public SettingsWindow()
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
