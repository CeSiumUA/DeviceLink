using System.Net;

namespace DeviceLink.CLI;

public record Peer(Guid Id, IPEndPoint IpEndPoint, string HostName)
{
    public DateTimeOffset LastUpdateTime { get; set; } = DateTimeOffset.UtcNow;
}
