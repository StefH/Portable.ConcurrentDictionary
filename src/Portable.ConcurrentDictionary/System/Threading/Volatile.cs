#if SILVERLIGHT
/// <summary>
/// Copied from https://github.com/SaladLab/NetLegacySupport/blob/master/core/ConcurrentDictionary/System/Collections/Concurrent/FrameworkTraits.cs
/// </summary>
namespace System.Threading
{
    internal static class Volatile
    {
        public static T Read<T>(ref T location) where T : class
        {
            // 
            // The VM will replace this with a more efficient implementation.
            //
            var value = location;
            Thread.MemoryBarrier();
            return value;
        }

        public static void Write<T>(ref T location, T value) where T : class
        {
            // 
            // The VM will replace this with a more efficient implementation.
            //
            Thread.MemoryBarrier();
            location = value;
        }
    }
}
#endif