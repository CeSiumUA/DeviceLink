using MessagePack;

namespace DeviceLink.Shared.Protocol.Messages;

[MessagePackObject]
public class DiscoverResponse
{
    [Key(0)]
    public Guid ResponseGuid { get; set; }

    [Key(1)]
    public Guid TargetGuid { get; set; }

    [Key(2)]
    public string HostName { get; set; } = Environment.MachineName;
}
