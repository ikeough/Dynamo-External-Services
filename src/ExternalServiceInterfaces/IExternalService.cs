using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace ExternalServiceInterfaces
{
    public interface IExternalService: INotifyPropertyChanged
    {
        /// <summary>
        /// The Name of the service.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The OAuth access token.
        /// </summary>
        string AccessToken { get; set; }

        /// <summary>
        /// The current user name.
        /// </summary>
        string CurrentUserName { get; }

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
        /// Initialize the client object for the service given
        /// an access token and a state.
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="state"></param>
        void InitializeClient(string accessToken, string state);

        /// <summary>
        /// Extracts the authentication code from the 
        /// redirecte request.
        /// </summary>
        /// <param name="url">The Uri of the redirect request.</param>
        /// <returns></returns>
        string ExtractAuthorizationCodeFromRedirectRequest(Uri url);

        /// <summary>
        /// A method representing the authentication sequence.
        /// 
        /// Returns the access token.
        /// </summary>
        Task<string> GetAccessTokenAsync(string authorizationCode);

        /// <summary>
        /// Logout of the service.
        /// </summary>
        void Logout();
    }
}
