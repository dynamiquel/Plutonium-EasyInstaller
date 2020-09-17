namespace PlutoniumEasyInstaller
{
    /// <summary>
    /// Interaction logic for PlutoniumConfigPage.xaml
    /// </summary>
    public partial class PlutoniumConfigPage : NavigationPage
    {
        public string BO2Directory;
        private bool loaded;

        public PlutoniumConfigPage()
        {
            InitializeComponent();

            DesktopShortcutCheckBox.IsChecked = !App.NoDesktopShortcut;
            StartShortcutCheckBox.IsChecked = !App.NoStartShortcut;
            ReShadeCheckBox.IsChecked = !App.NoReShade;        
        }

        private void PageLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (loaded)
                return;

            if (App.AutoConfig)
                OnNextButtonPressed();

            loaded = true;
        }

        protected override void OnWindowSet()
        {
            
        }

        protected override void OnNextButtonPressed()
        {
            var plutoniumInstallPage = new PlutoniumInstallPage
            {
                BO2Directory = BO2Directory
            };

            Window.Navigate(plutoniumInstallPage);
            plutoniumInstallPage.Start(
                (bool)DesktopShortcutCheckBox.IsChecked,
                (bool)StartShortcutCheckBox.IsChecked,
                (bool)ReShadeCheckBox.IsChecked);
        }

        public override void OnNavigated()
        {
            Window.EnableNextButton = true;
            Window.EnableNavigationBar = true;
        }
    }
}
