using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace SmallHax.Utils
{
    public abstract class AManager<T> : IManager<T>
    {
        protected Dictionary<string, T> Cache = new Dictionary<string, T> ();

        public void Clear()
        {
            Cache.Clear();
        }

        public T Get(string key)
        {
            if (!Cache.TryGetValue(key, out var item))
            {
                item = Initialize(key);
                Cache[key] = item;
            }
            return item;
        }

        protected abstract T Initialize(string key);
    }
}
