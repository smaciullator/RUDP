using RUDP.Enums;
using RUDP.Extensions;
using RUDP.Keys;
using System.Net;

namespace RUDP.Utilities
{
    internal class Body
    {
        internal static byte[] MTU_DISCOVERY(int dataSize, NPub npub, byte relativeIndex = 0)
        {
            byte[] pubKey = npub.Bech32.UTF8AsByteArray();
            byte[] body = new byte[dataSize - (pubKey.Length + 1)];
            Array.Copy(pubKey, 0, body, 0, pubKey.Length);
            body[pubKey.Length] = relativeIndex;
            pubKey = new byte[0];
            return body;
        }
        internal static bool MTU_DISCOVERY(byte[] packet, out NPub? npub, out byte relativeIndex)
        {
            npub = null;
            relativeIndex = 0;
            PacketType type = (PacketType)packet[0];
            if (type != PacketType.MTU_DISCOVERY)
                return false;
            int headerLength = Header.Deserialize(packet).Length;
            string bech32 = new Span<byte>(packet).Slice(headerLength, 63).ToArray().ToUTF8String();
            try
            {
                npub = string.IsNullOrEmpty(bech32) ? null : NPub.FromBech32(bech32);
            }
            catch
            {
                npub = null;
            }
            relativeIndex = packet[headerLength + 63];
            return npub is not null;
        }


        internal static byte[] MTU_FOUND(ushort dataLenght, NPub npub)
        {
            byte[] dl = BitConverter.GetBytes(dataLenght);
            byte[] pubKey = npub.Bech32.UTF8AsByteArray();
            byte[] body = new byte[dl.Length + pubKey.Length];
            Array.Copy(dl, 0, body, 0, dl.Length);
            Array.Copy(pubKey, 0, body, dl.Length, pubKey.Length);
            dl = new byte[0];
            pubKey = new byte[0];
            return body;
        }
        internal static bool MTU_FOUND(byte[] packet, out ushort? dataLenght, out NPub? npub)
        {
            dataLenght = null;
            npub = null;
            PacketType type = (PacketType)packet[0];
            if (type != PacketType.MTU_FOUND)
                return false;
            Span<byte> body = new Span<byte>(packet).Slice(Header.Deserialize(packet).Length);
            dataLenght = BitConverter.ToUInt16(body.Slice(0, 2));
            string bech32 = body.Slice(2).ToArray().ToUTF8String();
            try
            {
                npub = string.IsNullOrEmpty(bech32) ? null : NPub.FromBech32(bech32);
            }
            catch
            {
                npub = null;
            }
            return dataLenght.HasValue && npub is not null;
        }


        internal static byte[] HANDSHAKE(int? sentSecret = null, int? receivedSecret = null)
        {
            sentSecret = !sentSecret.HasValue ? 0 : sentSecret.Value;
            receivedSecret = !receivedSecret.HasValue ? 0 : receivedSecret.Value;
            byte[] sent = BitConverter.GetBytes(sentSecret.Value);
            byte[] received = BitConverter.GetBytes(receivedSecret.Value);
            byte[] body = new byte[sent.Length + received.Length];
            Array.Copy(sent, 0, body, 0, sent.Length);
            Array.Copy(received, 0, body, sent.Length, received.Length);
            sent = new byte[0];
            received = new byte[0];
            return body;
        }
        internal static bool HANDSHAKE(byte[] packet, NSec nsec, NPub? npub, out int? sentSecret, out int? receivedSecret)
        {
            sentSecret = null;
            receivedSecret = null;
            if (npub is null)
                return false;

            PacketType type = (PacketType)packet[0];
            if (type != PacketType.HANDSHAKE)
                return false;

            Header header = Header.Deserialize(packet);
            if (header.IV is null)
                return false;

            byte[] encryptedBody = new Span<byte>(packet).Slice(header.Length).ToArray();
            Span<byte> body = new Span<byte>(nsec.Decrypt(encryptedBody, header.IV, npub));
            if (body.Length < 8)
                return false;

            sentSecret = BitConverter.ToInt32(body.Slice(0, 4));
            receivedSecret = BitConverter.ToInt32(body.Slice(4, 4));
            sentSecret = sentSecret.Value == 0 ? null : sentSecret.Value;
            receivedSecret = receivedSecret.Value == 0 ? null : receivedSecret.Value;
            return sentSecret.HasValue && sentSecret.Value > 0 || receivedSecret.HasValue && receivedSecret.Value > 0;
        }


        internal static byte[] CONNECTION_CONFIRM(bool isSigServer)
        {
            return BitConverter.GetBytes(isSigServer);
        }
        internal static bool CONNECTION_CONFIRM(byte[] packet, NSec nsec, NPub? npub, out bool? isSigServer)
        {
            isSigServer = null;
            PacketType type = (PacketType)packet[0];
            if (type != PacketType.CONNECTION_CONFIRM)
                return false;

            Header header = Header.Deserialize(packet);
            if (header.IV is null)
                return false;

            byte[] encryptedBody = new Span<byte>(packet).Slice(header.Length).ToArray();
            Span<byte> body = new Span<byte>(nsec.Decrypt(encryptedBody, header.IV, npub));
            isSigServer = BitConverter.ToBoolean(body);
            return true;
        }


        internal static byte[] P2P_COORDINATION_REQUEST(NPub npub)
        {
            return npub.Bech32.UTF8AsByteArray();
        }
        internal static bool P2P_COORDINATION_REQUEST(byte[] packet, NSec nsec, NPub? npub, out string? requestedNPub)
        {
            requestedNPub = null;
            PacketType type = (PacketType)packet[0];
            if (type != PacketType.P2P_COORDINATION_REQUEST)
                return false;

            Header header = Header.Deserialize(packet);
            if (header.IV is null)
                return false;

            byte[] encryptedBody = new Span<byte>(packet).Slice(header.Length).ToArray();
            Span<byte> body = new Span<byte>(nsec.Decrypt(encryptedBody, header.IV, npub));
            requestedNPub = body.ToArray().ToUTF8String();
            return requestedNPub is not null;
        }


        internal static byte[] UNKNOWN_IDENTITY(NPub npub)
        {
            return npub.Bech32.UTF8AsByteArray();
        }
        internal static bool UNKNOWN_IDENTITY(byte[] packet, NSec nsec, NPub? npub, out string? requestedBech32NPub)
        {
            requestedBech32NPub = null;
            PacketType type = (PacketType)packet[0];
            if (type != PacketType.UNKNOWN_IDENTITY)
                return false;

            Header header = Header.Deserialize(packet);
            if (header.IV is null)
                return false;

            byte[] encryptedBody = new Span<byte>(packet).Slice(header.Length).ToArray();
            Span<byte> body = new Span<byte>(nsec.Decrypt(encryptedBody, header.IV, npub));
            requestedBech32NPub = body.ToArray().ToUTF8String();
            return requestedBech32NPub is not null;
        }


        internal static byte[] P2P_CONNECTION_COORDINATION(NPub npub, EndPoint? peerEP1, EndPoint? peerEP2, EndPoint? peerEP3)
        {
            byte[] encryptedNPub = npub.Bech32.UTF8AsByteArray();

            // Endpoints are always padded to reach a fixed lenght
            byte[] encryptedEP1 = peerEP1.ToIPV4String().PadRight(21, '_').UTF8AsByteArray();
            byte[] encryptedEP2 = peerEP2 is null ? new string('_', 21).UTF8AsByteArray() : peerEP2.ToIPV4String().PadRight(21, '_').UTF8AsByteArray();
            byte[] encryptedEP3 = peerEP3 is null ? new string('_', 21).UTF8AsByteArray() : peerEP3.ToIPV4String().PadRight(21, '_').UTF8AsByteArray();

            byte[] body = new byte[encryptedNPub.Length + encryptedEP1.Length + encryptedEP2.Length + encryptedEP3.Length];
            Array.Copy(encryptedNPub, 0, body, 0, encryptedNPub.Length);
            Array.Copy(encryptedEP1, 0, body, encryptedNPub.Length, encryptedEP1.Length);
            Array.Copy(encryptedEP2, 0, body, encryptedNPub.Length + encryptedEP1.Length, encryptedEP2.Length);
            Array.Copy(encryptedEP3, 0, body, encryptedNPub.Length + encryptedEP1.Length + encryptedEP2.Length, encryptedEP3.Length);
            encryptedNPub = new byte[0];
            encryptedEP1 = new byte[0];
            encryptedEP2 = new byte[0];
            encryptedEP3 = new byte[0];
            return body;
        }
        internal static bool P2P_CONNECTION_COORDINATION(byte[] packet, out NPub? peerNPub, out EndPoint? peerEP1, out EndPoint? peerEP2, out EndPoint? peerEP3)
        {
            peerNPub = null;
            peerEP1 = null;
            peerEP2 = null;
            peerEP3 = null;
            PacketType type = (PacketType)packet[0];
            if (type != PacketType.P2P_CONNECTION_COORDINATION)
                return false;

            Span<byte> body = new Span<byte>(packet).Slice(Header.Deserialize(packet).Length);
            string bech32 = body.Slice(0, 63).ToArray().ToUTF8String();
            try
            {
                peerNPub = string.IsNullOrEmpty(bech32) ? null : NPub.FromBech32(bech32);
            }
            catch
            {
                peerNPub = null;
            }
            peerEP1 = body.Slice(63, 21).ToArray().ToUTF8String().Replace("_", "").ToEndPoint();
            peerEP2 = body.Slice(84, 21).ToArray().ToUTF8String().Replace("_", "").ToEndPoint();
            peerEP3 = body.Slice(105, 21).ToArray().ToUTF8String().Replace("_", "").ToEndPoint();
            return peerNPub is not null;
        }


        internal static byte[] CONNECTION_POSSIBLE(NPub npub)
        {
            return npub.Bech32.UTF8AsByteArray();
        }
        internal static bool CONNECTION_POSSIBLE(byte[] packet, out string? bech32NPub)
        {
            bech32NPub = null;
            PacketType type = (PacketType)packet[0];
            if (type != PacketType.CONNECTION_POSSIBLE)
                return false;
            bech32NPub = new Span<byte>(packet).Slice(Header.Deserialize(packet).Length, 63).ToArray().ToUTF8String();
            return bech32NPub is not null;
        }


        internal static byte[] DISCONNECTION(int? sentSecret = null, int? receivedSecret = null)
        {
            sentSecret = !sentSecret.HasValue ? 0 : sentSecret.Value;
            receivedSecret = !receivedSecret.HasValue ? 0 : receivedSecret.Value;
            byte[] sent = BitConverter.GetBytes(sentSecret.Value);
            byte[] received = BitConverter.GetBytes(receivedSecret.Value);
            byte[] body = new byte[sent.Length + received.Length];
            Array.Copy(sent, 0, body, 0, sent.Length);
            Array.Copy(received, 0, body, sent.Length, received.Length);
            sent = new byte[0];
            received = new byte[0];
            return body;
        }
        internal static bool DISCONNECTION(byte[] packet, NSec nsec, NPub? npub, out int? sentSecret, out int? receivedSecret)
        {
            sentSecret = null;
            receivedSecret = null;
            if (npub is null)
                return false;

            PacketType type = (PacketType)packet[0];
            if (type != PacketType.DISCONNECTION)
                return false;

            Header header = Header.Deserialize(packet);
            if (header.IV is null)
                return false;

            byte[] encryptedBody = new Span<byte>(packet).Slice(header.Length).ToArray();
            Span<byte> body = new Span<byte>(nsec.Decrypt(encryptedBody, header.IV, npub));
            if (body.Length < 8)
                return false;

            sentSecret = BitConverter.ToInt32(body.Slice(0, 4));
            receivedSecret = BitConverter.ToInt32(body.Slice(4, 4));
            sentSecret = sentSecret.Value == 0 ? null : sentSecret.Value;
            receivedSecret = receivedSecret.Value == 0 ? null : receivedSecret.Value;
            return sentSecret.HasValue && sentSecret.Value > 0 || receivedSecret.HasValue && receivedSecret.Value > 0;
        }


        internal static byte[] SIGNALING_PROPAGATION(NPub peerNPub, EndPoint peerEP, byte relativeIndex)
        {
            byte[] pubKey = peerNPub.Bech32.UTF8AsByteArray();
            byte[] encryptedEP = peerEP.ToIPV4String().PadRight(21, '_').UTF8AsByteArray();
            byte[] body = new byte[pubKey.Length + encryptedEP.Length + 1];
            Array.Copy(pubKey, 0, body, 0, pubKey.Length);
            Array.Copy(encryptedEP, 0, body, pubKey.Length, encryptedEP.Length);
            pubKey = new byte[0];
            encryptedEP = new byte[0];
            body[body.Length - 1] = relativeIndex;
            return body;
        }
        internal static bool SIGNALING_PROPAGATION(byte[] packet, NSec nsec, NPub? npub, out NPub? peerNPub, out EndPoint? peerEP, out byte relativeIndex)
        {
            peerNPub = null;
            peerEP = null;
            relativeIndex = 0;
            PacketType type = (PacketType)packet[0];
            if (type != PacketType.SIGNALING_PROPAGATION)
                return false;

            Header header = Header.Deserialize(packet);
            if (header.IV is null)
                return false;

            byte[] encryptedBody = new Span<byte>(packet).Slice(header.Length).ToArray();
            Span<byte> body = new Span<byte>(nsec.Decrypt(encryptedBody, header.IV, npub));

            string bech32 = body.Slice(0, 63).ToArray().ToUTF8String();
            try
            {
                peerNPub = string.IsNullOrEmpty(bech32) ? null : NPub.FromBech32(bech32);
            }
            catch
            {
                peerNPub = null;
            }
            peerEP = body.Slice(63, 21).ToArray().ToUTF8String().Replace("_", "").ToEndPoint();
            relativeIndex = body[^1];
            return peerNPub is not null && peerEP is not null && relativeIndex > 0;
        }


        internal static byte[]? TryEncryptData(KeyPair myIdentity, NPub? epNPub, byte[] clearData, out byte[] ivBytes)
        {
            ivBytes = new byte[0];
            if (epNPub is not null)
                return myIdentity.Encrypt(clearData, epNPub, out ivBytes);
            return null;
        }


        internal static byte[] ExtractFromPacket(byte[] packet)
        {
            byte[] header = Header.Deserialize(packet).Serialize();
            return new Span<byte>(packet).Slice(header.Length).ToArray();
        }
    }
}
