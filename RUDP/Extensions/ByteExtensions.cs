namespace RUDP.Extensions
{
    internal static class ByteExtensions
    {
        /// <summary>
        /// Convert a single byte into it's Hex string representation
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        internal static string ToHexString(this byte b)
        {
            return b.ToString("x2");
        }
    }
}
