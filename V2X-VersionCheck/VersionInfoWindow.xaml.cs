using System.Windows;

namespace V2X_VersionCheck
{
    public partial class VersionInfoWindow : Window
    {
        public VersionInfoWindow(string line2, string line5)
        {
            InitializeComponent();
            VersionTextBlock.Text = $"Line 2: {line2}\nLine 5: {line5}";
        }
    }
}
