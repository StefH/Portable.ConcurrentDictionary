#region Assembly System.Runtime, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETPortable\v4.5\Profile\Profile259\System.Runtime.dll
#endregion

namespace System.Collections.Generic
{
    //
    // Summary:
    //     Represents a strongly-typed, read-only collection of elements.
    //
    // Type parameters:
    //   T:
    //     The type of the elements.This type parameter is covariant. That is, you can use
    //     either the type you specified or any type that is more derived. For more information
    //     about covariance and contravariance, see Covariance and Contravariance in Generics.
    public interface IReadOnlyCollection<out T> : IEnumerable<T>, IEnumerable
    {
        //
        // Summary:
        //     Gets the number of elements in the collection.
        //
        // Returns:
        //     The number of elements in the collection.
        int Count { get; }
    }
}