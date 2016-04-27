
namespace System
{
    internal static class PlatformHelper
    {
        public static int ProcessorCount
        {
            get { return Environment.ProcessorCount; }
        }
    }
}