using NBitcoin.Secp256k1;
using RUDP.Enums;
using RUDP.Extensions;
using System.Security.Cryptography;

namespace RUDP.Keys
{
    internal class NSec : IEquatable<NSec>
    {
        internal string Hex { get; }
        internal string Bech32 { get; }
        internal ECPrivKey Ec { get; }


        private NSec(string hex, string bech32, ECPrivKey ec)
        {
            Hex = hex;
            Bech32 = bech32;
            Ec = ec;
        }


        /// <summary>
        /// Generate a new private key Hex and return a valid NSec instance
        /// </summary>
        /// <returns></returns>
        internal static NSec New()
        {
            using RandomNumberGenerator randomGen = RandomNumberGenerator.Create();
            byte[] randomBytes = new byte[32];
            randomGen.GetBytes(randomBytes);
            string randomHex = randomBytes.ToHexString();
            return FromHex(randomHex);
        }


        /// <summary>
        /// Derive an NPub instance from this NSec
        /// </summary>
        /// <returns></returns>
        internal NPub DerivePublicKey()
        {
            return NPub.FromPrivateEc(Ec);
        }
        /// <summary>
        /// Derive a shared key between this NSec instance and a specifiec NPub instance
        /// </summary>
        /// <param name="publicKey"></param>
        /// <returns>Return the resulting shared key as an NPub instance</returns>
        /// <exception cref="InvalidOperationException"></exception>
        internal NPub DeriveSharedKey(NPub publicKey)
        {
            Span<byte> input = stackalloc byte[33];
            input[0] = 0x02;
            publicKey.Ec.WriteToSpan(input[1..]);

            bool success = Context.Instance.TryCreatePubKey(input, out ECPubKey? prefixedPublicKey);
            if (!success || prefixedPublicKey == null)
                throw new InvalidOperationException("Can't create prefixed public key");

            ECPubKey sharedKey = prefixedPublicKey.GetSharedPubkey(Ec);
            if (sharedKey == null)
                throw new InvalidOperationException("Can't create shared public key");

            return NPub.FromEc(sharedKey.ToXOnlyPubKey());
        }


        /// <summary>
        /// Accept aa byte array and sign it using this NSec instance
        /// </summary>
        /// <param name="data"></param>
        /// <returns>Return a valid signature if success, or null in case of error</returns>
        internal byte[]? SignHex(byte[] data)
        {
            if (data is null || data.Length == 0)
                return null;
            return !Ec.TrySignBIP340(data, null, out SecpSchnorrSignature? signature) || signature is null ? null : signature.ToBytes();
        }


        internal static NSec FromHex(string hex)
        {
            ECPrivKey ec = ECPrivKey.Create(hex.HexToByteArray());
            string bech32 = hex.HexToNsecBech32() ?? string.Empty;
            return new NSec(hex, bech32, ec);
        }
        internal static NSec FromBech32(string bech32)
        {
            string? hex = bech32.Bech32ToHexKey(out string? hrp);
            if (!Bech32Identifiers.NSec.Equals(hrp, StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(hex))
                throw new ArgumentException("Provided bech32 key is not 'nsec'", nameof(bech32));
            return FromHex(hex);
        }
        internal static NSec FromEc(ECPrivKey ec)
        {
            string hex = ToHex(ec);
            if (string.IsNullOrWhiteSpace(hex))
                throw new ArgumentException("Provided ec key is not correct", nameof(ec));
            return FromHex(hex);
        }


        internal static string ToHex(ECPrivKey key)
        {
            Span<byte> keySpan = stackalloc byte[65];
            key.WriteToSpan(keySpan);
            string keyHex = keySpan.ToHexString();
            return keyHex[..64];
        }


        internal byte[] Encrypt(byte[] data, NPub recipientPubKey, out byte[] ivBytes)
        {
            using Aes aes = Aes.Create();
            aes.Key = DeriveSharedKey(recipientPubKey).Ec.ToBytes();
            ivBytes = aes.IV;
            return aes.EncryptCbc(data, aes.IV);
        }
        internal byte[] Decrypt(byte[] encryptedData, byte[] ivBytes, NPub senderPubKey)
        {
            using Aes aes = Aes.Create();
            aes.Key = DeriveSharedKey(senderPubKey).Ec.ToBytes();
            return aes.DecryptCbc(encryptedData, ivBytes);
        }


        public bool Equals(NSec? other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return string.Equals(Hex, other.Hex, StringComparison.OrdinalIgnoreCase) && string.Equals(Bech32, other.Bech32, StringComparison.OrdinalIgnoreCase);
        }
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != GetType())
                return false;
            return Equals((NSec)obj);
        }


        public override int GetHashCode()
        {
            HashCode hashCode = new HashCode();
            hashCode.Add(Hex, StringComparer.OrdinalIgnoreCase);
            hashCode.Add(Bech32, StringComparer.OrdinalIgnoreCase);
            return hashCode.ToHashCode();
        }
    }
}
