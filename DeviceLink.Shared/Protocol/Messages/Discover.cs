using MessagePack;

namespace DeviceLink.Shared.Protocol.Messages;

[MessagePackObject]
public class Discover
{
    [Key(0)]
    public Guid Id { get; set; }
}
