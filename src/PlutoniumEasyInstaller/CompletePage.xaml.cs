using System.Windows;
using System.Windows.Controls;

namespace PlutoniumEasyInstaller
{
    /// <summary>
    /// Interaction logic for CompletePage.xaml
    /// </summary>
    public partial class CompletePage : Page
    {
        public CompletePage()
        {
            InitializeComponent();
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            PlutoniumSetup.StartPlutoniumLauncher();
            Application.Current.Shutdown();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
