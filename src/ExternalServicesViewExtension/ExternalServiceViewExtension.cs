using System;
using Dynamo.Wpf.Extensions;
using ExternalServiceInterfaces;

namespace ExternalServicesViewExtension
{
    public class ExternalServicesViewExtension : IViewExtension
    {
        public void Dispose()
        {
            
        }

        public void Startup(ViewStartupParams p)
        {
            
        }

        public void Loaded(ViewLoadedParams p)
        {
            // Add a menu item for each of the loaded services
            foreach (var service in OAuthServices.Instance.Services)
            {
                var login = new LoginForm(service);
                login.Show();
            }
        }

        public void Shutdown()
        {
            
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
