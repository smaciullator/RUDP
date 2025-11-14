using System.Collections.Concurrent;

namespace RUDP.Models
{
    internal class FileData : IDisposable
    {
        internal string FileFullPath { get; set; }
        internal long ChunksNumber { get; set; }
        internal long CurrentChunk { get; set; }
        internal FileStream Stream { get; set; }
        internal ConcurrentDictionary<uint, byte[]> ChunksBuffer { get; set; }
        internal bool AllChunksReceived => ChunksNumber == CurrentChunk + 1;


        internal FileData() { }
        internal FileData(string fileFullPath, long chunksNumber)
        {
            FileFullPath = fileFullPath;
            ChunksNumber = chunksNumber;
        }
        internal FileData(string fileFullPath, long chunksNumber, FileStream stream)
        {
            FileFullPath = fileFullPath;
            ChunksNumber = chunksNumber;
            CurrentChunk = 0;
            Stream = stream;
            ChunksBuffer = new();
        }


        public void Dispose()
        {
            if (Stream is not null)
                Stream.Dispose();
            if (ChunksBuffer is not null)
                ChunksBuffer.Clear();
        }
    }
}
