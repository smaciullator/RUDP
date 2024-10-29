using RUDP.Enums;
using RUDP.Utilities;

namespace RUDP.Models
{
    internal class UnackData : IDisposable
    {
        internal string UID => !PacketIdentifier.HasValue ? "" : PacketIdentifier.Value.ToString() + (!ChunkNumber.HasValue ? "" : ChunkNumber.Value.ToString());
        internal PacketType? PacketType { get; set; } = null;
        internal uint? PacketIdentifier { get; set; } = null;
        internal ulong? ChunkNumber { get; set; } = null;
        internal byte[] Packet { get; set; } = new byte[0];
        internal long? Timestamp { get; private set; } = null;


        internal UnackData(byte[] packet)
        {
            Header header = Header.Deserialize(packet);
            if (!header.PacketIdentifier.HasValue)
                return;
            PacketType = header.Type;
            PacketIdentifier = header.PacketIdentifier.Value;
            ChunkNumber = header.ChunkNumber.HasValue ? header.ChunkNumber.Value : null;
            Packet = packet;
            Timestamp = DateTime.Now.Ticks;
        }


        public void Dispose()
        {
            PacketType = null;
            PacketIdentifier = null;
            ChunkNumber = null;
            Packet = new byte[0];
            Timestamp = null;
        }
    }
}
