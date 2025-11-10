using RUDP.Enums;
using RUDP.Extensions;
using RUDP.Keys;
using System.Net;
using System.Text;

namespace RUDP.Utilities
{
    internal class Body
    {
        internal static byte[] MTU_DISCOVERY(string nPubBech32, int dataSize, byte relativeIndex = 0)
        {
            byte[] body = new byte[dataSize];
            byte[] hex = Encoding.UTF8.GetBytes(nPubBech32.Replace("npub1", ""));
            Array.Copy(hex, 0, body, 0, hex.Length);
            body[hex.Length] = relativeIndex;
            hex = new byte[0];
            return body;
        }
        internal static bool MTU_DISCOVERY(Span<byte> body, out NPub? npub, out byte relativeIndex)
        {
            npub = null;
            relativeIndex = 0;
            string bech32 = body.Slice(0, 58).ToArray().ToUTF8String();
            try
            {
                npub = string.IsNullOrEmpty(bech32) ? null : NPub.FromBech32($"npub1{bech32}");
            }
            catch
            {
                npub = null;
            }
            relativeIndex = body.Slice(58, 1)[0];
            return npub is not null;
        }


        internal static byte[] MTU_FOUND(int dataLenght, bool isSigServer)
        {
            byte[] dl = BitConverter.GetBytes(dataLenght);

            byte[] body = new byte[dl.Length + 1];
            Array.Copy(dl, 0, body, 0, dl.Length);
            body[^1] = isSigServer ? (byte)1 : (byte)0;

            dl = new byte[0];
            return body;
        }
        internal static bool MTU_FOUND(Span<byte> body, out ushort? dataLenght, out bool? isSigServer)
        {
            dataLenght = BitConverter.ToUInt16(body.Slice(0, 2));
            isSigServer = body.Slice(2, 1)[0] == 1;

            return dataLenght.HasValue;
        }


        internal static byte[] P2P_COORDINATION_REQUEST(NPub npub)
        {
            return npub.Bech32.Replace("npub1", "").UTF8AsByteArray();
        }
        internal static bool P2P_COORDINATION_REQUEST(Span<byte> body, out NPub? requestedNPub)
        {
            requestedNPub = null;
            string bech32 = body.Slice(0, 58).ToArray().ToUTF8String();
            try
            {
                requestedNPub = string.IsNullOrEmpty(bech32) ? null : NPub.FromBech32($"npub1{bech32}");
            }
            catch
            {
                requestedNPub = null;
            }
            return requestedNPub is not null;
        }


        internal static byte[] UNKNOWN_IDENTITY(NPub npub)
        {
            return npub.Bech32.Replace("npub1", "").UTF8AsByteArray();
        }
        internal static bool UNKNOWN_IDENTITY(Span<byte> body, out NPub? requestedNPub)
        {
            requestedNPub = null;
            string bech32 = body.Slice(0, 58).ToArray().ToUTF8String();
            try
            {
                requestedNPub = string.IsNullOrEmpty(bech32) ? null : NPub.FromBech32($"npub1{bech32}");
            }
            catch
            {
                requestedNPub = null;
            }
            return requestedNPub is not null;
        }


        internal static byte[] P2P_CONNECTION_COORDINATION(NPub npub, EndPoint? peerEP1, EndPoint? peerEP2, EndPoint? peerEP3)
        {
            byte[] encryptedNPub = npub.Bech32.Replace("npub1", "").UTF8AsByteArray();

            // Endpoints are always padded to reach a fixed lenght
            byte[] encryptedEP1 = peerEP1 is null ? new string('_', 21).UTF8AsByteArray() : peerEP1.ToIPV4String().PadRight(21, '_').UTF8AsByteArray();
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
        internal static bool P2P_CONNECTION_COORDINATION(Span<byte> body, out NPub? peerNPub, out EndPoint? peerEP1, out EndPoint? peerEP2, out EndPoint? peerEP3)
        {
            peerNPub = null;
            peerEP1 = null;
            peerEP2 = null;
            peerEP3 = null;

            string bech32 = body.Slice(0, 58).ToArray().ToUTF8String();
            try
            {
                peerNPub = string.IsNullOrEmpty(bech32) ? null : NPub.FromBech32($"npub1{bech32}");
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


        internal static byte[] CONNECTION_POSSIBLE(string nPubBech32)
        {
            return Encoding.UTF8.GetBytes(nPubBech32.Replace("npub1", ""));
        }
        internal static bool CONNECTION_POSSIBLE(Span<byte> body, out NPub? npub)
        {
            npub = null;
            string bech32 = body.Slice(0, 58).ToArray().ToUTF8String();
            try
            {
                npub = string.IsNullOrEmpty(bech32) ? null : NPub.FromBech32($"npub1{bech32}");
            }
            catch
            {
                npub = null;
            }
            return npub is not null;
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


        internal static byte[] ExtractFromPacket(byte[] packet)
        {
            byte[] header = Header.Deserialize(packet).Serialize();
            return new Span<byte>(packet).Slice(header.Length).ToArray();
        }
    }
}
