namespace RUDP.Models
{
    public class Rates
    {
        public double SentBytesPerSecond { get; set; } = 0;
        public int SentPacketsPerSecond { get; set; } = 0;
        public double ReceivedBytesPerSecond { get; set; } = 0;
        public int ReceivedPacketsPerSecond { get; set; } = 0;


        public Rates() { }
        public Rates(double sentBytesPerSecond, int sentPacketsPerSecond, double receivedBytesPerSecond, int receivedPacketsPerSecond)
        {
            SentBytesPerSecond = sentBytesPerSecond;
            SentPacketsPerSecond = sentPacketsPerSecond;
            ReceivedBytesPerSecond = receivedBytesPerSecond;
            ReceivedPacketsPerSecond = receivedPacketsPerSecond;
        }
        public Rates(Rates rates)
        {
            SentBytesPerSecond = rates.SentBytesPerSecond;
            SentPacketsPerSecond = rates.SentPacketsPerSecond;
            ReceivedBytesPerSecond = rates.ReceivedBytesPerSecond;
            ReceivedPacketsPerSecond = rates.ReceivedPacketsPerSecond;
        }
    }
}
