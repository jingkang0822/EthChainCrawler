﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtherScanServiceNetFrameWork
{
    public static class DictionaryExtensions
    {
        public static ConcurrentDictionary<TKey, TValue> ToConcurrentDictionary<TKey, TValue>(
            this IEnumerable<KeyValuePair<TKey, TValue>> source)
        {
            return new ConcurrentDictionary<TKey, TValue>(source);
        }

        public static ConcurrentDictionary<TKey, TValue> ToConcurrentDictionary<TKey, TValue>(
            this IEnumerable<TValue> source, Func<TValue, TKey> keySelector)
        {
            return new ConcurrentDictionary<TKey, TValue>(
                from v in source
                select new KeyValuePair<TKey, TValue>(keySelector(v), v));
        }

        public static ConcurrentDictionary<TKey, TElement> ToConcurrentDictionary<TKey, TValue, TElement>(
            this IEnumerable<TValue> source, Func<TValue, TKey> keySelector, Func<TValue, TElement> elementSelector)
        {
            return new ConcurrentDictionary<TKey, TElement>(
                from v in source
                select new KeyValuePair<TKey, TElement>(keySelector(v), elementSelector(v)));
        }
    }
}
