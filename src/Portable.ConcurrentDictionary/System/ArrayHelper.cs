using System.Collections.Generic;

namespace System
{
    internal static class ArrayHelper
    {
        public static KeyValuePair<TKey, TValue>[] Empty<TKey, TValue>()
        {
            return new KeyValuePair<TKey, TValue>[0];
        }
    }
}