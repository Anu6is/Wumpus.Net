﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace Wumpus.Requests
{
    public abstract class QueryMap : IDictionary<string, string>
    {
        public abstract IDictionary<string, object> GetQueryMap();

        // IDictionary
        string IDictionary<string, string>.this[string key] { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        ICollection<string> IDictionary<string, string>.Keys => throw new NotSupportedException();
        ICollection<string> IDictionary<string, string>.Values => throw new NotSupportedException();

        void IDictionary<string, string>.Add(string key, string value) => throw new NotSupportedException();
        bool IDictionary<string, string>.ContainsKey(string key) => throw new NotSupportedException();
        bool IDictionary<string, string>.Remove(string key) => throw new NotSupportedException();
        bool IDictionary<string, string>.TryGetValue(string key, out string value) => throw new NotSupportedException();

        // ICollection
        int ICollection<KeyValuePair<string, string>>.Count => throw new NotSupportedException();
        bool ICollection<KeyValuePair<string, string>>.IsReadOnly => true;
        void ICollection<KeyValuePair<string, string>>.Add(KeyValuePair<string, string> item) => throw new NotSupportedException();
        bool ICollection<KeyValuePair<string, string>>.Remove(KeyValuePair<string, string> item) => throw new NotSupportedException();
        void ICollection<KeyValuePair<string, string>>.Clear() => throw new NotSupportedException();
        bool ICollection<KeyValuePair<string, string>>.Contains(KeyValuePair<string, string> item) => throw new NotSupportedException();
        void ICollection<KeyValuePair<string, string>>.CopyTo(KeyValuePair<string, string>[] array, int arrayIndex) => throw new NotSupportedException();

        // IEnumerable
        IEnumerator<KeyValuePair<string, string>> IEnumerable<KeyValuePair<string, string>>.GetEnumerator() => throw new NotSupportedException();
        IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException();
    }
}
