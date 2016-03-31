using System;
using System.Collections.Generic;
using ExternalServiceInterfaces;

namespace ExternalServicesTokens
{
    public class AccessTokenStore : IAccessTokenStore
    {
        private Dictionary<string, IToken> tokens = new Dictionary<string, IToken>();
        private static AccessTokenStore instance;

        public event Action<IToken> TokenAdded;
        public event Action<IToken> TokenRemoved;

        private void OnTokenAdded(IToken token)
        {
            if (TokenAdded != null)
            {
                TokenAdded(token);
            }
        }

        private void OnTokenRemoved(IToken token)
        {
            if (TokenRemoved != null)
            {
                TokenRemoved(token);
            }
        }

        public void AddToken(IToken token)
        {
            if (!tokens.ContainsKey(token.Kind))
            {
                tokens.Add(token.Kind, token);
                OnTokenAdded(token);
            }
        }

        public void RemoveToken(string kind)
        {
            if (tokens.ContainsKey(kind))
            {
                var token = tokens[kind];
                tokens.Remove(kind);
                OnTokenRemoved(token);
            }
        }

        public IToken GetToken(string kind)
        {
            return tokens.ContainsKey(kind) ? tokens[kind] : null;
        }

        public static AccessTokenStore Instance
        {
            get { return instance ?? (instance = new AccessTokenStore()); }
        }
    }
}
