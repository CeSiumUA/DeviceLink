using DeviceLink.CLI;
using DeviceLink.Shared;
using DeviceLink.Shared.Protocol;
using DeviceLink.Shared.Protocol.Messages;
using MessagePack;
using System.Net;
using System.Net.Sockets;

Dictionary<Guid, Peer> peers = new Dictionary<Guid, Peer>();

using var network = new Network(ProtocolReceive, AudioReceive);
network.StartListener();
network.StartDiscover();

Console.WriteLine("Type anything to exit...");
Console.ReadLine();

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
    }
}

void PrintPeersTable()
{
    Console.WriteLine("Available audio devices:");
    int i = 1;
    foreach (var peer in peers)
    {
        Console.WriteLine("{0}\t{1}", i, peer.Value.HostName);
    }
}

void ProcessDiscoverMessage(Network network, byte[] data, IPEndPoint ipep)
{
    var discoverData = MessagePackSerializer.Deserialize<Discover>(data);
    var protocol = Protocol.ConstructDiscoverResponse(network.Id, discoverData.Id, Environment.MachineName);
    network.SendData(protocol.Serialize(), ipep);
}