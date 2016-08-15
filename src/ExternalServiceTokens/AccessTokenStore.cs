using System;
using System.Collections;
using System.Collections.Generic;

namespace Dynamo.ExternalServices.Tokens
{
    public class AccessTokens : IAccessTokens
    {
        private Dictionary<string, IToken> tokens = new Dictionary<string, IToken>();
        private static AccessTokens instance;

        public event Action<string, IToken> TokenAdded;
        public event Action<string, IToken> TokenRemoved;

        public static AccessTokens Instance
        {
            get
            {
                if (instance != null) return instance;
                instance = new AccessTokens();
                return instance;
            }
        }

        private void OnTokenAdded(string key, IToken token)
        {
            if (TokenAdded != null)
            {
                TokenAdded(key, token);
            }
        }

        private void OnTokenRemoved(string key, IToken token)
        {
            if (TokenRemoved != null)
            {
                TokenRemoved(key, token);
            }
        }

        public IEnumerator<KeyValuePair<string, IToken>> GetEnumerator()
        {
            return tokens.GetEnumerator();
        }

        public void Add(KeyValuePair<string, IToken> item)
        {
            if (tokens.ContainsKey(item.Key)) return;
            tokens.Add(item.Key, item.Value);
            OnTokenAdded(item.Key, item.Value);
        }

        public void Clear()
        {
            tokens.Clear();
        }

        public bool Contains(KeyValuePair<string, IToken> item)
        {
            return tokens.ContainsKey(item.Key);
        }

        public void CopyTo(KeyValuePair<string, IToken>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, IToken> item)
        {
            if (!tokens.ContainsKey(item.Key))
            {
                return false;
            }
            else
            {
                OnTokenRemoved(item.Key, item.Value);
                tokens.Remove(item.Key);
                return true;
            }
        }

        public int Count { get { return tokens.Count; } }

        public bool IsReadOnly { get { return false; } }

        public bool ContainsKey(string key)
        {
            return tokens.ContainsKey(key);
        }

        public void Add(string key, IToken value)
        {
            if (tokens.ContainsKey(key)) return;

            tokens.Add(key, value);
            OnTokenAdded(key, value);
        }

        public bool Remove(string key)
        {
            if (!tokens.ContainsKey(key)) return false;
            tokens.Remove(key);
            return true;
        }

        public bool TryGetValue(string key, out IToken value)
        {
            value = null;
            if (!tokens.ContainsKey(key))
            {
                return false;
            }
            value = tokens[key];
            return true;
        }

        public IToken this[string key]
        {
            get { return tokens[key]; }
            set { tokens[key] = value; }
        }

        public ICollection<string> Keys { get { return tokens.Keys; } }

        public ICollection<IToken> Values { get { return tokens.Values; } }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
