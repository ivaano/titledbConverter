using System.Collections.Concurrent;

namespace titledbConverter.Extensions;

public static class ConcurrentDictionaryExtensions
{
    public static ConcurrentDictionary<TKey, TValue> ToConcurrentDictionary<TKey, TValue>(
        this IEnumerable<TValue> source,
        Func<TValue, TKey> keySelector) where TKey : notnull
    {
        var dictionary = new ConcurrentDictionary<TKey, TValue>();

        foreach (var element in source)
        {
            var key = keySelector(element);
            dictionary.TryAdd(key, element);
        }

        return dictionary;
    }

    public static ConcurrentDictionary<TKey, TResult> ToConcurrentDictionary<TKey, TValue, TResult>(
        this IEnumerable<TValue> source,
        Func<TValue, TKey> keySelector,
        Func<TValue, TResult> valueSelector) where TKey : notnull
    {
        var dictionary = new ConcurrentDictionary<TKey, TResult>();

        foreach (var element in source)
        {
            var key = keySelector(element);
            dictionary.TryAdd(key, valueSelector(element));
        }

        return dictionary;
    }
}