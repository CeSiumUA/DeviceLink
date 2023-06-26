using DeviceLink.CLI;
using DeviceLink.Shared;
using DeviceLink.Shared.Protocol;
using DeviceLink.Shared.Protocol.Messages;
using MessagePack;
using System.Net;
using System.Net.Sockets;

Dictionary<Guid, Peer> peers = new Dictionary<Guid, Peer>();

Peer? selectedPeer = default;

using var player = new Player();

using var network = new Network(ProtocolReceive, AudioReceive);

using var recorder = new Recorder(SendSound);

network.StartListener();
network.StartDiscover();

player.StartPlayer();

while (true)
{
    Console.WriteLine("Type \"exit\" anything to exit...");
    string? cmd = Console.ReadLine();
    if (string.IsNullOrEmpty(cmd))
    {
        continue;
    }

    if(cmd.ToLower() == "exit")
    {
        break;
    }
    else
    {
        if(int.TryParse(cmd, out var peer))
        {
            selectedPeer = peers.Values.ToArray()[peer - 1];
        }
        else if(Guid.TryParse(cmd, out var peerGUid))
        {
            selectedPeer = peers[peerGUid];
        }
        else
        {
            Console.WriteLine("Bad argument value, enter peer number");
            continue;
        }
        recorder.StartRecorder();
        Console.WriteLine("Sending data to peer: {0} with IP: {1}", selectedPeer.HostName, selectedPeer.IpEndPoint.Address.ToString());
    }
}

var removalTask = new Task(async () =>
{
    foreach(var peer in peers)
    {
        var timeDiff = (peer.Value.LastUpdateTime - DateTimeOffset.UtcNow);
        if (timeDiff > TimeSpan.FromSeconds(10))
        {
            peers.Remove(peer.Key);
        }
    }

    await Task.Delay(500);
});
removalTask.Start();

void ProtocolReceive(Network network, IPEndPoint ipep, byte[] data)
{
    var protocol = Protocol.DeserializeProtocol(data);

    switch (protocol.Type)
    {
        case DeviceLink.Shared.Protocol.ProtocolType.Discover:
            ProcessDiscoverMessage(network, protocol.Data, ipep);
            break;
        case DeviceLink.Shared.Protocol.ProtocolType.DiscoverResponse:
            ProcessDiscoverResponseMessage(network, protocol.Data, ipep);
            break;
    }
}

void AudioReceive(IPEndPoint iPEndPoint, byte[] data)
{
    player.Play(data);
}

void ProcessDiscoverResponseMessage(Network network, byte[] data, IPEndPoint ipep)
{
    var discoverResponseData = MessagePackSerializer.Deserialize<DiscoverResponse>(data);
    if(discoverResponseData.TargetGuid == network.Id)
    {
        network.SetPassiveScan();

        if (!peers.ContainsKey(discoverResponseData.ResponseGuid))
        {
            peers[discoverResponseData.ResponseGuid] = new Peer(discoverResponseData.ResponseGuid, ipep, discoverResponseData.HostName);
            Console.WriteLine("Added peer: {0} with IP: {1}", discoverResponseData.HostName, ipep.Address.ToString());
            PrintPeersTable();
        }
        else
        {
            peers[discoverResponseData.ResponseGuid].LastUpdateTime = DateTimeOffset.UtcNow;
        }
    }
}

void SendSound(byte[] data, int len)
{
    if(selectedPeer != null)
    {
        network.SendData(data, len, selectedPeer.IpEndPoint.Address);
    }
}

void PrintPeersTable()
{
    Console.WriteLine("Available audio devices:");
    int i = 1;
    foreach (var peer in peers)
    {
        Console.WriteLine("{0}\t{1}\t{2}", i, peer.Value.HostName, peer.Value.Id);
    }
}

void ProcessDiscoverMessage(Network network, byte[] data, IPEndPoint ipep)
{
    var discoverData = MessagePackSerializer.Deserialize<Discover>(data);
    var protocol = Protocol.ConstructDiscoverResponse(network.Id, discoverData.Id, Environment.MachineName);
    network.SendData(protocol.Serialize(), ipep);
}