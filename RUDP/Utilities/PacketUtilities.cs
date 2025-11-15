using RUDP.Extensions;
using RUDP.Keys;
using System.Net;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        internal static byte[] EncryptMessage(byte[] plainBytes, NSec nsec, NPub npub)
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


        private static byte[] GetSignablePacket(Header header, byte[]? body)
        {
            header.Signature = null;
            byte[] head = header.Serialize();
            byte[] packet = new byte[head.Length + (body is null ? 0 : body.Length)];

            Array.Copy(head, 0, packet, 0, head.Length);
            if (body is not null)
                Array.Copy(body, 0, packet, head.Length, body.Length);

            return packet;
        }
        private static string? SignMessage(Header header, byte[]? body, NSec senderKey)
        {
            byte[] packet = GetSignablePacket(header, body);
            string? signature = senderKey.SignHex(packet);
            return signature;
        }
        internal static bool IsSignatureValid(KeyPair identity, NPub npub, Header receivedHeader, Span<byte> receivedBody = new Span<byte>())
        {
            string? signature = receivedHeader.Signature;
            if (string.IsNullOrEmpty(signature))
                return false;

            byte[] packet = GetSignablePacket(receivedHeader, receivedBody.ToArray());
            byte[] hash = packet.GetSha256();

            return npub.IsHexSignatureValid(signature, hash);
        }


        internal static byte[] CreatePacket(Header header, byte[]? body = null)
        {
            byte[] head = header.Serialize();
            if (body is null)
                return head;
            byte[] pkt = new byte[head.Length + body.Length];
            Array.Copy(head, 0, pkt, 0, head.Length);
            Array.Copy(body, 0, pkt, head.Length, body.Length);
            head = new byte[0];
            body = new byte[0];
            return pkt;
        }


        internal static (Header header, byte[]? data) RTTA(
            KeyPair senderKeys,
            uint uniqueIdentifier
        )
        {
            Header header = Header.RTTA(null, uniqueIdentifier);

            string? signature = SignMessage(header, new byte[0], senderKeys.NSec);
            header = Header.RTTA(signature, uniqueIdentifier);

            return (header, null);
        }
        internal static (Header header, byte[]? data) RTTB(
            KeyPair senderKeys,
            uint uniqueIdentifier
        )
        {
            Header header = Header.RTTB(null, uniqueIdentifier);

            string? signature = SignMessage(header, new byte[0], senderKeys.NSec);
            header = Header.RTTB(signature, uniqueIdentifier);

            return (header, null);
        }
        internal static (Header header, byte[]? body) ACKNOWLEDGEMENT(
            KeyPair senderKeys,
            uint uniqueIdentifier,
            uint? chunkNumber
        )
        {
            Header header = Header.ACKNOWLEDGEMENT(null, uniqueIdentifier, chunkNumber);

            string? signature = SignMessage(header, new byte[0], senderKeys.NSec);
            header = Header.ACKNOWLEDGEMENT(signature, uniqueIdentifier, chunkNumber);

            return (header, null);
        }


        internal static (Header header, byte[]? data) MTU_DISCOVERY(
            KeyPair senderKeys,
            uint uniqueIdentifier,
            int dataSize,
            byte relativeIndex = 0
        )
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
            bool isSigServer
        )
        {
            Header header = Header.MTU_FOUND(null, uniqueIdentifier);
            byte[] body = Body.MTU_FOUND(senderKeys.NPub.Bech32, dataLenght, isSigServer);

            string? signature = SignMessage(header, body, senderKeys.NSec);
            header = Header.MTU_FOUND(signature, uniqueIdentifier);

            return (header, body);
        }
        internal static (Header header, byte[]? data) DISCONNECTION(
            KeyPair senderKeys,
            uint uniqueIdentifier
        )
        {
            Header header = Header.DISCONNECTION(null, uniqueIdentifier);

            string? signature = SignMessage(header, new byte[0], senderKeys.NSec);
            header = Header.DISCONNECTION(signature, uniqueIdentifier);

            return (header, null);
        }


        internal static (Header header, byte[]? data) P2P_COORDINATION_REQUEST(
            KeyPair senderKeys,
            uint uniqueIdentifier,
            NPub requestedNPub
        )
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
            NPub requestedNPub
        )
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
            EndPoint? peerEP1, EndPoint? peerEP2, EndPoint? peerEP3
        )
        {
            Header header = Header.P2P_CONNECTION_COORDINATION(null, uniqueIdentifier);
            byte[] body = Body.P2P_CONNECTION_COORDINATION(targetNpub, peerEP1, peerEP2, peerEP3);

            string? signature = SignMessage(header, body, senderKeys.NSec);
            header = Header.P2P_CONNECTION_COORDINATION(signature, uniqueIdentifier);

            return (header, body);
        }
        internal static (Header header, byte[]? data) CONNECTION_POSSIBLE(KeyPair senderKeys)
        {
            Header header = Header.CONNECTION_POSSIBLE(null);
            byte[] body = Body.CONNECTION_POSSIBLE(senderKeys.NPub.Bech32);

            string? signature = SignMessage(header, body, senderKeys.NSec);
            header = Header.CONNECTION_POSSIBLE(signature);

            return (header, body);
        }


        internal static (Header header, byte[]? data) SIGNALING_PROPAGATION(
            KeyPair senderKeys,
            uint uniqueIdentifier,
            NPub peerNPub,
            EndPoint peerEndPoint,
            byte relativeIndex,
            NPub targetNpub
        )
        {
            Header header = Header.SIGNALING_PROPAGATION(null, uniqueIdentifier);
            byte[] body = Body.SIGNALING_PROPAGATION(peerNPub, peerEndPoint, relativeIndex);
            byte[] encryptedBody = EncryptMessage(body, senderKeys.NSec, targetNpub);

            string? signature = SignMessage(header, encryptedBody, senderKeys.NSec);
            header = Header.SIGNALING_PROPAGATION(signature, uniqueIdentifier);

            return (header, encryptedBody);
        }


        internal static (Header header, byte[]? data) CHUNKS_PRESENTATION(
            KeyPair senderKeys,
            uint uniqueIdentifier,
            uint chunkNumber,
            NPub targetNpub
        )
        {
            Header header = Header.CHUNKS_PRESENTATION(null, uniqueIdentifier, chunkNumber);

            string? signature = SignMessage(header, null, senderKeys.NSec);
            header = Header.CHUNKS_PRESENTATION(signature, uniqueIdentifier, chunkNumber);

            return (header, null);
        }
        internal static (Header header, byte[]? data) DATA(
            KeyPair senderKeys,
            uint uniqueIdentifier,
            uint chunkNumber,
            byte[] rawData,
            NPub targetNpub
        )
        {
            Header header = Header.DATA(null, uniqueIdentifier, chunkNumber);
            byte[] body = EncryptMessage(rawData, senderKeys.NSec, targetNpub);

            string? signature = SignMessage(header, body, senderKeys.NSec);
            header = Header.DATA(signature, uniqueIdentifier, chunkNumber);

            return (header, body);
        }
        internal static (Header header, byte[]? data) STREAM(
            KeyPair senderKeys,
            byte[] rawData,
            NPub targetNpub
        )
        {
            Header header = Header.STREAM(null);
            byte[] body = EncryptMessage(rawData, senderKeys.NSec, targetNpub);

            string? signature = SignMessage(header, body, senderKeys.NSec);
            header = Header.STREAM(signature);

            return (header, body);
        }
        internal static (Header header, byte[]? data) FILE_PRESENTATION(
            KeyPair senderKeys,
            uint uniqueIdentifier,
            uint chunkNumber,
            byte[] rawData,
            NPub targetNpub
        )
        {
            Header header = Header.FILE_PRESENTATION(null, uniqueIdentifier, chunkNumber);
            byte[] body = EncryptMessage(rawData, senderKeys.NSec, targetNpub);

            string? signature = SignMessage(header, body, senderKeys.NSec);
            header = Header.FILE_PRESENTATION(signature, uniqueIdentifier, chunkNumber);

            return (header, body);
        }
        internal static (Header header, byte[]? data) FILE_CONTENT(
            KeyPair senderKeys,
            uint uniqueIdentifier,
            uint chunkNumber,
            byte[] rawData,
            NPub targetNpub
        )
        {
            Header header = Header.FILE_CONTENT(null, uniqueIdentifier, chunkNumber);
            byte[] body = EncryptMessage(rawData, senderKeys.NSec, targetNpub);

            string? signature = SignMessage(header, body, senderKeys.NSec);
            header = Header.FILE_CONTENT(signature, uniqueIdentifier, chunkNumber);

            return (header, body);
        }
    }
}
