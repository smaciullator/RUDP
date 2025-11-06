using RUDP.Keys;
using System.Net;
using System.Security.Cryptography;

namespace RUDP.Utilities
{
    internal static class PacketUtilities
    {
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


        private static string? SignMessage(Header header, byte[] body, NSec senderKey)
        {
            header.Signature = null;
            byte[] head = header.Serialize();
            byte[] packet = new byte[head.Length + body.Length];

            Array.Copy(head, 0, packet, 0, packet.Length);
            Array.Copy(body, 0, packet, packet.Length, body.Length);

            string? signature = senderKey.SignHex(packet);
            return signature;
        }
        internal static bool IsSignatureValid(NPub npub, Header receivedHeader, Span<byte> receivedBody = new Span<byte>())
        {
            string? signature = receivedHeader.Signature;
            if (string.IsNullOrEmpty(signature))
                return false;

            receivedHeader.Signature = null;
            byte[] head = receivedHeader.Serialize();
            byte[] body = receivedBody.ToArray();

            byte[] packet = new byte[head.Length + body.Length];
            Array.Copy(head, 0, packet, 0, head.Length);
            Array.Copy(head, 0, body, head.Length, body.Length);

            return npub.IsHexSignatureValid(signature, packet);
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
            KeyPair senderKeys,
            uint uniqueIdentifier,
            int dataSize,
            byte relativeIndex = 0)
        {
            Header header = Header.MTU_DISCOVERY(null, uniqueIdentifier);
            dataSize = dataSize - header.Length;
            byte[] body = Body.MTU_DISCOVERY(senderKeys.NPub.Bech32, dataSize, relativeIndex);

            string? signature = SignMessage(header, body, senderKeys.NSec);
            header = Header.MTU_DISCOVERY(signature, uniqueIdentifier);

            return (header, body);
        }
        internal static (Header header, byte[]? data) MTU_FOUND(
            KeyPair senderKeys,
            uint uniqueIdentifier,
            ushort dataLenght,
            bool isSigServer)
        {
            Header header = Header.MTU_FOUND(null, uniqueIdentifier);
            byte[] body = Body.MTU_FOUND(dataLenght, isSigServer);

            string? signature = SignMessage(header, body, senderKeys.NSec);
            header = Header.MTU_FOUND(signature, uniqueIdentifier);

            return (header, body);
        }
        internal static (Header header, byte[]? data) DISCONNECTION(
            KeyPair senderKeys,
            uint uniqueIdentifier)
        {
            Header header = Header.DISCONNECTION(null, uniqueIdentifier);

            string? signature = SignMessage(header, new byte[0], senderKeys.NSec);
            header = Header.DISCONNECTION(signature, uniqueIdentifier);

            return (header, null);
        }


        internal static (Header header, byte[]? data) P2P_COORDINATION_REQUEST(
            KeyPair senderKeys,
            uint uniqueIdentifier,
            NPub requestedNPub)
        {
            Header header = Header.P2P_COORDINATION_REQUEST(null, uniqueIdentifier);
            byte[] body = Body.P2P_COORDINATION_REQUEST(requestedNPub);

            string? signature = SignMessage(header, body, senderKeys.NSec);
            header = Header.P2P_COORDINATION_REQUEST(signature, uniqueIdentifier);

            return (header, body);
        }
        internal static (Header header, byte[]? data) UNKNOWN_IDENTITY(
            KeyPair senderKeys,
            uint uniqueIdentifier,
            NPub requestedNPub)
        {
            Header header = Header.UNKNOWN_IDENTITY(null, uniqueIdentifier);
            byte[] body = Body.UNKNOWN_IDENTITY(requestedNPub);

            string? signature = SignMessage(header, body, senderKeys.NSec);
            header = Header.UNKNOWN_IDENTITY(signature, uniqueIdentifier);

            return (header, body);
        }
        internal static (Header header, byte[]? data) P2P_CONNECTION_COORDINATION(
            KeyPair senderKeys,
            uint uniqueIdentifier,
            NPub targetNpub,
            EndPoint? peerEP1, EndPoint? peerEP2, EndPoint? peerEP3)
        {
            Header header = Header.P2P_CONNECTION_COORDINATION(null, uniqueIdentifier);
            byte[] body = Body.P2P_CONNECTION_COORDINATION(targetNpub, peerEP1, peerEP2, peerEP3);

            string? signature = SignMessage(header, body, senderKeys.NSec);
            header = Header.P2P_CONNECTION_COORDINATION(signature, uniqueIdentifier);

            return (header, body);
        }
        


    }
}
