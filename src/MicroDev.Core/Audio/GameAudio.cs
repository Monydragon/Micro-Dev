using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace MicroDev.Core.Audio;

public enum BackgroundMusicMode
{
    None = 0,
    Menu,
    Workspace,
}

public sealed class GameAudio : IDisposable
{
    private readonly SoundEffect? _buttonClick;
    private readonly SoundEffect? _writeKey;
    private readonly SoundEffect? _alert;
    private readonly SoundEffect? _success;
    private readonly SoundEffect? _failure;
    private readonly SoundEffect? _musicLoop;
    private readonly SoundEffectInstance? _musicLoopInstance;
    private float _currentMusicVolume;
    private float _currentMusicPitch;
    private float _musicWarpTime;
    private float _typingMomentum;

    public GameAudio()
    {
        try
        {
            _buttonClick = CreateTone(780f, 620f, 0.05f, 0.24f);
            _writeKey = CreateTone(1220f, 860f, 0.035f, 0.20f);
            _alert = CreateTone(420f, 280f, 0.16f, 0.28f);
            _success = CreateSuccessTone();
            _failure = CreateFailureTone();
            _musicLoop = CreateAmbientLoop();
            _musicLoopInstance = _musicLoop.CreateInstance();
            _musicLoopInstance.IsLooped = true;
            _musicLoopInstance.Volume = 0f;
        }
        catch
        {
            _buttonClick = null;
            _writeKey = null;
            _alert = null;
            _success = null;
            _failure = null;
            _musicLoop = null;
            _musicLoopInstance = null;
        }
    }

    public bool Enabled { get; set; } = true;

    public bool MusicEnabled { get; set; } = true;

    public float MasterVolume { get; set; } = 1f;

    public float SoundEffectsVolume { get; set; } = 1f;

    public float MusicVolume { get; set; } = 1f;

    public void PlayButtonClick()
    {
        if (!Enabled)
        {
            return;
        }

        Play(_buttonClick, 0.22f * GetEffectsMix(), 0.02f, 0f);
    }

    public void PlayWriteKey()
    {
        if (!Enabled)
        {
            return;
        }

        _typingMomentum = MathF.Min(1f, _typingMomentum + 0.24f);
        var volume = MathHelper.Lerp(0.16f, 0.28f, _typingMomentum);
        var pitch = MathHelper.Lerp(-0.06f, 0.10f, _typingMomentum);
        Play(_writeKey, volume * GetEffectsMix(), pitch, 0f);
    }

    public void PlayAlert()
    {
        if (!Enabled)
        {
            return;
        }

        Play(_alert, 0.24f * GetEffectsMix(), -0.04f, 0f);
    }

    public void PlaySuccess()
    {
        if (!Enabled)
        {
            return;
        }

        Play(_success, 0.26f * GetEffectsMix(), 0.06f, 0f);
    }

    public void PlayFailure()
    {
        if (!Enabled)
        {
            return;
        }

        Play(_failure, 0.24f * GetEffectsMix(), -0.02f, 0f);
    }

    public void UpdateMusic(double elapsedSeconds, BackgroundMusicMode mode, float sanityRatio)
    {
        _typingMomentum = Math.Max(0f, _typingMomentum - ((float)elapsedSeconds * 1.15f));

        if (_musicLoopInstance is null)
        {
            return;
        }

        if (!MusicEnabled || mode == BackgroundMusicMode.None)
        {
            FadeOutMusic();
            return;
        }

        EnsureMusicPlaying();

        var clampedSanity = Math.Clamp(sanityRatio, 0f, 1f);
        var distress = 1f - clampedSanity;
        _musicWarpTime += (float)elapsedSeconds * MathHelper.Lerp(0.55f, 2.1f, distress);

        var targetVolume = mode == BackgroundMusicMode.Menu
            ? 0.08f
            : MathHelper.Lerp(0.16f, 0.09f, distress);

        var targetPitch = mode == BackgroundMusicMode.Menu
            ? -0.02f
            : MathHelper.Lerp(0.02f, -0.34f, distress);

        if (mode == BackgroundMusicMode.Workspace)
        {
            targetPitch += MathF.Sin(_musicWarpTime * 1.9f) * 0.018f * distress;
            targetVolume += MathF.Sin(_musicWarpTime * 2.7f) * 0.012f * distress;
        }

        var mixedTargetVolume = Math.Clamp(targetVolume * GetMusicMix(), 0f, 0.35f);
        _currentMusicVolume = MathHelper.Lerp(_currentMusicVolume, mixedTargetVolume, 0.08f);
        _currentMusicPitch = MathHelper.Lerp(_currentMusicPitch, Math.Clamp(targetPitch, -0.95f, 0.95f), 0.08f);

        try
        {
            _musicLoopInstance.Volume = _currentMusicVolume;
            _musicLoopInstance.Pitch = _currentMusicPitch;
        }
        catch
        {
        }
    }

    public void Dispose()
    {
        try
        {
            _musicLoopInstance?.Stop();
        }
        catch
        {
        }

        _musicLoopInstance?.Dispose();
        _buttonClick?.Dispose();
        _writeKey?.Dispose();
        _alert?.Dispose();
        _success?.Dispose();
        _failure?.Dispose();
        _musicLoop?.Dispose();
    }

    private void EnsureMusicPlaying()
    {
        if (_musicLoopInstance is null)
        {
            return;
        }

        try
        {
            if (_musicLoopInstance.State != SoundState.Playing)
            {
                _musicLoopInstance.Play();
            }
        }
        catch
        {
        }
    }

    private float GetEffectsMix()
    {
        return Math.Clamp(MasterVolume * SoundEffectsVolume, 0f, 1f);
    }

    private float GetMusicMix()
    {
        return Math.Clamp(MasterVolume * MusicVolume, 0f, 1f);
    }

    private void FadeOutMusic()
    {
        if (_musicLoopInstance is null)
        {
            return;
        }

        _currentMusicVolume = MathHelper.Lerp(_currentMusicVolume, 0f, 0.18f);

        try
        {
            _musicLoopInstance.Volume = _currentMusicVolume;

            if (_currentMusicVolume <= 0.01f &&
                _musicLoopInstance.State == SoundState.Playing)
            {
                _musicLoopInstance.Stop();
            }
        }
        catch
        {
        }
    }

    private static void Play(SoundEffect? effect, float volume, float pitch, float pan)
    {
        try
        {
            effect?.Play(Math.Clamp(volume, 0f, 1f), Math.Clamp(pitch, -1f, 1f), Math.Clamp(pan, -1f, 1f));
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

    private static SoundEffect CreateAmbientLoop()
    {
        const int sampleRate = 22050;
        const float durationSeconds = 4f;
        var sampleCount = Math.Max(1, (int)(sampleRate * durationSeconds));
        var buffer = new byte[sampleCount * 2];
        var roots = new[] { 174.61f, 196f, 220f, 196f };

        for (var sampleIndex = 0; sampleIndex < sampleCount; sampleIndex++)
        {
            var time = sampleIndex / (float)sampleRate;
            var segmentProgress = (time % 1f);
            var segmentIndex = Math.Min(roots.Length - 1, (int)MathF.Floor(time) % roots.Length);
            var root = roots[segmentIndex];

            var padEnvelope = 0.72f + (0.28f * MathF.Sin(MathF.PI * segmentProgress));
            var pad =
                (MathF.Sin(2f * MathF.PI * root * time) * 0.09f) +
                (MathF.Sin(2f * MathF.PI * root * 1.5f * time) * 0.046f) +
                (MathF.Sin(2f * MathF.PI * root * 2f * time) * 0.028f);

            var chord =
                (MathF.Sin(2f * MathF.PI * (root * 1.2599f) * time) * 0.030f) +
                (MathF.Sin(2f * MathF.PI * (root * 1.4983f) * time) * 0.022f);
            var bass = MathF.Sin(2f * MathF.PI * (root * 0.5f) * time) * 0.048f;

            var beatPhase = time % 0.5f;
            var beatProgress = beatPhase / 0.5f;
            var kickEnvelope = MathF.Exp(-7.5f * beatProgress);
            var kickFrequency = 58f + (18f * (1f - beatProgress));
            var kick = MathF.Sin(2f * MathF.PI * kickFrequency * time) * 0.16f * kickEnvelope;

            var shimmerPhase = (time + 0.125f) % 0.5f;
            var shimmerEnvelope = MathF.Exp(-18f * (shimmerPhase / 0.5f));
            var shimmer =
                MathF.Sin(2f * MathF.PI * (root * 3.2f) * time) * 0.015f * shimmerEnvelope;

            var hatPhase = time % 0.25f;
            var hatEnvelope = MathF.Exp(-24f * (hatPhase / 0.25f));
            var hatNoise = SignedNoise((sampleIndex * 131) + 17) * 0.018f * hatEnvelope;

            var snarePhase = (time + 0.25f) % 1f;
            var snareEnvelope = MathF.Exp(-15f * snarePhase);
            var snare = SignedNoise((sampleIndex * 197) + 41) * 0.05f * snareEnvelope;

            var vinyl = SignedNoise((sampleIndex * 37) + 13) * 0.004f;
            vinyl += SignedNoise((sampleIndex * 71) + 19) * 0.003f * (0.35f + (0.65f * segmentProgress));

            var loopProgress = sampleIndex / (float)Math.Max(1, sampleCount - 1);
            var edgeFade = MathF.Min(1f, MathF.Min(loopProgress * 14f, (1f - loopProgress) * 14f));
            var sampleValue = (((pad + chord) * padEnvelope) + bass + kick + shimmer + hatNoise + snare + vinyl) * edgeFade;
            sampleValue = Math.Clamp(sampleValue, -0.45f, 0.45f);
            var pcm = (short)(sampleValue * short.MaxValue);

            buffer[sampleIndex * 2] = (byte)(pcm & 0xff);
            buffer[(sampleIndex * 2) + 1] = (byte)((pcm >> 8) & 0xff);
        }

        return new SoundEffect(buffer, sampleRate, AudioChannels.Mono);
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

    private static float SignedNoise(int seed)
    {
        var value = MathF.Sin((seed * 12.9898f) + 78.233f) * 43758.5453f;
        return ((value - MathF.Floor(value)) * 2f) - 1f;
    }
}
