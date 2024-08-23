using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace Mo_Artifactory_Tool
{
    public partial class MainWindow : Window
    {
        private string authTokenPath = "authToken.txt"; // Relative path
        private static readonly HttpClient client = new HttpClient();
        private string baseUrl = "https://artifactory-fr.harman.com/artifactory"; // Base URL for Artifactory API
        private string repoName = "mqb-conmod-harman"; // Repository name
        private string localRootFolder = @"C:\CONMOD-Software\TR-Files"; // Local root folder for storing .tr files

        public MainWindow()
        {
            InitializeComponent();
            LoadAuthToken();
        }

        private void LoadAuthToken()
        {
            try
            {
                if (File.Exists(authTokenPath))
                {
                    string authToken = File.ReadAllText(authTokenPath);
                    if (!string.IsNullOrWhiteSpace(authToken))
                    {
                        Log("Auth Token Loaded Successfully.");
                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);
                    }
                    else
                    {
                        Log("Auth Token is missing. Please update it in the settings.");
                    }
                }
                else
                {
                    Log($"Looking for authToken at: {Path.GetFullPath(authTokenPath)}");
                }
            }
            catch (Exception ex)
            {
                Log($"Error loading auth token: {ex.Message}");
            }
        }

        private async void SyncButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsAuthTokenValid())
            {
                Log("Sync .tr Files action initiated.");
                await SyncRepository($"{baseUrl}/api/storage/{repoName}/RC");
                Log("Sync .tr Files action completed.");
            }
        }

        private async Task SyncRepository(string path)
        {
            Log($"Scanning {path}...");
            try
            {
                string apiUrl = $"{path}?list&deep=1"; // API call to list files and directories
                var response = await client.GetStringAsync(apiUrl);

                var directoryListing = JsonSerializer.Deserialize<ArtifactoryFileList>(response);

                foreach (var file in directoryListing.files)
                {
                    if (file.uri.EndsWith(".tr")) // Only process .tr files
                    {
                        string filePath = $"{path}/{file.uri.TrimStart('/')}";
                        await DownloadFile(filePath);
                    }
                    else if (file.folder) // If it's a directory, recurse into it
                    {
                        string newPath = $"{path}/{file.uri.TrimStart('/')}";
                        await SyncRepository(newPath);
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"Failed to sync {path}: {ex.Message}");
            }
        }

        private async Task DownloadFile(string filePath)
        {
            string relativePath = filePath.Replace($"{baseUrl}/api/storage/", "");
            string localPath = Path.Combine(localRootFolder, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(localPath));

            Log($"Downloading {filePath}...");

            try
            {
                var fileUrl = filePath.Replace("/api/storage/", "/");
                byte[] fileData = await client.GetByteArrayAsync(fileUrl);

                await File.WriteAllBytesAsync(localPath, fileData);

                Log($"Downloaded {filePath} to {localPath}");
            }
            catch (Exception ex)
            {
                Log($"Failed to download {filePath}: {ex.Message}");
            }
        }

        private async void ConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsAuthTokenValid())
            {
                Log("Repo. Connection Check action initiated.");
                bool connectionSuccessful = await TestRepoConnection();
                if (connectionSuccessful)
                {
                    Log("Repository connection successful.");
                }
                else
                {
                    Log("Repository connection failed. Please check your auth token.");
                }
            }
        }

        private async Task<bool> TestRepoConnection()
        {
            try
            {
                // Make an API call to verify the connection to the repository
                string apiUrl = $"{baseUrl}/api/repositories/{repoName}";
                var response = await client.GetAsync(apiUrl);

                return response.IsSuccessStatusCode; // Returns true if the status code indicates success (e.g., 200 OK)
            }
            catch (Exception ex)
            {
                Log($"Error testing repository connection: {ex.Message}");
                return false;
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.ShowDialog();
            LoadAuthToken(); // Reload token after settings are updated
        }

        private bool IsAuthTokenValid()
        {
            if (File.Exists(authTokenPath))
            {
                string authToken = File.ReadAllText(authTokenPath);
                if (!string.IsNullOrWhiteSpace(authToken))
                {
                    return true;
                }
            }
            Log("Auth Token is missing. Please update it in the settings.");
            return false;
        }

        private void Log(string message)
        {
            LogTextBox.AppendText($"{DateTime.Now}: {message}{Environment.NewLine}");
            LogTextBox.ScrollToEnd();
        }
    }

    public class ArtifactoryFileList
    {
        public List<ArtifactoryFile> files { get; set; }
    }

    public class ArtifactoryFile
    {
        public string uri { get; set; }
        public bool folder { get; set; }
    }
}
