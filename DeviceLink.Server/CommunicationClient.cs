using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using DeviceLink.Shared;
using NAudio.Wave;

namespace DeviceLink.Server
{
    public class CommunicationClient : IDisposable
    {
        private readonly UdpClient _udpClient;
        private IPEndPoint? _clientEndpoint = null;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private Task? _listeningTask = null;
        private readonly WasapiLoopbackCapture _wasapiLoopbackCapture;
        public CommunicationClient()
        {
            _udpClient = new UdpClient(CommunicationConstants.DefaultUdpPort);
            _wasapiLoopbackCapture = new WasapiLoopbackCapture();
            _wasapiLoopbackCapture.DataAvailable += WasapiLoopbackCaptureOnDataAvailable;
        }

        private void WasapiLoopbackCaptureOnDataAvailable(object? sender, WaveInEventArgs e)
        {
            if (_clientEndpoint != null)
            {
                _udpClient.Send(e.Buffer, e.BytesRecorded, _clientEndpoint);
            }
        }

        public void StartListener()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;
            _listeningTask = new Task(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        var result = await _udpClient.ReceiveAsync(token);
                        _clientEndpoint = result.RemoteEndPoint;
                    }
                    catch{}
                }
            });
            _wasapiLoopbackCapture.StartRecording();
            _listeningTask.Start();
        }

        public void StopListener()
        {
            _cancellationTokenSource.Cancel();
            _listeningTask?.Dispose();
            _wasapiLoopbackCapture.StopRecording();
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _listeningTask?.Dispose();
            _udpClient.Close();
            _wasapiLoopbackCapture.Dispose();
        }
    }
}
