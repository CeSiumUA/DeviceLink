using DeviceLink.Shared;
using DeviceLink.Shared.Protocol;
using System.Net;

using var network = new Network(ProtocolReceive, AudioReceive);
network.StartListener();
network.StartDiscover();

Console.WriteLine("Type anything to exit...");
Console.ReadLine();

void ProtocolReceive(IPEndPoint ipep, byte[] data)
{
    Console.WriteLine("Data received from: {0}:{1}", ipep.Address.ToString(), ipep.Port);

    var protocol = Protocol.DeserializeProtocol(data);

    Console.WriteLine("Package type: {0}", protocol.Type.ToString());
}

void AudioReceive(IPEndPoint iPEndPoint, byte[] data)
{

}