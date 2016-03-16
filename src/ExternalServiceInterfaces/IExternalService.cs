using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace ExternalServiceInterfaces
{
    public interface IExternalService<TAuthData>: INotifyPropertyChanged
    {
        /// <summary>
        /// The Name of the service.
        /// </summary>
        string Name { get; }

        TAuthData AuthenticationData { get; }

        /// <summary>
        /// The Uri for authorization.
        /// </summary>
        Uri AuthorizationUri { get; }

        /// <summary>
        /// The Uri to which the system will be
        /// redirected after authorization.
        /// </summary>
        Uri RedirectUri { get; }

        /// <summary>
        /// The client object provided by the service.
        /// </summary>
        object Client { get; }

        /// <summary>
        /// A method that is called during redirect.
        /// </summary>
        void ParseRedirectResponse(Uri uri);

        /// <summary>
        /// Login to the service.
        /// </summary>
        Task LoginAsync();

        /// <summary>
        /// Logout of the service.
        /// </summary>
        void Logout();

        /// <summary>
        /// A method representing the authentication sequence.
        /// </summary>
        /// <returns>A <see cref="TAuthData"/> object.</returns>
        Func<Task<TAuthData>> AuthenticateAsync { get; set; }
    }
}
