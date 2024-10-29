using System.Collections.Concurrent;

namespace RUDP.Models
{
    internal class FileData : IDisposable
    {
        internal string FileFullPath { get; set; }
        internal string FileName { get; set; }
        internal long ChunksNumber { get; set; }
        internal long CurrentChunk { get; set; }
        internal FileStream Stream { get; set; }
        internal ConcurrentDictionary<uint, byte[]> EarlyChunks { get; set; }
        /// <summary>
        /// Flag usato per indicare se stiamo recuperando i chunks arrivati in seguito e salvati temporaneamente nella proprietà EarlyChunks
        /// </summary>
        internal bool Recovering { get; set; }


        internal FileData() { }
        internal FileData(string fileFullPath, long chunksNumber)
        {
            FileFullPath = fileFullPath;
            ChunksNumber = chunksNumber;
        }
        internal FileData(string fileName, long chunksNumber, FileStream stream)
        {
            FileName = fileName;
            ChunksNumber = chunksNumber;
            CurrentChunk = 0;
            Stream = stream;
            EarlyChunks = new();
            Recovering = false;
        }


        public void Dispose()
        {
            if (Stream is not null)
                Stream.Dispose();
            if (EarlyChunks is not null)
                EarlyChunks.Clear();
        }
    }
}
