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

    private readonly Action<IPEndPoint, byte[]> _protocolReceiveCallback;
    private readonly Action<IPEndPoint, byte[]> _audioReceiveCallback;

    private readonly Guid _clientId = Guid.NewGuid();

    const int NW_PROTOCOl_PORT = 9343;

    const int NW_AUDIO_PORT = 9344;

    public Network(Action<IPEndPoint, byte[]> protocolReceiveCallback, Action<IPEndPoint, byte[]> audioReceiveCallback)
    {
        _udpClient = new UdpClient(new IPEndPoint(IPAddress.Any, NW_PROTOCOl_PORT));
        PickNetworkInterface(_udpClient.Client);

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
        _discoveryTask = new Task(async () =>
        {
            while(!discoveryToken.IsCancellationRequested)
            {
                await _udpClient.SendAsync(discoverPackageBytes, discoverPackageBytes.Length, new IPEndPoint(IPAddress.Broadcast, NW_PROTOCOl_PORT));
                await Task.Delay(10000);
            }
        });
        _discoveryTask.Start();
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
                _protocolReceiveCallback(result.RemoteEndPoint, result.Buffer);
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

    private void PickNetworkInterface(Socket socket)
    {
        var nics = NetworkInterface.GetAllNetworkInterfaces();
        foreach (var nic in nics)
        {
            var niProps = nic.GetIPProperties();
            if (!niProps.MulticastAddresses.Any())
            {
                continue;
            }
            if (!nic.SupportsMulticast)
            {
                continue;
            }
            var niPropsV4 = niProps.GetIPv4Properties();
            if(niPropsV4 == null)
            {
                continue;
            }

            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastInterface, (int)IPAddress.HostToNetworkOrder(niPropsV4.Index));
        }
    }
}
