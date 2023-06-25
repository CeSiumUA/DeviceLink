﻿using DeviceLink.Shared.Protocol.Messages;
using MessagePack;

namespace DeviceLink.Shared.Protocol;

[MessagePackObject]
public class Protocol
{
    [Key(0)]
    public ProtocolType Type { get; set; }

    [Key(1)]
    public byte[] Data { get; set; } = new byte[0];

    public static Protocol ConstructDiscover(Guid id)
    {
        var discoverPackage = new Discover()
        {
            Id = id
        };

        var data = MessagePackSerializer.Serialize(discoverPackage);

        var protocol = new Protocol()
        {
            Type = ProtocolType.Discover,
            Data = data
        };

        return protocol;
    }

    public static Protocol ConstructDiscoverResponse(Guid responseId, Guid targetId, string? machineName = null)
    {
        var discoverResponse = new DiscoverResponse()
        {
            TargetGuid = targetId,
            ResponseGuid = responseId,
        };

        if (!string.IsNullOrEmpty(machineName))
        {
            discoverResponse.HostName = machineName;
        }

        var protocol = new Protocol()
        {
            Data = MessagePackSerializer.Serialize(discoverResponse),
            Type = ProtocolType.DiscoverResponse
        };

        return protocol;
    }

    public static Protocol DeserializeProtocol(byte[] data)
    {
        return MessagePackSerializer.Deserialize<Protocol>(data);
    }

    public byte[] Serialize()
    {
        return MessagePackSerializer.Serialize(this);
    }
}