﻿using NBitcoin.Secp256k1;
using RUDP.Enums;
using RUDP.Extensions;

namespace RUDP.Keys
{
    internal class NPub : IEquatable<NPub>
    {
        internal string Hex { get; }
        internal string Bech32 { get; }
        internal ECXOnlyPubKey Ec { get; }


        internal NPub(string hex, string bech32, ECXOnlyPubKey ec)
        {
            Hex = hex;
            Bech32 = bech32;
            Ec = ec;
        }


        /// <summary>
        /// Validate the given signature against the given hex
        /// </summary>
        /// <param name="signatureHex"></param>
        /// <param name="hex"></param>
        /// <returns></returns>
        internal bool IsHexSignatureValid(string? signatureHex, string? hex)
        {
            if (string.IsNullOrEmpty(signatureHex))
                return false;
            if (!SecpSchnorrSignature.TryCreate(signatureHex.HexToByteArray(), out SecpSchnorrSignature? schnorr) || schnorr is null)
                return false;
            return Ec.SigVerifyBIP340(schnorr, (hex ?? "").HexToByteArray());
        }


        internal static NPub FromHex(string hex)
        {
            ECXOnlyPubKey ec = ECXOnlyPubKey.Create(hex.HexToByteArray());
            string bech32 = hex.HexToNpubBech32() ?? string.Empty;
            return new NPub(hex, bech32, ec);
        }
        internal static NPub FromBech32(string bech32)
        {
            string? hex = bech32.Bech32ToHexKey(out string? hrp);
            if (!Bech32Identifiers.NPub.Equals(hrp, StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(hex))
                throw new ArgumentException("Provided bech32 key is not 'npub'", nameof(bech32));
            return FromHex(hex);
        }
        internal static NPub FromEc(ECXOnlyPubKey ec)
        {
            string hex = ec.ToBytes().ToHexString();
            if (string.IsNullOrWhiteSpace(hex))
                throw new ArgumentException("Provided ec key is not correct", nameof(ec));
            return FromHex(hex);
        }
        internal static NPub FromPrivateEc(ECPrivKey ec)
        {
            ECXOnlyPubKey publicEc = ec.CreateXOnlyPubKey();
            return FromEc(publicEc);
        }


        public bool Equals(NPub? other)
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
            return Equals((NPub)obj);
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
