using System.Windows.Controls;

namespace PlutoniumEasyInstaller
{
    /// <summary>
    /// Interaction logic for NavigationPage.xaml
    /// </summary>
    public partial class NavigationPage : Page
    {
        protected MainWindow _window;
        public virtual MainWindow Window
        {
            get => _window;
            set
            {
                _window = value;

                value.NextButtonPressed += OnNextButtonPressed;
                OnWindowSet();
            }
        }

        protected virtual void OnWindowSet()
        {

        }

        protected virtual void OnNextButtonPressed()
        {
            
        }

        public virtual void OnNavigated()
        {

        }

        public void Unlink()
        {
            Window.NextButtonPressed -= OnNextButtonPressed;
        }
    }
}
