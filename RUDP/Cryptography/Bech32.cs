﻿using System.Diagnostics;

namespace RUDP.Cryptography
{
    /// <summary>
    /// Taken from https://github.com/guillaumebonnot/bech32
    /// </summary>
    internal static class Bech32
    {
        /// <summary>
        /// Charset is the sequence of ascii characters that make up the bech32 alphabet.
        /// Each character represents a 5-bit squashed byte.
        /// q = 0b00000, p = 0b00001, z = 0b00010, and so on.
        /// </summary>
        private const string Charset = "qpzry9x8gf2tvdw0s3jn54khce6mua7l";

        /// <summary>
        /// used for polymod
        /// </summary>
        private static readonly uint[] Generator = { 0x3b6a57b2, 0x26508e6d, 0x1ea119fa, 0x3d4233dd, 0x2a1462b3 };
        /// <summary>
        /// Icharset is a mapping of 8-bit ascii characters to the charset positions.
        /// Both uppercase and lowercase ascii are mapped to the 5-bit position values.
        /// </summary>
        private static readonly short[] Icharset =
        {
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            15, -1, 10, 17, 21, 20, 26, 30, 7, 5, -1, -1, -1, -1, -1, -1,
            -1, 29, -1, 24, 13, 25, 9, 8, 23, -1, 18, 22, 31, 27, 19, -1,
            1, 0, 3, 16, 11, 28, 12, 14, 6, 4, 2, -1, -1, -1, -1, -1,
            -1, 29, -1, 24, 13, 25, 9, 8, 23, -1, 18, 22, 31, 27, 19, -1,
            1, 0, 3, 16, 11, 28, 12, 14, 6, 4, 2, -1, -1, -1, -1, -1
        };


        /// <summary>
        /// We encode the data and the human readable prefix
        /// </summary>
        /// <param name="hrp"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        internal static string? Encode(string hrp, byte[] data)
        {
            byte[]? base5 = Bytes8To5(data);
            if (base5 == null)
                return string.Empty;
            return EncodeSquashed(hrp, base5);
        }
        /// <summary>
        /// on error, data == null
        /// </summary>
        /// <param name="encoded"></param>
        /// <param name="hrp"></param>
        /// <param name="data"></param>
        internal static void Decode(string encoded, out string? hrp, out byte[]? data)
        {
            DecodeSquashed(encoded, out hrp, out byte[]? squashed);
            if (squashed == null)
            {
                data = null;
                return;
            }
            data = Bytes5To8(squashed);
        }



        /// <summary>
        /// on error, data == null
        /// </summary>
        /// <param name="adr"></param>
        /// <param name="hrp"></param>
        /// <param name="data"></param>
        private static void DecodeSquashed(string? adr, out string? hrp, out byte[]? data)
        {
            adr = CheckAndFormat(adr);
            if (adr == null)
            {
                data = null; hrp = null; return;
            }

            // find the last "1" and split there
            int splitLoc = adr.LastIndexOf("1", StringComparison.Ordinal);
            if (splitLoc == -1)
            {
                data = null; hrp = null; return;
            }

            // hrp comes before the split
            hrp = adr.Substring(0, splitLoc);

            // get squashed data
            byte[]? squashed = StringToSquashedBytes(adr.Substring(splitLoc + 1));
            if (squashed == null)
            {
                data = null; return;
            }

            // make sure checksum works
            if (!VerifyChecksum(hrp, squashed))
            {
                data = null; return;
            }

            // chop off checksum to return only payload
            int length = squashed.Length - 6;
            data = new byte[length];
            Array.Copy(squashed, 0, data, 0, length);
        }
        /// <summary>
        /// on error, return null
        /// </summary>
        /// <param name="hrp"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private static string? EncodeSquashed(string hrp, byte[] data)
        {
            byte[] checksum = CreateChecksum(hrp, data);
            byte[] combined = data.Concat(checksum).ToArray();

            // Should be squashed, return empty string if it's not.
            string? encoded = SquashedBytesToString(combined);
            if (encoded == null)
                return null;
            return hrp + "1" + encoded;
        }


        /// <summary>
        /// PolyMod takes a byte slice and returns the 32-bit BCH checksum.
        /// Note that the input bytes to PolyMod need to be squashed to 5-bits tall before being used in this function.
        /// And this function will not error, but instead return an unusable checksum, if you give it full-height bytes.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        private static uint PolyMod(byte[] values)
        {
            uint chk = 1;
            foreach (byte value in values)
            {
                uint top = chk >> 25;
                chk = (chk & 0x1ffffff) << 5 ^ value;
                for (int i = 0; i < 5; ++i)
                {
                    if ((top >> i & 1) == 1)
                    {
                        chk ^= Generator[i];
                    }
                }
            }
            return chk;
        }


        /// <summary>
        /// on error, return null
        /// </summary>
        /// <param name="adr"></param>
        /// <returns></returns>
        private static string? CheckAndFormat(string? adr)
        {
            if (adr == null)
                return adr;

            // make an all lowercase and all uppercase version of the input string
            string lowAdr = adr.ToLower();
            string highAdr = adr.ToUpper();

            // if there's mixed case, that's not OK
            if (adr != lowAdr && adr != highAdr)
            {
                return null;
            }

            // default to lowercase
            return lowAdr;
        }


        /// <summary>
        /// HRPExpand turns the human readable part into 5bit-bytes for later processing
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static byte[] HrpExpand(string input)
        {
            byte[] output = new byte[input.Length * 2 + 1];

            // first half is the input string shifted down 5 bits.
            // not much is going on there in terms of data / entropy
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                output[i] = (byte)(c >> 5);
            }

            // then there's a 0 byte separator
            // don't need to set 0 byte in the middle, as it starts out that way

            // second half is the input string, with the top 3 bits zeroed.
            // most of the data / entropy will live here.
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                output[i + input.Length + 1] = (byte)(c & 0x1f);
            }
            return output;
        }


        private static bool VerifyChecksum(string hrp, byte[] data)
        {
            byte[] values = HrpExpand(hrp).Concat(data).ToArray();
            uint checksum = PolyMod(values);
            // make sure it's 1 (from the LSB flip in CreateChecksum
            return checksum == 1;
        }
        private static byte[] CreateChecksum(string hrp, byte[] data)
        {
            byte[] values = HrpExpand(hrp).Concat(data).ToArray();
            // put 6 zero bytes on at the end
            values = values.Concat(new byte[6]).ToArray();
            //get checksum for whole slice

            // flip the LSB of the checksum data after creating it
            uint checksum = PolyMod(values) ^ 1;

            byte[] ret = new byte[6];
            for (int i = 0; i < 6; i++)
            {
                // note that this is NOT the same as converting 8 to 5
                // this is it's own expansion to 6 bytes from 4, chopping
                // off the MSBs.
                ret[i] = (byte)(checksum >> 5 * (5 - i) & 0x1f);
            }

            return ret;
        }


        /// <summary>
        /// on error, return null
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// 
        private static byte[]? StringToSquashedBytes(string input)
        {
            byte[] squashed = new byte[input.Length];

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                short buffer = Icharset[c];
                if (buffer == -1)
                {
                    return null;
                }
                squashed[i] = (byte)buffer;
            }

            return squashed;
        }
        private static string? SquashedBytesToString(byte[] input)
        {
            string s = string.Empty;
            for (int i = 0; i < input.Length; i++)
            {
                byte c = input[i];
                if ((c & 0xe0) != 0)
                {
                    Debug.WriteLine("high bits set at position {0}: {1}", i, c);
                    return null;
                }
                s += Charset[c];
            }

            return s;
        }


        private static byte[]? Bytes8To5(byte[] data)
        {
            return ByteSquasher(data, 8, 5);
        }
        private static byte[]? Bytes5To8(byte[] data)
        {
            return ByteSquasher(data, 5, 8);
        }
        /// <summary>
        /// ByteSquasher squashes full-width (8-bit) bytes into "squashed" 5-bit bytes, and vice versa.
        /// It can operate on other widths but in this package only goes 5 to 8 and back again.
        /// It can return null if the squashed input you give it isn't actually squashed, or if there is padding (trailing q characters) when going from 5 to 8
        /// </summary>
        /// <param name="input"></param>
        /// <param name="inputWidth"></param>
        /// <param name="outputWidth"></param>
        /// <returns></returns>
        private static byte[]? ByteSquasher(byte[] input, int inputWidth, int outputWidth)
        {
            int bitstash = 0;
            int accumulator = 0;
            List<byte> output = new List<byte>();
            int maxOutputValue = (1 << outputWidth) - 1;

            foreach (byte c in input)
            {
                if (c >> inputWidth != 0)
                {
                    return null;
                }
                accumulator = accumulator << inputWidth | c;
                bitstash += inputWidth;
                while (bitstash >= outputWidth)
                {
                    bitstash -= outputWidth;
                    output.Add((byte)(accumulator >> bitstash & maxOutputValue));
                }
            }

            // pad if going from 8 to 5
            if (inputWidth == 8 && outputWidth == 5)
            {
                if (bitstash != 0)
                {
                    output.Add((byte)(accumulator << outputWidth - bitstash & maxOutputValue));
                }
            }
            else if (bitstash >= inputWidth || (accumulator << outputWidth - bitstash & maxOutputValue) != 0)
            {
                // no pad from 5 to 8 allowed
                return null;
            }
            return output.ToArray();
        }
    }
}
