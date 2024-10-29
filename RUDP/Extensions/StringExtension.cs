using RUDP.Cryptography;
using RUDP.Enums;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace RUDP.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Converte una stringa IPV4 con indirizzo ip e opzionalmente la porta in un IPEndPoint
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public static EndPoint? ToEndPoint(this string endpoint)
        {
            if (string.IsNullOrEmpty(endpoint))
                return null;

            string[] ep = endpoint.Split(':');
            if (ep.Length != 2)
                throw new FormatException("Invalid endpoint format");

            if (!IPAddress.TryParse(ep[0], out IPAddress ip))
                throw new FormatException("Invalid ip-adress");

            if (!int.TryParse(ep[1], NumberStyles.None, NumberFormatInfo.CurrentInfo, out int port))
                throw new FormatException("Invalid port");

            return new IPEndPoint(ip, port);
        }

        /// <summary>
        /// Return the SHA256 hashed representation of the given string as a byte array
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        internal static byte[] GetSha256(this string data)
        {
            byte[] sha256 = SHA256.HashData(data.UTF8AsByteArray());
            return sha256;
        }
        /// <summary>
        /// Return a byte array from the given UTF-8 encoded string
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        internal static byte[] UTF8AsByteArray(this string data)
        {
            return Encoding.UTF8.GetBytes(data);
        }


        /// <summary>
        /// Return the byte array representation of the given Hex string
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        internal static byte[] HexToByteArray(this string hex)
        {
            if (hex.Length % 2 == 1)
                throw new Exception("The binary key cannot have an odd number of digits");

            byte[] arr = new byte[hex.Length >> 1];

            for (int i = 0; i < hex.Length >> 1; ++i)
            {
                char a = hex[i << 1];
                arr[i] = (byte)((hex[i << 1].GetHexValue() << 4) + hex[(i << 1) + 1].GetHexValue());
            }

            return arr;
        }


        /// <summary>
        /// Convert the given Hex string into the corresponding Bech32 string, in addition with the given HRP
        /// </summary>
        /// <param name="hexKey"></param>
        /// <param name="hrp"></param>
        /// <returns></returns>
        internal static string? HexKeyToBech32(this string? hexKey, string hrp)
        {
            if (string.IsNullOrWhiteSpace(hexKey))
                return hexKey;

            byte[] hexArray = hexKey.HexToByteArray();
            return hexArray.HexKeyToBech32String(hrp);
        }


        /// <summary>
        /// Convert the given Hex string into the corresponding Bech32 string with the right HRP for NPub ("npub")
        /// </summary>
        /// <param name="hexKey"></param>
        /// <returns></returns>
        internal static string? HexToNpubBech32(this string? hexKey)
        {
            return hexKey.HexKeyToBech32(Bech32Identifiers.NPub);
        }
        /// <summary>
        /// Convert the given Hex string into the corresponding Bech32 string with the right HRP for NSec ("nsec")
        /// </summary>
        /// <param name="hexKey"></param>
        /// <returns></returns>
        internal static string? HexToNsecBech32(this string? hexKey)
        {
            return hexKey.HexKeyToBech32(Bech32Identifiers.NSec);
        }


        /// <summary>
        /// Convert a Bech32 string into it's byte array representation, and also output the initial HRP
        /// </summary>
        /// <param name="bech32"></param>
        /// <param name="hrp"></param>
        /// <returns></returns>
        internal static byte[]? Bech32ToHexBytes(this string? bech32, out string? hrp)
        {
            hrp = null;
            if (string.IsNullOrWhiteSpace(bech32))
                return Array.Empty<byte>();

            Bech32.Decode(bech32, out hrp, out byte[]? decoded);
            return decoded;
        }
        /// <summary>
        /// Convert a Bech32 string into it's Hex string representation, and also output the initial HRP
        /// </summary>
        /// <param name="bech32"></param>
        /// <param name="hrp"></param>
        /// <returns></returns>
        internal static string? Bech32ToHexKey(this string? bech32, out string? hrp)
        {
            hrp = null;
            if (string.IsNullOrWhiteSpace(bech32))
                return bech32;

            Bech32.Decode(bech32, out hrp, out byte[]? decoded);
            string? hex = decoded?.ToHexString();
            return hex;
        }
    }
}
