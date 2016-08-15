using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Dynamo.Extensions;
using Dynamo.ExternalServices.Tokens;

namespace Dynamo.ExternalServices.Extensions
{
    public class ExternalServicesExtension : IExtension
    {
        public void Dispose()
        {
        }

        public void Startup(StartupParams sp)
        {
            RegisterServices(sp.PathManager.DynamoCoreDirectory);
        }

        public void Ready(ReadyParams sp)
        {
            AccessTokens.Instance.TokenAdded += InitializeClientForToken;
        }

        public void Shutdown()
        {
            foreach (var s in ExternalServices.Instance.Services)
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

        /// <summary>
        /// Called when the extension is ready. Used to find service implementations
        /// and register them with the services collection. Service implementations
        /// will not be initialized until an access token is added to the token store.
        /// </summary>
        private void RegisterServices(string dynamoCoreDir)
        {
            var servicesDir = Path.Combine(dynamoCoreDir, "services");

            if (!Directory.Exists(servicesDir)) return;
            
            foreach (var di in new DirectoryInfo(servicesDir).GetDirectories())
            {
                var implName = "Dynamo.ExternalServices." + di.Name + ".dll";
                var implPath = Path.Combine(di.FullName, implName);

                if (!File.Exists(implPath))
                {
                    continue;
                }

                var asm = Assembly.LoadFrom(implPath);
                var serviceImpls = asm.GetTypes().Where(t => typeof(IExternalService).IsAssignableFrom(t)).ToArray();
                if (!serviceImpls.Any()) return;

                foreach (var impl in serviceImpls)
                {
                    try
                    {
                        var service = (IExternalService)Activator.CreateInstance(AppDomain.CurrentDomain, asm.FullName, impl.FullName).Unwrap();
                        ExternalServices.Instance.AddService(service);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("The external service, {0} could not be registered.", impl.FullName);
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Handler for the TokenAdded event on ITokenStore
        /// </summary>
        /// <param name="token"></param>
        private void InitializeClientForToken(string name, IToken token)
        {
            var serviceMatch = ExternalServices.Instance.Services.FirstOrDefault(s => s.Name == name);
            serviceMatch?.InitializeClient(token.AccessToken, token.State);
        }
    }
}
