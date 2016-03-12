using System.Collections.Generic;

namespace ExternalServiceInterfaces
{
    /// <summary>
    /// OAuthServices is a singleton which is backed by a collection of IOAuthServices.
    /// </summary>
    public class OAuthServices : IExternalServices<IOAuthService>
    {
        private static OAuthServices instance;
        private Dictionary<string, IOAuthService> services = new Dictionary<string, IOAuthService>();
         
        public static OAuthServices Instance
        {
            get
            {
                instance = instance ?? new OAuthServices();
                return instance;
            }
        }

        private OAuthServices()
        {
            
        }

        public IEnumerable<IOAuthService> Services
        {
            get { return services.Values; }
        }

        public void AddService(IOAuthService service)
        {
            if (!services.ContainsKey(service.Name))
            {
                services.Add(service.Name, service);
            }
        }

        public void RemoveService(IOAuthService service)
        {
            if (services.ContainsKey(service.Name))
            {
                services.Remove(service.Name);
            }
        }

        public IOAuthService GetServiceByName(string serviceName)
        {
            return services.ContainsKey(serviceName) ? services[serviceName] : null;
        }
    }
}
