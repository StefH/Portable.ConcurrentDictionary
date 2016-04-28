#region Assembly System.Runtime, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETPortable\v4.5\Profile\Profile259\System.Runtime.dll
#endregion

using System.Reflection;

#if SILVERLIGHT
namespace System.Collections.Generic
{
    //
    // Summary:
    //     Represents a generic read-only collection of key/value pairs.
    //
    // Type parameters:
    //   TKey:
    //     The type of keys in the read-only dictionary.
    //
    //   TValue:
    //     The type of values in the read-only dictionary.
    // [DefaultMember("Item")] STEF
    public interface IReadOnlyDictionary<TKey, TValue> : IReadOnlyCollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
    {
        //
        // Summary:
        //     Gets the element that has the specified key in the read-only dictionary.
        //
        // Parameters:
        //   key:
        //     The key to locate.
        //
        // Returns:
        //     The element that has the specified key in the read-only dictionary.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     key is null.
        //
        //   T:System.Collections.Generic.KeyNotFoundException:
        //     The property is retrieved and key is not found.
        TValue this[TKey key] { get; }

        //
        // Summary:
        //     Gets an enumerable collection that contains the keys in the read-only dictionary.
        //
        // Returns:
        //     An enumerable collection that contains the keys in the read-only dictionary.
        IEnumerable<TKey> Keys { get; }
        //
        // Summary:
        //     Gets an enumerable collection that contains the values in the read-only dictionary.
        //
        // Returns:
        //     An enumerable collection that contains the values in the read-only dictionary.
        IEnumerable<TValue> Values { get; }

        //
        // Summary:
        //     Determines whether the read-only dictionary contains an element that has the
        //     specified key.
        //
        // Parameters:
        //   key:
        //     The key to locate.
        //
        // Returns:
        //     true if the read-only dictionary contains an element that has the specified key;
        //     otherwise, false.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     key is null.
        bool ContainsKey(TKey key);
        //
        // Summary:
        //     Gets the value that is associated with the specified key.
        //
        // Parameters:
        //   key:
        //     The key to locate.
        //
        //   value:
        //     When this method returns, the value associated with the specified key, if the
        //     key is found; otherwise, the default value for the type of the value parameter.
        //     This parameter is passed uninitialized.
        //
        // Returns:
        //     true if the object that implements the System.Collections.Generic.IReadOnlyDictionary`2
        //     interface contains an element that has the specified key; otherwise, false.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     key is null.
        bool TryGetValue(TKey key, out TValue value);
    }
}
#endif