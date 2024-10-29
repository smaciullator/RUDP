using RUDP.Utilities;

namespace RUDP.Models
{
    internal class ChunkData : IDisposable
    {
        internal ulong ChunkNumber { get; set; }
        internal byte[] Chunk { get; set; } = new byte[0];


        internal ChunkData(Header header, byte[] packet)
        {
            ChunkNumber = header.ChunkNumber.Value;
            Chunk = new Span<byte>(packet).Slice(header.Length).ToArray();
        }


        public void Dispose()
        {
            Chunk = new byte[0];
        }
    }
}
