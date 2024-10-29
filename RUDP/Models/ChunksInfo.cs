namespace RUDP.Models
{
    internal class ChunksInfo : IDisposable
    {
        internal uint TotalChunks { get; set; }
        internal List<ChunkData> Chunks { get; set; } = new();


        internal ChunksInfo(uint totalChunks)
        {
            TotalChunks = totalChunks;
        }


        public void Dispose()
        {
            foreach (ChunkData chunk in Chunks)
                chunk.Dispose();
            Chunks.Clear();
        }
    }
}
