using System;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Dropbox.Api;
using ExternalServiceInterfaces;

namespace DropboxService
{
    public class DropboxService : IOAuthService
    {
        #region private members

        private string oauth2State;
        private IOAuthAuthenticationData authenticationData;
        private DropboxClient client;
        private string apiKey;

        #endregion

        #region public events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region public methods

        public string Name
        {
            get { return "Dropbox"; }
        }

        public IOAuthAuthenticationData AuthenticationData
        {
            get { return authenticationData; }
            private set
            {
                authenticationData = value;
                OnPropertyChanged("AuthenticationData");
            }
        }

        public Uri AuthorizationUri { get; }

        public Uri RedirectUri { get; private set; }

        public object Client
        {
            get
            {
                return client;
            }
        }

        public DropboxService()
        {
            var redirectVar = Environment.GetEnvironmentVariable("DROPBOX_REDIRECT_URI");
            RedirectUri = new Uri(string.IsNullOrEmpty(redirectVar)?"":redirectVar);

            apiKey = Environment.GetEnvironmentVariable("DROPBOX_API_KEY");
            this.oauth2State = Guid.NewGuid().ToString("N");
            AuthorizationUri = DropboxOAuth2Helper.GetAuthorizeUri(OAuthResponseType.Token, apiKey, RedirectUri, oauth2State);
        }

        public Func<Task<IOAuthAuthenticationData>> AuthenticateAsync { get; set; }

        public async void ParseRedirectResponse(Uri uri)
        {
            var result = DropboxOAuth2Helper.ParseTokenFragment(uri);
            if (result.State != this.oauth2State)
            {
                return;
            }

            client = (DropboxClient)GetClient(result.AccessToken);

            var full = await client.Users.GetCurrentAccountAsync();
            AuthenticationData = new OAuthAuthenticationData(result.AccessToken, full.Email);
        }

        /// <summary>
        /// Gets the dropbox access token.
        /// <para>
        /// This fetches the access token from the applications settings, if it is not found there
        /// (or if the user chooses to reset the settings) then the UI in <see cref="LoginForm"/> is
        /// displayed to authorize the user.
        /// </para>
        /// </summary>
        /// <returns>A valid access token or null.</returns>
        public async Task LoginAsync()
        {
            try
            {
                if (AuthenticationData != null)
                {
                    return;
                }

                if (AuthenticateAsync == null)
                {
                    throw new Exception("No authentication method has been provided.");
                }

                AuthenticationData = await AuthenticateAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void Logout()
        {
            client = null;
            AuthenticationData = null;
        }

        #endregion

        #region private methods

        private static object GetClient(string accessToken)
        {
            InitializeCertPinning();

            // Specify socket level timeout which decides maximum waiting time when on bytes are
            // received by the socket.
            var httpClient = new HttpClient(new WebRequestHandler { ReadWriteTimeout = 10 * 1000 })
            {
                // Specify request level timeout which decides maximum time taht can be spent on
                // download/upload files.
                Timeout = TimeSpan.FromMinutes(20)
            };

            try
            {
                var config = new DropboxClientConfig("Dynamo")
                {
                    HttpClient = httpClient
                };

                var client = new DropboxClient(accessToken, config);
                return client;
            }
            catch (HttpException e)
            {
                Console.WriteLine("Exception reported from RPC layer");
                Console.WriteLine("    Status code: " + e.StatusCode);
                Console.WriteLine("    Message    : " + e.Message);
                if (e.RequestUri != null)
                {
                    Console.WriteLine("    Request uri: {0}", e.RequestUri);
                }
            }

            return null;
        }

        /// <summary>
        /// Initializes ssl certificate pinning.
        /// </summary>
        private static void InitializeCertPinning()
        {
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
            {
                var root = chain.ChainElements[chain.ChainElements.Count - 1];
                var publicKey = root.Certificate.GetPublicKeyString();

                return DropboxCertHelper.IsKnownRootCertPublicKey(publicKey);
            };
        }

        #endregion

        #region protected methods

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    public class OAuthAuthenticationData : IOAuthAuthenticationData
    {
        public string AccessToken { get; }
        public string CurrentUserName { get; }

        public OAuthAuthenticationData(string accessToken, string currentUserName)
        {
            AccessToken = accessToken;
            CurrentUserName = currentUserName;
        }
    }
}
