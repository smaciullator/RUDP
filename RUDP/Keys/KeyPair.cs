namespace RUDP.Keys
{
    internal class KeyPair : IEquatable<KeyPair>
    {
        internal NSec NSec { get; }
        internal NPub NPub { get; }


        internal KeyPair(NSec privateKey, NPub publicKey)
        {
            NSec = privateKey;
            NPub = publicKey;
        }
        internal KeyPair(NSec privateKey)
        {
            NSec = privateKey;
            NPub = NPub.FromPrivateEc(NSec.Ec);
        }


        /// <summary>
        /// Generate a new random key pair
        /// </summary>
        /// <returns></returns>
        internal static KeyPair GenerateNew()
        {
            NSec privateKey = NSec.New();
            return new KeyPair(privateKey);
        }
        /// <summary>
        /// Create a valid NSKeyPair instance from a valid NSec instance by deriving the corresponding NPub
        /// </summary>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        internal static KeyPair From(NSec privateKey)
        {
            return new KeyPair(privateKey);
        }


        internal byte[] Encrypt(byte[] data, NPub recipientPubKey, out byte[] ivBytes)
        {
            return NSec.Encrypt(data, recipientPubKey, out ivBytes);
        }
        internal byte[] Decrypt(byte[] encryptedData, byte[] ivBytes, NPub senderPubKey)
        {
            return NSec.Decrypt(encryptedData, ivBytes, senderPubKey);
        }


        public bool Equals(KeyPair? other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return NSec.Equals(other.NSec) && NPub.Equals(other.NPub);
        }
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != GetType())
                return false;
            return Equals((KeyPair)obj);
        }


        public override int GetHashCode()
        {
            return HashCode.Combine(NSec, NPub);
        }
    }
}
