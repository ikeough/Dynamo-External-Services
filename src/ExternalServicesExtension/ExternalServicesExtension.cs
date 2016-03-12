using System;
using DropboxService;
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
            var dropBox = new DropBoxService { Authenticate = Authenticate };
            
            OAuthServices.Instance.AddService(dropBox);
        }

        private void Authenticate()
        {
            return;
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
