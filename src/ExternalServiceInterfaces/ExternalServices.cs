using System.Collections.Generic;

namespace Dynamo.ExternalServices
{
    /// <summary>
    /// ExternalServices is a singleton which is backed by a collection of IExternalServices.
    /// </summary>
    public class ExternalServices : IExternalServices<IExternalService>
    {
        private static ExternalServices instance;
        private Dictionary<string, IExternalService> services = new Dictionary<string, IExternalService>();
         
        public static ExternalServices Instance
        {
            get
            {
                instance = instance ?? new ExternalServices();
                return instance;
            }
        }

        private ExternalServices()
        {
            
        }

        public IEnumerable<IExternalService> Services
        {
            get { return services.Values; }
        }

        public void AddService(IExternalService service)
        {
            if (!services.ContainsKey(service.Name))
            {
                services.Add(service.Name, service);
            }
        }

        public void RemoveService(IExternalService service)
        {
            if (services.ContainsKey(service.Name))
            {
                services.Remove(service.Name);
            }
        }

        public IExternalService GetServiceByName(string serviceName)
        {
            return services.ContainsKey(serviceName) ? services[serviceName] : null;
        }
    }
}
