using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace PlutoniumEasyInstaller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public event Action NextButtonPressed;
        public bool EnableNextButton { set => NextButton.IsEnabled = value; }
        public bool EnableBackButton { set => BackButton.IsEnabled = value; }
        public bool EnableNavigationBar
        {
            set
            {
                if (value)
                    NavigationBar.Visibility = Visibility.Visible;
                else
                    NavigationBar.Visibility = Visibility.Collapsed;
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            if (App.SingleInstance)
                ForceSingleInstance();

            PageViewer.Navigated += PageViewer_Navigated;

            Navigate(new MainPage());
        }

        public void Navigate(Page page)
        {
            if (PageViewer.Content is NavigationPage previousPage)
                previousPage.Unlink();

            if (page is NavigationPage navPage)
                navPage.Window = this;

            //UpdateNavigationButtons();

            PageViewer.Navigate(page);
        }

        private void ForceSingleInstance()
        {
            string currentProcessName = Process.GetCurrentProcess().ProcessName;

            // get the list of all processes by the "procName"       
            if (Process.GetProcessesByName(currentProcessName).Length > 1)
            {
                MessageBox.Show(Properties.Resources.AlreadyRunningWarning);
                Application.Current.Shutdown();
            }
        }

        private void PageViewer_Navigated(object sender, NavigationEventArgs e)
        {
            if (e.Content is NavigationPage navPage)
            {
                navPage.OnNavigated();
            }
        }

        private void UpdateNavigationButtons()
        {
            BackButton.IsEnabled = PageViewer.CanGoBack;
            NextButton.IsEnabled = false;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            UpdateNavigationButtons();

            if (PageViewer.CanGoBack)
            {
                PageViewer.GoBack();
                PageViewer.Navigated += PageViewer_Navigated;
            }
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            UpdateNavigationButtons();

            NextButtonPressed?.Invoke();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
