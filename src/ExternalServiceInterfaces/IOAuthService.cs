namespace ExternalServiceInterfaces
{
    public interface IOAuthService : IExternalService<IOAuthAuthenticationData> { }

    /// <summary>
    /// Data returned from an authentication operation
    /// </summary>
    public interface IOAuthAuthenticationData
    {
        /// <summary>
        /// The current access token.
        /// </summary>
        string AccessToken { get; }

        /// <summary>
        /// The current user name.
        /// </summary>
        string CurrentUserName { get; }
    }
}
