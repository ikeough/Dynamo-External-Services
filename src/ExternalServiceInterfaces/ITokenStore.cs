using System;

namespace ExternalServiceInterfaces
{
    public interface IToken
    {
        /// <summary>
        /// The Kind of token is 'dropbox', or 'twitter', etc.
        /// </summary>
        string Kind { get; set; }

        /// <summary>
        /// A string token provided by the authenticating service.
        /// </summary>
        string AccessToken { get; set; }

        /// <summary>
        /// An string state object provided by the authenticating service.
        /// Check the service's documentation to see whether a state will be
        /// provided. Not all services provide a state.
        /// </summary>
        string State { get; set; }
    }

    public interface IAccessTokenStore
    {
        /// <summary>
        /// Add a <see cref="IToken"/> to the store.
        /// </summary>
        /// <param name="token"></param>
        void AddToken(IToken token);

        /// <summary>
        /// Remove a <see cref="IToken"/> from the store.
        /// </summary>
        /// <param name="token">The name of the token to remove.</param>
        void RemoveToken(string kind);

        /// <summary>
        /// Get a <see cref="IToken"/> by name.
        /// </summary>
        /// <param name="kind"></param>
        /// <returns>An <see cref="IToken"/> or null if none is found.</returns>
        IToken GetToken(string kind);

        /// <summary>
        /// An event triggered when a <see cref="IToken"/> is added to the store.
        /// </summary>
        event Action<IToken> TokenAdded;

        /// <summary>
        /// An event triggered when a <see cref="IToken"/> is removed from the store.
        /// </summary>
        event Action<IToken> TokenRemoved;
    }
}
