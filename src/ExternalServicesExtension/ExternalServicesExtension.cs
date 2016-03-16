using System;
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
