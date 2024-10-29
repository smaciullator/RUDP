using ByteSizeLib;
using RUDP.Models;

namespace SignalingServer.Models
{
    public class EPDetailsInfo : EPInfo
    {
        public string CurrentUploadSpeed => new ByteSize(BytesUploadPerSecond).ToString();
        public string CurrentDownloadSpeed => new ByteSize(BytesDownloadPerSecond).ToString();


        public EPDetailsInfo() { }
        public EPDetailsInfo(EPInfo info) : base(info) { }
    }
}
