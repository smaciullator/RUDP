﻿using System.Text;

namespace RUDP.Extensions
{
    internal static class SpanByteExtensions
    {
        /// <summary>
        /// Return an Hex string from the given Span<byte>
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        internal static string ToHexString(this Span<byte> bytes)
        {
            StringBuilder builder = new StringBuilder();
            foreach (byte b in bytes)
                builder.Append(b.ToHexString());

            return builder.ToString();
        }
    }
}
