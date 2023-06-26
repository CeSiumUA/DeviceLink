using NAudio.Wave;

namespace DeviceLink.Shared;

public class Player : IDisposable
{
    private readonly BufferedWaveProvider _waveProvider;
    private readonly DirectSoundOut _waveOutEvent = new();

    public Player()
    {
        _waveProvider = new BufferedWaveProvider(WaveFormat.CreateIeeeFloatWaveFormat(48000, 2));
        _waveOutEvent.Init(_waveProvider);
    }

    public void Dispose()
    {
        _waveOutEvent.Stop();
        _waveOutEvent.Dispose();
    }

    public void StartPlayer()
    {
        _waveOutEvent.Play();
    }

    public void Play(byte[] bytes)
    {
        _waveProvider.AddSamples(bytes, 0, bytes.Length);
    }
}
