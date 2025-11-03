using RUDP.Keys;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;

namespace RUDP.Utilities
{
    internal static class PacketUtilities
    {
        private static KeyPair GetEphemeralKeys(string recipientNpubHex, out NPub ephemeralSharedNPub)
        {
            KeyPair eph_keys = KeyPair.GenerateNew();
            ephemeralSharedNPub = eph_keys.NSec.DeriveSharedKey(NPub.FromHex(recipientNpubHex));
            return eph_keys;
        }


        internal static bool IsSignatureValid(NPub npub, Header receivedHeader, byte[]? receivedBody = null)
        {
            string? signature = receivedHeader.Signature;
            if (string.IsNullOrEmpty(signature))
                return false;

            receivedHeader.Signature = null;
            byte[] header = receivedHeader.Serialize();
            byte[] body = new byte[0];
            if (receivedBody is not null)
            {
                body = new byte[receivedBody.Length];
                Array.Copy(receivedBody, body, receivedBody.Length);
            }

            byte[] packet = new byte[header.Length + body.Length];
            Array.Copy(header, 0, packet, 0, header.Length);
            Array.Copy(header, 0, body, header.Length, body.Length);

            return npub.IsHexSignatureValid(signature, packet);
        }


        private static byte[] DeriveKeyAndIV(NSec nsec, NPub npub, out byte[] iv)
        {
            // Compute the shared secret (ECDH)
            NPub shared = nsec.DeriveSharedKey(npub);
            byte[] sharedBytes = shared.Ec.ToBytes();

            // Use HKDF/SHA256 to derive both AES key and IV deterministically
            using SHA256 sha256 = SHA256.Create();
            byte[] hash = sha256.ComputeHash(sharedBytes);

            // Use first 32 bytes as AES key, next 16 bytes as IV (deterministic)
            byte[] key = new byte[32];
            iv = new byte[16];
            Array.Copy(hash, 0, key, 0, 32);
            Array.Copy(hash, 0, iv, 0, 16); // IV derived from part of the hash

            return key;
        }
        private static byte[] EncryptMessage(byte[] plainBytes, NSec nsec, NPub npub)
        {
            byte[] key = DeriveKeyAndIV(nsec, npub, out byte[] iv);

            using Aes aes = GetAesInstance(key, iv);
            using ICryptoTransform encryptor = aes.CreateEncryptor();
            byte[] encrypted = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            return encrypted;
        }
        internal static byte[] DecryptMessage(byte[] cipheredBytes, NSec nsec, NPub npub)
        {
            byte[] key = DeriveKeyAndIV(nsec, npub, out byte[] iv);

            using Aes aes = GetAesInstance(key, iv);
            using ICryptoTransform decryptor = aes.CreateDecryptor();
            byte[] decrypted = decryptor.TransformFinalBlock(cipheredBytes, 0, cipheredBytes.Length);

            return decrypted;
        }


        private static Aes GetAesInstance(byte[] key, byte[] iv)
        {
            Aes aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            return aes;
        }


        internal static byte[] CreatePacket(Header header, byte[]? data = null)
        {
            byte[] head = header.Serialize();
            if (data is null)
                return head;
            byte[] pkt = new byte[head.Length + data.Length];
            Array.Copy(head, 0, pkt, 0, head.Length);
            Array.Copy(data, 0, pkt, head.Length, data.Length);
            head = new byte[0];
            return pkt;
        }


        internal static (Header header, byte[]? data) MTU_DISCOVERY(
            string hex,
            int dataSize,
            byte relativeIndex = 0)
        {
            Header header = Header.MTU_DISCOVERY(hex);
            dataSize = dataSize - header.Length;
            byte[] data = Body.MTU_DISCOVERY(dataSize, relativeIndex);

            return (header, data);
        }
        internal static (Header header, byte[]? data) MTU_FOUND(
            NPub senderNPub,
            uint uniqueIdentifier,
            ushort dataLenght)
        {
            Header header = Header.MTU_FOUND(senderNPub.Hex, uniqueIdentifier);
            byte[] data = Body.MTU_FOUND(dataLenght, senderNPub);
            return (header, data);
        }
        internal static (Header header, byte[]? data) HANDSHAKE(
            KeyPair senderKeys,
            uint uniqueIdentifier)
        {
            Header header = Header.HANDSHAKE(null, uniqueIdentifier);
            byte[] plainBytes = header.Serialize();

            string? signature = senderKeys.NSec.SignHex(plainBytes);
            header = Header.HANDSHAKE(signature, uniqueIdentifier);
            return (header, null);
        }
        internal static (Header header, byte[]? data) CONNECTION_CONFIRM(
            KeyPair senderKeys,
            uint uniqueIdentifier,
            bool isSigServer)
        {
            Header header = Header.CONNECTION_CONFIRM(null, uniqueIdentifier);
            byte[] plainBytes = header.Serialize();

            string? signature = senderKeys.NSec.SignHex(plainBytes);
            header = Header.CONNECTION_CONFIRM(signature, uniqueIdentifier);
            return (header, Body.CONNECTION_CONFIRM(isSigServer));
        }
    }
}
