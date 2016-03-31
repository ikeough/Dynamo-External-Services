using System;
using System.Linq;
using System.Threading.Tasks;
using Dynamo.Extensions;
using ExternalServiceInterfaces;

namespace ExternalServicesExtension
{
    public class ExternalServicesExtension : IExtension
    {
        public void Dispose()
        {
        }

        public void Startup(StartupParams sp)
        {
        }

        public void Ready(ReadyParams sp)
        {
            var dropBox = new DropboxService.DropboxService { AuthenticateAsync = Authenticate };
            
            OAuthServices.Instance.AddService(dropBox);

            ExternalServicesTokens.AccessTokenStore.Instance.TokenAdded += Instance_TokenAdded;
        }

        private void Instance_TokenAdded(IToken token)
        {
            var serviceMatch = OAuthServices.Instance.Services.FirstOrDefault(s => s.Name == token.Kind);
            if (serviceMatch == null) return;

            serviceMatch.InitializeClient(token.AccessToken, token.State);
        }

        private Task<IOAuthAuthenticationData> Authenticate()
        {
            return null;
        }

        public void Shutdown()
        {
            foreach (var s in OAuthServices.Instance.Services)
            {
                s.Logout();
            }
        }

        public string UniqueId
        {
            get { return Guid.NewGuid().ToString(); } 
        }

        public string Name
        {
            get { return "External Services"; }
        }
    }
}
