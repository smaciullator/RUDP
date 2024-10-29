using NostrSharp.Keys;
using RUDP;
using RUDP.Extensions;
using RUDP.Utilities;
using System.Net;


Console.WriteLine("RUDP - Client");


BaseRUDPSocket udp = new BaseRUDPSocket(false, 40003, "nsec1hr304hv3ck5weqcjc45nj8vyqtrrrww39aymyvgaaxyzed3f0fxs57tjda");
udp.OnTotalRateUpdated += (rates, sendBufferFill) =>
{
    string inOut = $"{rates.SentPacketsPerSecond}sp/s ({rates.SentBytesPerSecond} bytes), {rates.ReceivedPacketsPerSecond}rp/s ({rates.ReceivedBytesPerSecond} bytes)";
    Console.Write("\r{0}%   ", $"{inOut} - send buff fill:{sendBufferFill}");
};
udp.OnConnectionConfirmed += (ep, npub, isSigServer, relativeIndex) =>
{
    if (isSigServer)
    {
        Console.Write("\r{0}%   ", $"SigServer connected with endpoint {ep.ToIPV4String()}");
        Console.WriteLine("");
    }
    else
    {
        Console.Write("\r{0}%   ", $"Peer connected with endpoint {ep.ToIPV4String()}");
        Console.WriteLine("");

        Task.Run(async () =>
        {
            while (true)
            {
                udp.SendStream(ep, new byte[500]);
            }
        });
    }
};
udp.OnP2PConnectionPossible += (ep, npub) =>
{
    ThreadUtilities.PauseThread(500);
    udp.TryConnectWith(ep);
};
udp.Start();


ThreadUtilities.PauseThread(6000);
EndPoint sigServerEP = $"{NetworkUtilities.GetLocalIPAddress()}:40001".ToEndPoint();
udp.TryConnectWith(sigServerEP, 1);
EndPoint sigServerEP2 = $"{NetworkUtilities.GetLocalIPAddress()}:40002".ToEndPoint();
udp.TryConnectWith(sigServerEP2, 2);


Console.WriteLine("Premi un tasto qualsiasi per connetterti con l'altro Peer");
Console.ReadLine();

// Mi connetto con un peer di cui conosco la NPub
udp.SendP2PCoordinationRequest(sigServerEP, NSKeyPair.From(NSec.FromBech32("nsec1cptklu084ypqtkjhgvn6y0da270pd8ta8klevv3hvad4uvnnfzuqk3slhr")).NPub.Bech32);


while (udp.Status == RUDP.Enums.SocketStatus.Running)
    ThreadUtilities.PauseThread(3000);


Console.WriteLine("Il socket è stoppato. Premi un tasto qualsiasi per uscire");
Console.ReadKey();