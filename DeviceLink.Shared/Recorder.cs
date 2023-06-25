using NAudio.Wave;

namespace DeviceLink.Shared;
public class Recorder : IDisposable
{
    private readonly WasapiLoopbackCapture _capture;
    private readonly Action<byte[], int> _send_callback;

    public Recorder(Action<byte[], int> send_callback)
    {
        _capture = new WasapiLoopbackCapture();
        _send_callback = send_callback;
        _capture.DataAvailable += DataAvailableCallback;
    }

    public void StartRecorder()
    {
        _capture.StartRecording();
    }

    private void DataAvailableCallback(object? sender, WaveInEventArgs e)
    {
        _send_callback(e.Buffer, e.BytesRecorded);
    }

    public void Dispose()
    {
        _capture.StopRecording();
        _capture.Dispose();
    }
}
