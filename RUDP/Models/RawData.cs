using System.Net;

namespace RUDP.Models
{
    internal class RawData : IDisposable
    {
        internal EndPoint EP { get; set; }
        internal byte[] Data { get; set; }


        internal RawData() { }
        internal RawData(EndPoint ep, byte[] data)
        {
            EP = ep;
            Data = data;
        }

        public void Dispose()
        {
            EP = null;
            Data = new byte[0];
        }
    }
}
