using System;

namespace SmallHax.Utils
{
    public interface IManager<T>
    {
        T Get(string key);
        void Clear();
    }
}
