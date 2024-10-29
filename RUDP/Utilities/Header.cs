using RUDP.Enums;

namespace RUDP.Utilities
{
    internal class Header
    {
        internal PacketType? Type { get; set; } = null;
        internal uint? PacketIdentifier { get; set; } = null;
        internal uint? ChunkNumber { get; set; } = null;
        internal byte[]? IV { get; set; } = null;
        internal int Length => (Type.HasValue ? 1 : 0) + (PacketIdentifier.HasValue ? 4 : 0) + (ChunkNumber.HasValue ? 4 : 0) + (IV is not null ? 16 : 0);


        internal static Header DATA(uint uniqueIdentifier, uint chunkNumber, byte[] ivBytes)
        {
            return new()
            {
                Type = PacketType.DATA,
                PacketIdentifier = uniqueIdentifier,
                ChunkNumber = chunkNumber,
                IV = ivBytes
            };
        }
        internal static Header STREAM(byte[] ivBytes)
        {
            return new()
            {
                Type = PacketType.STREAM,
                IV = ivBytes
            };
        }
        internal static Header RTTA(uint uniqueIdentifier)
        {
            return new()
            {
                Type = PacketType.RTTA,
                PacketIdentifier = uniqueIdentifier
            };
        }
        internal static Header RTTB(uint uniqueIdentifier)
        {
            return new()
            {
                Type = PacketType.RTTB,
                PacketIdentifier = uniqueIdentifier
            };
        }
        internal static Header ACKNOWLEDGEMENT(uint uniqueIdentifier, uint? chunkNumber)
        {
            return new()
            {
                Type = PacketType.ACKNOWLEDGEMENT,
                PacketIdentifier = uniqueIdentifier,
                ChunkNumber = chunkNumber
            };
        }
        internal static Header MTU_DISCOVERY()
        {
            return new()
            {
                Type = PacketType.MTU_DISCOVERY
            };
        }
        internal static Header MTU_FOUND(uint uniqueIdentifier)
        {
            return new()
            {
                Type = PacketType.MTU_FOUND,
                PacketIdentifier = uniqueIdentifier
            };
        }
        internal static Header HANDSHAKE(uint uniqueIdentifier, byte[] ivBytes)
        {
            return new()
            {
                Type = PacketType.HANDSHAKE,
                PacketIdentifier = uniqueIdentifier,
                IV = ivBytes
            };
        }
        internal static Header CONNECTION_CONFIRM(uint uniqueIdentifier, byte[] ivBytes)
        {
            return new()
            {
                Type = PacketType.CONNECTION_CONFIRM,
                PacketIdentifier = uniqueIdentifier,
                IV = ivBytes
            };
        }
        internal static Header P2P_COORDINATION_REQUEST(uint uniqueIdentifier, byte[] ivBytes)
        {
            return new()
            {
                Type = PacketType.P2P_COORDINATION_REQUEST,
                PacketIdentifier = uniqueIdentifier,
                IV = ivBytes
            };
        }
        internal static Header UNKNOWN_IDENTITY(uint uniqueIdentifier, byte[] ivBytes)
        {
            return new()
            {
                Type = PacketType.UNKNOWN_IDENTITY,
                PacketIdentifier = uniqueIdentifier,
                IV = ivBytes
            };
        }
        internal static Header P2P_CONNECTION_COORDINATION()
        {
            return new()
            {
                Type = PacketType.P2P_CONNECTION_COORDINATION
            };
        }
        internal static Header CONNECTION_POSSIBLE()
        {
            return new()
            {
                Type = PacketType.CONNECTION_POSSIBLE
            };
        }
        internal static Header DISCONNECTION(uint uniqueIdentifier, byte[] ivBytes)
        {
            return new()
            {
                Type = PacketType.DISCONNECTION,
                PacketIdentifier = uniqueIdentifier,
                IV = ivBytes
            };
        }
        internal static Header SIGNALING_PROPAGATION(uint uniqueIdentifier, byte[] ivBytes)
        {
            return new()
            {
                Type = PacketType.SIGNALING_PROPAGATION,
                PacketIdentifier = uniqueIdentifier,
                IV = ivBytes
            };
        }
        internal static Header FILE_PRESENTATION(uint uniqueIdentifier, uint chunkNumber, byte[] ivBytes)
        {
            return new()
            {
                Type = PacketType.FILE_PRESENTATION,
                PacketIdentifier = uniqueIdentifier,
                ChunkNumber = chunkNumber,
                IV = ivBytes
            };
        }
        internal static Header FILE(uint uniqueIdentifier, uint chunkNumber, byte[] ivBytes)
        {
            return new()
            {
                Type = PacketType.FILE,
                PacketIdentifier = uniqueIdentifier,
                ChunkNumber = chunkNumber,
                IV = ivBytes
            };
        }


        internal byte[] Serialize()
        {
            if (Type is null)
                throw new ApplicationException("Type missing from header");

            byte[] pi = PacketIdentifier.HasValue ? BitConverter.GetBytes(PacketIdentifier.Value) : new byte[0];
            byte[] cn = ChunkNumber.HasValue ? BitConverter.GetBytes(ChunkNumber.Value) : new byte[0];
            byte[] iv = IV is not null ? IV : new byte[0];

            byte[] header = new byte[1 + pi.Length + cn.Length + iv.Length];
            header[0] = (byte)Type;
            Array.Copy(pi, 0, header, 1, pi.Length);
            Array.Copy(cn, 0, header, 1 + pi.Length, cn.Length);
            Array.Copy(iv, 0, header, 1 + pi.Length + cn.Length, iv.Length);

            pi = new byte[0];
            cn = new byte[0];
            iv = new byte[0];

            return header;
        }
        /// <summary>
        /// Takes the packet as parameter and return a deserialized instance of the header section
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        internal static Header Deserialize(byte[] packet)
        {
            Header header = new Header();
            header.Type = (PacketType)packet[0];
            Span<byte> span = new Span<byte>(packet);
            switch (header.Type)
            {
                case PacketType.DATA:
                    header.PacketIdentifier = BitConverter.ToUInt16(span.Slice(1, 4));
                    header.ChunkNumber = BitConverter.ToUInt32(span.Slice(5, 4));
                    if (header.ChunkNumber <= 1)
                        header.IV = span.Slice(9, 16).ToArray();
                    break;
                case PacketType.STREAM:
                    header.IV = span.Slice(1, 16).ToArray();
                    break;
                case PacketType.ACKNOWLEDGEMENT:
                    header.PacketIdentifier = BitConverter.ToUInt16(span.Slice(1, 4));
                    if (span.Length > 5)
                        header.ChunkNumber = BitConverter.ToUInt32(span.Slice(5, 4));
                    break;
                case PacketType.MTU_DISCOVERY:
                case PacketType.P2P_CONNECTION_COORDINATION:
                case PacketType.CONNECTION_POSSIBLE:
                    break;
                case PacketType.HANDSHAKE:
                case PacketType.CONNECTION_CONFIRM:
                case PacketType.P2P_COORDINATION_REQUEST:
                case PacketType.UNKNOWN_IDENTITY:
                case PacketType.DISCONNECTION:
                case PacketType.SIGNALING_PROPAGATION:
                    header.PacketIdentifier = BitConverter.ToUInt16(span.Slice(1, 4));
                    header.IV = span.Slice(5, 16).ToArray();
                    break;
                case PacketType.MTU_FOUND:
                case PacketType.RTTA:
                case PacketType.RTTB:
                    header.PacketIdentifier = BitConverter.ToUInt16(span.Slice(1, 4));
                    break;
            }
            return header;
        }
    }
}
