using RUDP.Enums;
using RUDP.Extensions;
using System.Text;

namespace RUDP.Utilities
{
    internal class Header
    {
        internal const int _sigSize = 128;
        internal static int _minSize => 1/*Type*/ + _sigSize;
        internal static int _minFileContentSize => _minSize + 4/*PacketIdentifier size*/ + 4/*ChunkNumber size*/;


        internal PacketType? Type { get; set; } = null;
        internal string? Signature { get; set; } = null;
        internal uint? PacketIdentifier { get; set; } = null;
        internal uint? ChunkNumber { get; set; } = null;
        internal int Length => (Type.HasValue ? 1 : 0)
            + _sigSize
            + (PacketIdentifier.HasValue ? 4 : 0)
            + (ChunkNumber.HasValue ? 4 : 0);


        internal static Header RTTA(string? sig, uint uniqueIdentifier)
        {
            return new()
            {
                Type = PacketType.RTTA,
                Signature = sig,
                PacketIdentifier = uniqueIdentifier
            };
        }
        internal static Header RTTB(string? sig, uint uniqueIdentifier)
        {
            return new()
            {
                Type = PacketType.RTTB,
                Signature = sig,
                PacketIdentifier = uniqueIdentifier
            };
        }
        internal static Header ACKNOWLEDGEMENT(string? sig, uint uniqueIdentifier, uint? chunkNumber)
        {
            return new()
            {
                Type = PacketType.ACKNOWLEDGEMENT,
                Signature = sig,
                PacketIdentifier = uniqueIdentifier,
                ChunkNumber = chunkNumber
            };
        }


        internal static Header MTU_DISCOVERY(string? sig, uint uniqueIdentifier)
        {
            return new()
            {
                Type = PacketType.MTU_DISCOVERY,
                Signature = sig,
                PacketIdentifier = uniqueIdentifier
            };
        }
        internal static Header MTU_FOUND(string? sig, uint uniqueIdentifier)
        {
            return new()
            {
                Type = PacketType.MTU_FOUND,
                Signature = sig,
                PacketIdentifier = uniqueIdentifier
            };
        }
        internal static Header DISCONNECTION(string? sig, uint uniqueIdentifier)
        {
            return new()
            {
                Type = PacketType.DISCONNECTION,
                Signature = sig,
                PacketIdentifier = uniqueIdentifier
            };
        }


        internal static Header P2P_COORDINATION_REQUEST(string? sig, uint uniqueIdentifier)
        {
            return new()
            {
                Type = PacketType.P2P_COORDINATION_REQUEST,
                Signature = sig,
                PacketIdentifier = uniqueIdentifier
            };
        }
        internal static Header UNKNOWN_IDENTITY(string? sig, uint uniqueIdentifier)
        {
            return new()
            {
                Type = PacketType.UNKNOWN_IDENTITY,
                Signature = sig,
                PacketIdentifier = uniqueIdentifier
            };
        }
        internal static Header P2P_CONNECTION_COORDINATION(string? sig, uint uniqueIdentifier)
        {
            return new()
            {
                Type = PacketType.P2P_CONNECTION_COORDINATION,
                Signature = sig,
                PacketIdentifier = uniqueIdentifier
            };
        }
        internal static Header CONNECTION_POSSIBLE(string? sig)
        {
            return new()
            {
                Type = PacketType.CONNECTION_POSSIBLE,
                Signature = sig
            };
        }


        internal static Header SIGNALING_PROPAGATION(string? sig, uint uniqueIdentifier)
        {
            return new()
            {
                Type = PacketType.SIGNALING_PROPAGATION,
                Signature = sig,
                PacketIdentifier = uniqueIdentifier
            };
        }


        internal static Header CHUNKS_PRESENTATION(string? sig, uint uniqueIdentifier, uint chunkNumber)
        {
            return new()
            {
                Type = PacketType.CHUNKS_PRESENTATION,
                Signature = sig,
                PacketIdentifier = uniqueIdentifier,
                ChunkNumber = chunkNumber
            };
        }
        internal static Header DATA(string? sig, uint uniqueIdentifier, uint chunkNumber)
        {
            return new()
            {
                Type = PacketType.DATA,
                Signature = sig,
                PacketIdentifier = uniqueIdentifier,
                ChunkNumber = chunkNumber
            };
        }
        internal static Header STREAM(string? sig)
        {
            return new()
            {
                Type = PacketType.STREAM,
                Signature = sig
            };
        }
        internal static Header FILE_PRESENTATION(string? sig, uint uniqueIdentifier, uint chunkNumber)
        {
            return new()
            {
                Type = PacketType.FILE_PRESENTATION,
                Signature = sig,
                PacketIdentifier = uniqueIdentifier,
                ChunkNumber = chunkNumber
            };
        }
        internal static Header FILE_CONTENT(string? sig, uint uniqueIdentifier, uint chunkNumber)
        {
            return new()
            {
                Type = PacketType.FILE_CONTENT,
                Signature = sig,
                PacketIdentifier = uniqueIdentifier,
                ChunkNumber = chunkNumber
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
                case PacketType.STREAM:
                    header.Signature = Encoding.UTF8.GetString(span.Slice(1, _sigSize));
                    break;
                case PacketType.CONNECTION_POSSIBLE:
                    header.Signature = Encoding.UTF8.GetString(span.Slice(1, _sigSize));
                    break;
                case PacketType.MTU_DISCOVERY:
                case PacketType.MTU_FOUND:
                case PacketType.P2P_CONNECTION_COORDINATION:
                case PacketType.P2P_COORDINATION_REQUEST:
                case PacketType.UNKNOWN_IDENTITY:
                case PacketType.SIGNALING_PROPAGATION:
                case PacketType.DISCONNECTION:
                case PacketType.RTTA:
                case PacketType.RTTB:
                    header.Signature = Encoding.UTF8.GetString(span.Slice(1, _sigSize));
                    header.PacketIdentifier = BitConverter.ToUInt16(span.Slice(1 + _sigSize, 4));
                    break;
                case PacketType.ACKNOWLEDGEMENT:
                    header.Signature = Encoding.UTF8.GetString(span.Slice(1, _sigSize));
                    header.PacketIdentifier = BitConverter.ToUInt16(span.Slice(1 + _sigSize, 4));
                    if (span.Length > 1 + _sigSize + 4)
                        header.ChunkNumber = BitConverter.ToUInt32(span.Slice(1 + _sigSize + 4, 4));
                    break;
                case PacketType.CHUNKS_PRESENTATION:
                case PacketType.DATA:
                case PacketType.FILE_PRESENTATION:
                case PacketType.FILE_CONTENT:
                    header.Signature = Encoding.UTF8.GetString(span.Slice(1, _sigSize));
                    header.PacketIdentifier = BitConverter.ToUInt16(span.Slice(1 + _sigSize, 4));
                    header.ChunkNumber = BitConverter.ToUInt32(span.Slice(1 + _sigSize + 4, 4));
                    break;
                default:
                    break;
            }
            return header;
        }
    }
}
