using Microsoft.Xna.Framework.Audio;

namespace MicroDev.Core.Audio;

public sealed class GameAudio : IDisposable
{
    private readonly SoundEffect? _buttonClick;
    private readonly SoundEffect? _writeKey;
    private readonly SoundEffect? _alert;
    private readonly SoundEffect? _success;
    private readonly SoundEffect? _failure;

    public GameAudio()
    {
        try
        {
            _buttonClick = CreateTone(780f, 620f, 0.05f, 0.24f);
            _writeKey = CreateTone(1220f, 860f, 0.035f, 0.20f);
            _alert = CreateTone(420f, 280f, 0.16f, 0.28f);
            _success = CreateSuccessTone();
            _failure = CreateFailureTone();
        }
        catch
        {
            _buttonClick = null;
            _writeKey = null;
            _alert = null;
            _success = null;
            _failure = null;
        }
    }

    public bool Enabled { get; set; } = true;

    public void PlayButtonClick()
    {
        if (!Enabled)
        {
            return;
        }

        Play(_buttonClick);
    }

    public void PlayWriteKey()
    {
        if (!Enabled)
        {
            return;
        }

        Play(_writeKey);
    }

    public void PlayAlert()
    {
        if (!Enabled)
        {
            return;
        }

        Play(_alert);
    }

    public void PlaySuccess()
    {
        if (!Enabled)
        {
            return;
        }

        Play(_success);
    }

    public void PlayFailure()
    {
        if (!Enabled)
        {
            return;
        }

        Play(_failure);
    }

    public void Dispose()
    {
        _buttonClick?.Dispose();
        _writeKey?.Dispose();
        _alert?.Dispose();
        _success?.Dispose();
        _failure?.Dispose();
    }

    private static void Play(SoundEffect? effect)
    {
        try
        {
            effect?.Play();
        }
        catch
        {
        }
    }

    private static SoundEffect CreateTone(float startFrequency, float endFrequency, float durationSeconds, float amplitude)
    {
        const int sampleRate = 44100;
        var sampleCount = Math.Max(1, (int)(sampleRate * durationSeconds));
        var buffer = new byte[sampleCount * 2];

        for (var sampleIndex = 0; sampleIndex < sampleCount; sampleIndex++)
        {
            var progress = sampleIndex / (float)Math.Max(1, sampleCount - 1);
            var frequency = Interpolate(startFrequency, endFrequency, progress);
            var envelope = 1f - progress;
            var time = sampleIndex / (float)sampleRate;
            var sampleValue = MathF.Sin(2f * MathF.PI * frequency * time) * amplitude * envelope;
            var pcm = (short)(sampleValue * short.MaxValue);

            buffer[sampleIndex * 2] = (byte)(pcm & 0xff);
            buffer[(sampleIndex * 2) + 1] = (byte)((pcm >> 8) & 0xff);
        }

        return new SoundEffect(buffer, sampleRate, AudioChannels.Mono);
    }

    private static SoundEffect CreateSuccessTone()
    {
        return CreateCompositeTone(
            [
                (520f, 740f, 0.08f, 0.24f),
                (740f, 980f, 0.10f, 0.20f),
            ]);
    }

    private static SoundEffect CreateFailureTone()
    {
        return CreateCompositeTone(
            [
                (380f, 250f, 0.09f, 0.24f),
                (250f, 180f, 0.12f, 0.20f),
            ]);
    }

    private static SoundEffect CreateCompositeTone((float start, float end, float duration, float amplitude)[] segments)
    {
        const int sampleRate = 44100;
        var totalSamples = segments.Sum(segment => (int)(sampleRate * segment.duration));
        var buffer = new byte[Math.Max(1, totalSamples) * 2];
        var offset = 0;

        foreach (var (start, end, duration, amplitude) in segments)
        {
            var sampleCount = Math.Max(1, (int)(sampleRate * duration));
            for (var sampleIndex = 0; sampleIndex < sampleCount; sampleIndex++)
            {
                var progress = sampleIndex / (float)Math.Max(1, sampleCount - 1);
                var frequency = Interpolate(start, end, progress);
                var envelope = 1f - progress;
                var time = sampleIndex / (float)sampleRate;
                var sampleValue = MathF.Sin(2f * MathF.PI * frequency * time) * amplitude * envelope;
                var pcm = (short)(sampleValue * short.MaxValue);

                buffer[offset++] = (byte)(pcm & 0xff);
                buffer[offset++] = (byte)((pcm >> 8) & 0xff);
            }
        }

        return new SoundEffect(buffer, sampleRate, AudioChannels.Mono);
    }

    private static float Interpolate(float start, float end, float progress)
    {
        return start + ((end - start) * progress);
    }
}
