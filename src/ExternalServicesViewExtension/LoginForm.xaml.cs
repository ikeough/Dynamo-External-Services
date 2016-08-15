using System;
using System.Windows;
using System.Windows.Navigation;
using Dynamo.ExternalServices.Tokens;

namespace Dynamo.ExternalServices.Extensions
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class LoginForm : Window
    {
        private IExternalService service;

        public LoginForm(IExternalService service)
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
                FinishAuthentication(e.Uri);
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

        private async void FinishAuthentication(Uri uri)
        {
            var authenticationCode = service.ExtractAuthorizationCodeFromRedirectRequest(uri);
            var token = await service.GetAccessTokenAsync(authenticationCode);
            service.AccessToken = token;

            // Add the token to the token store.
            AccessTokens.Instance.Add(service.Name, new OAuthToken(token));
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
