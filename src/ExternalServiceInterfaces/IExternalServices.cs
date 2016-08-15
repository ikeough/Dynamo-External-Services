using System.Collections.Generic;

namespace Dynamo.ExternalServices
{
    public interface IExternalServices<TService>
    {
        /// <summary>
        /// A collection of <see cref="TService"/>.
        /// </summary>
        IEnumerable<TService> Services { get; }

        /// <summary>
        /// Add a <see cref="TService"/> to the collection.
        /// </summary>
        /// <param name="service"></param>
        void AddService(TService service);

        /// <summary>
        /// Remove a <see cref="TService"/> from the collection.
        /// </summary>
        /// <param name="service"></param>
        void RemoveService(TService service);

        /// <summary>
        /// Get a <see cref="TService"/> by name from the collection.
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        TService GetServiceByName(string serviceName);
    }
}
