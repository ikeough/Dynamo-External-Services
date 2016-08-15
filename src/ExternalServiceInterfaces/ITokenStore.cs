using System;
using System.Collections.Generic;

namespace Dynamo.ExternalServices
{
    public class OAuthToken : IToken
    {
        public string AccessToken { get; }
        public string State { get; }

        public OAuthToken(string token, string state = null)
        {
            AccessToken = token;
            State = state;
        }
    }

    public interface IToken
    {
        /// <summary>
        /// A string token provided by the authenticating service.
        /// </summary>
        string AccessToken { get;}

        /// <summary>
        /// An string state object provided by the authenticating service.
        /// Check the service's documentation to see whether a state will be
        /// provided. Not all services provide a state.
        /// </summary>
        string State { get;}
    }

    public interface IAccessTokens : IDictionary<string,IToken>
    {
        /// <summary>
        /// An event triggered when a token is added to the store is added to the collection.
        /// </summary>
        event Action<string, IToken> TokenAdded;

        /// <summary>
        /// An event triggered when a token is removed from the collection.
        /// </summary>
        event Action<string, IToken> TokenRemoved;
    }
}
