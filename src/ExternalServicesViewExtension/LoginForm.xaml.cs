using System;
using System.Windows;
using System.Windows.Navigation;
using ExternalServiceInterfaces;

namespace ExternalServicesViewExtension
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class LoginForm : Window
    {
        private IExternalService<IOAuthAuthenticationData> service;

        public LoginForm(IExternalService<IOAuthAuthenticationData> service)
        {
            InitializeComponent();

            this.service = service;
            DataContext = service;
            Dispatcher.BeginInvoke(new Action<Uri>(this.Start), service.AuthorizationUri);
        }

        private void Start(Uri authorizeUri)
        {
            this.Browser.Navigate(authorizeUri);
        }

        protected virtual void BrowserNavigating(object sender, NavigatingCancelEventArgs e)
        {
            if (!e.Uri.ToString().StartsWith(service.RedirectUri.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                // we need to ignore all navigation that isn't to the redirect uri.
                return;
            }
            try
            {
                service.ParseRedirectResponse(e.Uri);
            }
            catch (ArgumentException)
            {
                // There was an error in the URI passed to ParseTokenFragment
            }
            finally
            {
                e.Cancel = true;
                this.Close();
            }
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
