using RUDP.Enums;
using RUDP.Extensions;
using System.Text;

namespace RUDP.Utilities
{
    internal class Header
    {
        internal const int _sigSize = 32;
        internal const int _npubHexSize = 64;
        internal static int _minSize => 1/*Type*/;
        internal static int _minSignedSize => 1/*Type*/ + _sigSize;


        internal PacketType? Type { get; set; } = null;
        internal string? Signature { get; set; } = null;
        internal string? NPubHex { get; set; } = null;
        internal uint? PacketIdentifier { get; set; } = null;
        internal uint? ChunkNumber { get; set; } = null;
        //internal byte[]? IV { get; set; } = null;
        internal int Length => (Type.HasValue ? 1 : 0)
            + _sigSize
            + (PacketIdentifier.HasValue ? 4 : 0)
            + (ChunkNumber.HasValue ? 4 : 0); // + (IV is not null ? 16 : 0);


        internal static Header DATA(string hex, uint uniqueIdentifier, uint chunkNumber) //, byte[] ivBytes)
        {
            return new()
            {
                Type = PacketType.DATA,
                NPubHex = hex,
                PacketIdentifier = uniqueIdentifier,
                ChunkNumber = chunkNumber,
                //IV = ivBytes
            };
        }
        internal static Header STREAM(string hex) //byte[] ivBytes)
        {
            return new()
            {
                Type = PacketType.STREAM,
                NPubHex = hex,
                //IV = ivBytes
            };
        }
        internal static Header RTTA(string hex, uint uniqueIdentifier)
        {
            return new()
            {
                Type = PacketType.RTTA,
                PacketIdentifier = uniqueIdentifier
            };
        }
        internal static Header RTTB(string hex, uint uniqueIdentifier)
        {
            return new()
            {
                Type = PacketType.RTTB,
                PacketIdentifier = uniqueIdentifier
            };
        }
        internal static Header ACKNOWLEDGEMENT(string hex, uint uniqueIdentifier, uint? chunkNumber)
        {
            return new()
            {
                Type = PacketType.ACKNOWLEDGEMENT,
                PacketIdentifier = uniqueIdentifier,
                ChunkNumber = chunkNumber
            };
        }
        internal static Header MTU_DISCOVERY(string hex)
        {
            return new()
            {
                Type = PacketType.MTU_DISCOVERY,
                NPubHex = hex
            };
        }
        internal static Header MTU_FOUND(string hex, uint uniqueIdentifier)
        {
            return new()
            {
                Type = PacketType.MTU_FOUND,
                PacketIdentifier = uniqueIdentifier
            };
        }
        internal static Header HANDSHAKE(string? sig, uint uniqueIdentifier)
        {
            return new()
            {
                Type = PacketType.HANDSHAKE,
                Signature = sig,
                PacketIdentifier = uniqueIdentifier
            };
        }
        internal static Header CONNECTION_CONFIRM(string? sig, uint uniqueIdentifier)
        {
            return new()
            {
                Type = PacketType.CONNECTION_CONFIRM,
                Signature = sig,
                PacketIdentifier = uniqueIdentifier
            };
        }
        internal static Header P2P_COORDINATION_REQUEST(string hex, uint uniqueIdentifier)
        {
            return new()
            {
                Type = PacketType.P2P_COORDINATION_REQUEST,
                PacketIdentifier = uniqueIdentifier,
                //IV = ivBytes
            };
        }
        internal static Header UNKNOWN_IDENTITY(string hex, uint uniqueIdentifier) //, byte[] ivBytes)
        {
            return new()
            {
                Type = PacketType.UNKNOWN_IDENTITY,
                PacketIdentifier = uniqueIdentifier,
                //IV = ivBytes
            };
        }
        internal static Header P2P_CONNECTION_COORDINATION(string hex)
        {
            return new()
            {
                Type = PacketType.P2P_CONNECTION_COORDINATION
            };
        }
        internal static Header CONNECTION_POSSIBLE(string hex)
        {
            return new()
            {
                Type = PacketType.CONNECTION_POSSIBLE
            };
        }
        internal static Header DISCONNECTION(string hex, uint uniqueIdentifier) //, byte[] ivBytes)
        {
            return new()
            {
                Type = PacketType.DISCONNECTION,
                PacketIdentifier = uniqueIdentifier,
                //IV = ivBytes
            };
        }
        internal static Header SIGNALING_PROPAGATION(string hex, uint uniqueIdentifier) //, byte[] ivBytes)
        {
            return new()
            {
                Type = PacketType.SIGNALING_PROPAGATION,
                PacketIdentifier = uniqueIdentifier,
                //IV = ivBytes
            };
        }
        internal static Header FILE_PRESENTATION(string hex, uint uniqueIdentifier, uint chunkNumber) //, byte[] ivBytes)
        {
            return new()
            {
                Type = PacketType.FILE_PRESENTATION,
                PacketIdentifier = uniqueIdentifier,
                ChunkNumber = chunkNumber,
                //IV = ivBytes
            };
        }
        internal static Header FILE(string hex, uint uniqueIdentifier, uint chunkNumber) //, byte[] ivBytes)
        {
            return new()
            {
                Type = PacketType.FILE,
                PacketIdentifier = uniqueIdentifier,
                ChunkNumber = chunkNumber,
                //IV = ivBytes
            };
        }


        internal byte[] Serialize()
        {
            if (Type is null)
                throw new ApplicationException("Type missing from header");

            byte[] sg = !string.IsNullOrEmpty(Signature) ? Signature.UTF8AsByteArray() : new byte[0];
            byte[] pi = PacketIdentifier.HasValue ? BitConverter.GetBytes(PacketIdentifier.Value) : new byte[0];
            byte[] cn = ChunkNumber.HasValue ? BitConverter.GetBytes(ChunkNumber.Value) : new byte[0];

            byte[] header = new byte[1 + sg.Length + pi.Length + cn.Length];
            header[0] = (byte)Type;
            Array.Copy(sg, 0, header, 1, sg.Length);
            Array.Copy(pi, 0, header, 1 + sg.Length, pi.Length);
            Array.Copy(cn, 0, header, 1 + sg.Length + pi.Length, cn.Length);

            sg = new byte[0];
            pi = new byte[0];
            cn = new byte[0];

            return header;
        }
        /// <summary>
        /// Takes the packet as parameter and return a deserialized instance of the header section
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        internal static Header Deserialize(byte[] packet)
        {
            Span<byte> span = new Span<byte>(packet);

            Header header = new Header();
            header.Type = (PacketType)span[0];
            switch (header.Type)
            {
                case PacketType.DATA:
                    header.PacketIdentifier = BitConverter.ToUInt16(span.Slice(_npubHexSizeBytes + 1, 4));
                    header.ChunkNumber = BitConverter.ToUInt32(span.Slice(_npubHexSizeBytes + 5, 4));
                    //if (header.ChunkNumber <= 1)
                    //    header.IV = span.Slice(9, 16).ToArray();
                    break;
                case PacketType.STREAM:
                    //header.IV = span.Slice(1, 16).ToArray();
                    break;
                case PacketType.ACKNOWLEDGEMENT:
                    header.PacketIdentifier = BitConverter.ToUInt16(span.Slice(_npubHexSizeBytes + 1, 4));
                    if (span.Length > 5)
                        header.ChunkNumber = BitConverter.ToUInt32(span.Slice(_npubHexSizeBytes + 5, 4));
                    break;
                case PacketType.MTU_DISCOVERY:
                case PacketType.P2P_CONNECTION_COORDINATION:
                case PacketType.CONNECTION_POSSIBLE:
                    break;
                case PacketType.HANDSHAKE:
                    header.Signature = Encoding.UTF8.GetString(span.Slice(1, _sigSize));
                    header.PacketIdentifier = BitConverter.ToUInt16(span.Slice(1 + _sigSize, 4));
                    break;
                case PacketType.CONNECTION_CONFIRM:
                case PacketType.P2P_COORDINATION_REQUEST:
                case PacketType.UNKNOWN_IDENTITY:
                case PacketType.DISCONNECTION:
                case PacketType.SIGNALING_PROPAGATION:
                    header.PacketIdentifier = BitConverter.ToUInt16(span.Slice(_npubHexSizeBytes + 1, 4));
                    break;
                case PacketType.MTU_FOUND:
                case PacketType.RTTA:
                case PacketType.RTTB:
                    header.PacketIdentifier = BitConverter.ToUInt16(span.Slice(_npubHexSizeBytes + 1, 4));
                    break;
            }
            return header;
        }
    }
}
