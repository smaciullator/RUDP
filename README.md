# RUDP
## Implementation in C# of a custom transmission protocol based upon UDP Sockets.
DISCLAIMER: i made this repo for fun, use it freely in any kind of projects you want but remember it's made for fun.

## Introduction
Think about this protocol as a connection-less TCP, where you use the speed of UDP with **a bit** of the reliability of TCP.
The implementation efficiently send and receive UDP packets in a multi-thread way.
While the send and receive part is quite standard, the interesting part is on the protocol itself.
It features a set of base commands, or *PacketType*, meant for providing a way to create a direct socket *connection* (UDP is connection-less) between peers, encrypting the data transfer, optionally managing resend of lost packets and managing **MTU size** and **Congestion Window**.

Since it's a way to transfer bytes peer-to-peer you can use to send whatever kind of data you want on top of it.

If you know the public IPv4 endpoint of your peer you can try a direct connection with the protocol, but for the connection to succed he need a port forwarding rule and a firewall exception: not very practical nor suggested.
Also, tipically you don't know the public ip+port of a peer.

To circumvent this problem the protocol implement a **UDP Hole Punch** tecnique, used *in sync by both connecting peers*, to establish a connection on *most* network.
**NOTE:** a particularly adverse NAT or a VPN preventing UDP packets will very probably make this kind of connection impossible, anyway it *should* still be possibile on most network topographies.

The syncing is achieved with a special middle-man, called *Signaling Server**, which has to be a known endpoint to which both peers will announce themselves.
Announcing to a Signaling Server means to communicate him your *public key* and let him correlate that with your endpoint (ip+port).
When both peers have announced themselves, one of them may try to start a connection by requesting a coordination to the Signaling Server using the *public key* of the peer he wish to connect with.
The Signaling Server simultaneously send the endpoint of the **peer B** to the **peer A**, and the endpoint of the **peer A** to the **peer B**.
After both peers confirm the reception of the coordination packet, it's no longer needed.
Each peer then start trying to connect to the freshly received public endpoint.
In case of success the connection is maintained via a recurring send of small packets between the peers, primarily used to prevent the endpoints loosing usability after long enough inactivity time.

To better achieve a connection, peers should announce themselves to at least 1 and less than 4 signaling server, where 1-2 is tipically enough and 3 doesn't significantly increase the connection chances (but can help in few cases).
The number of Signaling Server a peer should announce himself to depend on the kind of it's current network location: nat networks require 2 signaling servers at least.

This repository contains an implementation of both a Client (peer) and a Server (Signaling Server) you can play with.

You may noticed i wrote *public key* earlier when summarizing the connection phase.
This protocol use key pairs to achieve both identity and data encryption, the same used in the **Nostr** protocol.
You can use an existing Nostr keys.

## Things to be improved
Since any Signaling Server can see and store usable endpoints in pair with the peer public key i would love to find a way to make it more private, but i'm currently not sure on how to do it.
The role of a Signaling Server is to provide some reliable and very much needed information to establish peer-to-peer connections, but it's a privacy nightmare since there's no guarantee the Signaling Server will forget your public endpoint after a succesful connection is made.
The best approach for now is to have some trusted Signaling Servers.
I'm open to suggestions on ways to improve it.


## Technical details
### MultiThreadUDPSocket Class
This class is the core of the library.
It's an event-based, asyncronous wrapper of the System.Net.Sockets.Socket, that bind itself to a choosen or automatic port, then when started cast 4 background long running tasks to:
1. Send queue: each packet you want to send is first added to a ConcurrentQueue, then picked with a FIFO priority
2. Packet Reception: receive and store each packet to another ConcurrentQueue, reducing at the very minimum the time that intervenes between reception and restart of listening
3. Emit queue: each received packet is emitted with a FIFO priority
4. Metric: each second emit an evento to notify the exact amount of bytes, both sent and received, along with a percentage of the send buffer current fill status.

This class also store the amount of bytes sent and received by each different endpoint.

You can also set a max upload speed for each endpoint and/or in total, by setting the number of bytes per second.

### BaseRUDPSocket Class
It's a wrapper where the protocol logic gets applied.

Can be configured to run as a *Client* or a *Signaling Server*, also you can set a custom port to be used, insted of letting the OS choose for you.
As stated before you can set the current identity (private-public key pair) for the instance, but if not provided a new ephemeral key pair will be generated at startup.
Will use an instance of **MultiThreadUDPSocket**.

Between all of the public methods i'm gonna do a quick dive inside some of the less self-explaining:
1. *TryConnectWith*: used to connect remotely to another peer, involving a Signaling Server
2. *TryConnectLocallyWith*: used to connect to a peer inside your local network, doesn't require any Signaling Server
3. *SendData*, *SendStream* and *SendFile*: those 3 all are intended for Client use, they send bytes to a peer but their behaviour and intended use is a bit different:
  - SendData: used to send data of any size, will always require a confirm of the reception (ACK, 9 bytes packet) from the receiver for each single packet sent
  - SendStream: used to send data of any size, will never require any confirmation from the receiver, just throw the data on the void
  - SendFile: used to send files of anysize, will create a temporary file (will let user choose path in future release) to write each packet to as soon as it arrives, keeping it in ram only for the strictly necessary time
4. *SendP2PCoordinationRequest*: used by Client to request a Signaling Server a coordination to connect with a specific identity (public key)
5. *SendUnknownIdentity*: used by Signaling Server to notify a peer he doesn't know nothing of the identity he requested a coordination for
6. *SendP2PConnectionCoordination*: used by Signaling Server to coordinate Peers efforts on establishing a direct connection
7. *SendSignalingPropagation*: used by Signaling Server to propagate a received peer announcement to other Signaling Servers it is aware of. **NOTE**: this behaviour is optional on the Signaling Server implementation, but may help a lot to increase connection chances
8. *GetEPMTUSize*: return the MTU size of a specific peer
