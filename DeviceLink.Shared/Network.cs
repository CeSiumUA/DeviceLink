using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace DeviceLink.Shared;

public class Network : IDisposable
{
    private readonly UdpClient _udpClient;
    private readonly UdpClient _audioUdpClient;

    private Task? _protocolListeningTask;

    private Task? _audioListeningTask;

    private Task? _discoveryTask;

    private CancellationTokenSource? _listeningTokenSource;
    private CancellationTokenSource? _discoveryTokenSource;

    private readonly Action<Network, IPEndPoint, byte[]> _protocolReceiveCallback;
    private readonly Action<IPEndPoint, byte[]> _audioReceiveCallback;

    private readonly Guid _clientId = Guid.NewGuid();

    private readonly IPAddress _assignedIpAddress;

    private int _scanDelay = 3000;

    const int NW_PROTOCOl_PORT = 9343;

    const int NW_AUDIO_PORT = 9344;
    
    public Guid Id => _clientId;

    public Network(Action<Network, IPEndPoint, byte[]> protocolReceiveCallback, Action<IPEndPoint, byte[]> audioReceiveCallback)
    {
        _assignedIpAddress = GetLocalIpAddress();

        _udpClient = new UdpClient(new IPEndPoint(_assignedIpAddress, NW_PROTOCOl_PORT));

        _audioUdpClient = new UdpClient(new IPEndPoint(IPAddress.Any, NW_AUDIO_PORT));

        _protocolReceiveCallback = protocolReceiveCallback;
        _audioReceiveCallback = audioReceiveCallback;
    }

    public void StartDiscover()
    {
        _discoveryTokenSource?.Cancel();
        _discoveryTokenSource = new CancellationTokenSource();

        var discoveryToken = _discoveryTokenSource.Token;

        _udpClient.EnableBroadcast = true;
        var discoverPackageBytes = Protocol.Protocol.ConstructDiscover(_clientId).Serialize();
        _scanDelay = 100;
        _discoveryTask = new Task(async () =>
        {
            while(!discoveryToken.IsCancellationRequested)
            {
                await _udpClient.SendAsync(discoverPackageBytes, discoverPackageBytes.Length, new IPEndPoint(IPAddress.Broadcast, NW_PROTOCOl_PORT));
                await Task.Delay(_scanDelay);
            }
        });
        _discoveryTask.Start();
    }

    public void SendData(byte[] data, IPEndPoint ipep)
    {
        _udpClient.Send(data, data.Length, ipep);
    }

    public void SetPassiveScan()
    {
        if(_discoveryTask?.Status == TaskStatus.Running)
        {
            _scanDelay = 3000;
        }
    }

    public void StartListener()
    {
        _listeningTokenSource?.Cancel();
        _listeningTokenSource = new CancellationTokenSource();
        var listenToken = _listeningTokenSource.Token;

        _protocolListeningTask = new Task(async () =>
        {
            while(!listenToken.IsCancellationRequested)
            {
                var result = await _udpClient.ReceiveAsync(listenToken);
                if(result.RemoteEndPoint.Address.ToString() != _assignedIpAddress.ToString())
                {
                    _protocolReceiveCallback(this, result.RemoteEndPoint, result.Buffer);
                }
            }
        });

        _audioListeningTask = new Task(async () =>
        {
            while (!listenToken.IsCancellationRequested)
            {
                var result = await _audioUdpClient.ReceiveAsync(listenToken);
                _audioReceiveCallback(result.RemoteEndPoint, result.Buffer);
            }
        });

        _protocolListeningTask.Start();
        _audioListeningTask.Start();
    }

    public void Dispose()
    {
        _listeningTokenSource?.Cancel();
        _listeningTokenSource?.Dispose();
        _discoveryTokenSource?.Cancel();
        _discoveryTokenSource?.Dispose();

        _udpClient.Close();
        _udpClient.Dispose();

        _audioUdpClient.Close();
        _audioUdpClient.Dispose();

        _audioListeningTask?.Dispose();
        _protocolListeningTask?.Dispose();
        _discoveryTask?.Dispose();
    }

    private IPAddress GetLocalIpAddress()
    {
        foreach (var netI in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (netI.NetworkInterfaceType != NetworkInterfaceType.Wireless80211 &&
                (netI.NetworkInterfaceType != NetworkInterfaceType.Ethernet ||
                    netI.OperationalStatus != OperationalStatus.Up)) continue;
            foreach (var uniIpAddrInfo in netI.GetIPProperties().UnicastAddresses.Where(x => netI.GetIPProperties().GatewayAddresses.Count > 0))
            {

                if (uniIpAddrInfo.Address.AddressFamily == AddressFamily.InterNetwork)
                    return uniIpAddrInfo.Address;
            }
        }
        return IPAddress.Any;
    }
}
