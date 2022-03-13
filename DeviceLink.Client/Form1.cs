using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using DeviceLink.Shared;
using DeviceLink.Shared.Messages;
using NAudio.Wave;

namespace DeviceLink.Client
{
    public partial class Form1 : Form
    {
        private readonly UdpClient _udpClient;
        private Task? _listeningTask = null;
        private readonly WaveOut _waveOut;
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly BufferedWaveProvider _waveProvider;
        public Form1()
        {
            InitializeComponent();
            _udpClient = new UdpClient();
            _waveOut = new WaveOut();
            _waveProvider = new BufferedWaveProvider(WaveFormat.CreateIeeeFloatWaveFormat(48000, 2));
            _waveOut.Init(_waveProvider);
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            _cts = new CancellationTokenSource();
            var ipEndPoint = ipAddressInputBox.Text;
            var endPoint = new IPEndPoint(IPAddress.Parse(ipEndPoint), CommunicationConstants.DefaultUdpPort);
            _udpClient.Connect(endPoint);
            _udpClient.Send(new byte[] {(byte) CommandType.Handshake});
            var token = _cts.Token;
            _listeningTask = new Task(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    var result = await _udpClient.ReceiveAsync(token);
                    var buffer = result.Buffer;
                    _waveProvider.AddSamples(buffer, 0, buffer.Length);
                }
            });
            _waveOut.Play();
            _listeningTask.Start();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _udpClient.Close();
            _cts.Cancel();
            base.OnClosing(e);
        }
    }
}