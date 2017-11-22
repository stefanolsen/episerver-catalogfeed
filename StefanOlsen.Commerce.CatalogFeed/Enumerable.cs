using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace StefanOlsen.Commerce.CatalogFeed
{
    public class Enumerable<T> : IEnumerable<T>
    {
        private readonly IEnumerable<T> _baseEnumerable;

        public Enumerable(IEnumerable<T> baseEnumerable)
        {
            _baseEnumerable = baseEnumerable;
        }

        public void Add(T obj)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _baseEnumerable?.GetEnumerator() ?? Enumerable.Empty<T>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
