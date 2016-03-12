using System;

namespace ExternalServiceInterfaces
{
    public interface IExternalService<TAuthData>
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

        string ApiKey { get; }

        /// <summary>
        /// A method that is called during redirect.
        /// </summary>
        void ParseRedirectResponse(Uri uri);

        /// <summary>
        /// Login to the service.
        /// </summary>
        void Login();

        /// <summary>
        /// Logout of the service.
        /// </summary>
        void Logout();

        /// <summary>
        /// A method representing the authentication sequence.
        /// </summary>
        /// <returns>A <see cref="TAuthData"/> object.</returns>
        Action Authenticate { get; set; }

        /// <summary>
        /// Get the client object provided by the service's API.
        /// </summary>
        object GetClient();
    }
}
