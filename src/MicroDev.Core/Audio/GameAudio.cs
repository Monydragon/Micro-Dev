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
            _musicLoop = CreateLoFiLoop();
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
            ? 0.13f
            : MathHelper.Lerp(0.27f, 0.20f, distress);

        var targetPitch = mode == BackgroundMusicMode.Menu
            ? -0.01f
            : MathHelper.Lerp(0.015f, -0.10f, distress);

        if (mode == BackgroundMusicMode.Workspace)
        {
            targetPitch += MathF.Sin(_musicWarpTime * 1.9f) * 0.010f * distress;
            targetVolume += MathF.Sin(_musicWarpTime * 2.7f) * 0.014f * distress;
        }

        var mixedTargetVolume = Math.Clamp(targetVolume * GetMusicMix(), 0f, 0.50f);
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

    private static SoundEffect CreateLoFiLoop()
    {
        const int sampleRate = 22050;
        const float beatsPerMinute = 80f;
        const float beatsPerLoop = 40f;
        const float chordTailBeats = 2.2f;
        const float bassTailBeats = 1.2f;
        const float leadTailBeats = 0.9f;
        const float phraseBeats = 8f;
        const float edgeFadeSeconds = 0.6f;
        var secondsPerBeat = 60f / beatsPerMinute;
        var durationSeconds = beatsPerLoop * secondsPerBeat;
        var sampleCount = Math.Max(1, (int)(sampleRate * durationSeconds));
        var buffer = new byte[sampleCount * 4];
        var leftChannel = new float[sampleCount];
        var rightChannel = new float[sampleCount];

        var chordEvents = new List<(float startBeat, int[] notes, float gain)>();
        AppendChordEvents(chordEvents, 0f,
        [
            (0f, [60, 64, 67, 71, 74], 0.92f),
            (1.5f, [64, 67, 71, 74], 0.54f),
            (4f, [57, 60, 64, 67, 71], 0.90f),
            (5.5f, [60, 64, 67, 71], 0.52f),
        ]);
        AppendChordEvents(chordEvents, 8f,
        [
            (0f, [62, 65, 69, 72, 76], 0.96f),
            (1.5f, [65, 69, 72, 76], 0.56f),
            (4f, [55, 59, 62, 65, 69], 0.94f),
            (5.5f, [59, 62, 65, 69], 0.54f),
        ]);
        AppendChordEvents(chordEvents, 16f,
        [
            (0f, [60, 64, 67, 71, 74], 0.88f),
            (1.5f, [64, 67, 71, 74], 0.50f),
            (4f, [57, 60, 64, 67, 71], 0.86f),
            (5.5f, [60, 64, 67, 71], 0.50f),
        ]);
        AppendChordEvents(chordEvents, 24f,
        [
            (0f, [62, 65, 69, 72, 76], 0.94f),
            (1.5f, [65, 69, 72, 76], 0.54f),
            (4f, [55, 59, 62, 65, 69], 0.92f),
            (5.5f, [59, 62, 65, 69, 72], 0.52f),
        ]);
        AppendChordEvents(chordEvents, 32f,
        [
            (0f, [60, 64, 67, 71, 74], 0.90f),
            (1.5f, [64, 67, 71, 74], 0.50f),
            (4f, [55, 59, 62, 65, 69], 0.98f),
            (5.5f, [59, 62, 65, 69, 72], 0.60f),
        ]);

        var bassEvents = new List<(float startBeat, int midi, float gain)>();
        AppendBassEvents(bassEvents, 0f,
        [
            (0f, 36, 0.98f),
            (1f, 43, 0.72f),
            (2f, 36, 0.86f),
            (3f, 43, 0.68f),
            (4f, 33, 0.98f),
            (5f, 40, 0.74f),
            (6f, 33, 0.86f),
            (7f, 40, 0.66f),
        ]);
        AppendBassEvents(bassEvents, 8f,
        [
            (0f, 38, 0.98f),
            (1f, 45, 0.74f),
            (2f, 38, 0.86f),
            (3f, 45, 0.68f),
            (4f, 31, 0.98f),
            (5f, 38, 0.76f),
            (6f, 43, 0.86f),
        ]);
        AppendBassEvents(bassEvents, 16f,
        [
            (0f, 36, 0.94f),
            (1f, 43, 0.70f),
            (2f, 36, 0.82f),
            (3f, 43, 0.66f),
            (4f, 33, 0.94f),
            (5f, 40, 0.72f),
            (6f, 33, 0.84f),
            (7f, 40, 0.64f),
        ]);
        AppendBassEvents(bassEvents, 24f,
        [
            (0f, 38, 0.96f),
            (1f, 45, 0.72f),
            (2f, 38, 0.84f),
            (3f, 45, 0.66f),
            (4f, 31, 0.96f),
            (5f, 38, 0.74f),
            (6f, 43, 0.84f),
        ]);
        AppendBassEvents(bassEvents, 32f,
        [
            (0f, 36, 0.96f),
            (1f, 43, 0.72f),
            (2f, 36, 0.84f),
            (3f, 43, 0.66f),
            (4f, 31, 0.98f),
            (5f, 38, 0.76f),
            (6.5f, 47, 0.82f),
        ]);

        var leadEvents = new List<(float startBeat, int midi, float gain)>();
        AppendLeadEvents(leadEvents, 0f,
        [
            (0.75f, 76, 0.50f),
            (2.25f, 79, 0.46f),
            (4.75f, 76, 0.48f),
            (6.25f, 72, 0.44f),
        ]);
        AppendLeadEvents(leadEvents, 8f,
        [
            (0.75f, 77, 0.52f),
            (2.25f, 81, 0.48f),
            (4.75f, 74, 0.46f),
            (6.25f, 71, 0.40f),
        ]);
        AppendLeadEvents(leadEvents, 16f,
        [
            (0.75f, 76, 0.46f),
            (2.25f, 79, 0.42f),
            (4.75f, 81, 0.40f),
            (6.25f, 76, 0.36f),
        ]);
        AppendLeadEvents(leadEvents, 24f,
        [
            (0.75f, 77, 0.50f),
            (2.25f, 81, 0.46f),
            (4.75f, 79, 0.42f),
            (6.25f, 74, 0.38f),
        ]);
        AppendLeadEvents(leadEvents, 32f,
        [
            (0.75f, 76, 0.46f),
            (2.25f, 79, 0.42f),
            (4.75f, 74, 0.40f),
            (6.25f, 71, 0.36f),
        ]);

        var kickPattern = new[] { 0f, 2f, 4f, 6.5f };
        var openHatPattern = new[] { 3.5f, 7f };

        var peak = 0f;

        for (var sampleIndex = 0; sampleIndex < sampleCount; sampleIndex++)
        {
            var time = sampleIndex / (float)sampleRate;
            var beat = time / secondsPerBeat;
            var phraseBeat = beat % phraseBeats;
            var phraseIndex = Math.Min((int)(beat / phraseBeats), 4);
            var musicLeft = 0f;
            var musicRight = 0f;
            var drumsLeft = 0f;
            var drumsRight = 0f;

            foreach (var (startBeat, notes, gain) in chordEvents)
            {
                var ageBeats = beat - startBeat;
                if (ageBeats < 0f || ageBeats > chordTailBeats)
                {
                    continue;
                }

                var ageSeconds = ageBeats * secondsPerBeat;
                var envelope = BuildPluckEnvelope(ageSeconds, 0.020f, 1.55f);
                var pulse = 0.96f + (0.04f * MathF.Sin((time * 0.85f) + (startBeat * 0.21f)));

                for (var noteIndex = 0; noteIndex < notes.Length; noteIndex++)
                {
                    var note = notes[noteIndex];
                    var pan = (-0.45f + (0.90f * noteIndex / Math.Max(1, notes.Length - 1))) * 0.75f;
                    var detune = 1f + (((noteIndex & 1) == 0 ? -1f : 1f) * 0.0045f);
                    var tone = RhodesVoice(MidiToFrequency(note), ageSeconds, detune, noteIndex * 0.17f);
                    var layer = tone * envelope * gain * 0.155f * pulse;
                    AddPan(ref musicLeft, ref musicRight, layer, pan);
                }
            }

            foreach (var (startBeat, midi, gain) in bassEvents)
            {
                var ageBeats = beat - startBeat;
                if (ageBeats < 0f || ageBeats > bassTailBeats)
                {
                    continue;
                }

                var ageSeconds = ageBeats * secondsPerBeat;
                var envelope = BuildPluckEnvelope(ageSeconds, 0.010f, 3.3f);
                var frequency = MidiToFrequency(midi);
                var bassTone =
                    (MathF.Sin(2f * MathF.PI * frequency * ageSeconds) * 0.82f) +
                    (MathF.Sin(2f * MathF.PI * frequency * 2f * ageSeconds) * 0.18f) +
                    (MathF.Sin(2f * MathF.PI * frequency * 0.5f * ageSeconds) * 0.07f);
                var bassSample = SoftClip(bassTone * 0.78f) * envelope * gain * 0.24f;
                AddPan(ref musicLeft, ref musicRight, bassSample, -0.04f);
            }

            foreach (var (startBeat, midi, gain) in leadEvents)
            {
                var ageBeats = beat - startBeat;
                if (ageBeats < 0f || ageBeats > leadTailBeats)
                {
                    continue;
                }

                var ageSeconds = ageBeats * secondsPerBeat;
                var envelope = BuildPluckEnvelope(ageSeconds, 0.008f, 5.1f);
                var frequency = MidiToFrequency(midi) * (1f + (0.0025f * MathF.Sin(time * 1.7f)));
                var leadTone =
                    (MathF.Sin(2f * MathF.PI * frequency * ageSeconds) * 0.80f) +
                    (MathF.Sin(2f * MathF.PI * frequency * 2f * ageSeconds) * 0.11f) +
                    (MathF.Sin(2f * MathF.PI * frequency * 3f * ageSeconds + 0.24f) * 0.07f);
                var leadSample = SoftClip(leadTone * 0.88f) * envelope * gain * 0.18f;
                AddPan(ref musicLeft, ref musicRight, leadSample, 0.18f);
            }

            var pump = 1f;
            foreach (var startBeat in kickPattern)
            {
                var ageBeats = GetRepeatedPatternAge(phraseBeat, startBeat, phraseBeats);
                if (ageBeats > 0.45f)
                {
                    continue;
                }

                var release = ageBeats / 0.45f;
                pump = MathF.Min(pump, 0.84f + (0.16f * release));
            }

            musicLeft *= pump;
            musicRight *= pump;

            for (var kickIndex = 0; kickIndex < kickPattern.Length; kickIndex++)
            {
                var ageBeats = GetRepeatedPatternAge(phraseBeat, kickPattern[kickIndex], phraseBeats);
                if (ageBeats > 0.42f)
                {
                    continue;
                }

                var ageSeconds = ageBeats * secondsPerBeat;
                var envelope = BuildPluckEnvelope(ageSeconds, 0.003f, 10.5f);
                var sweep = MathHelper.Lerp(90f, 46f, Math.Clamp(ageSeconds / 0.18f, 0f, 1f));
                var thump =
                    (MathF.Sin(2f * MathF.PI * sweep * ageSeconds) * 0.90f) +
                    (MathF.Sin(2f * MathF.PI * sweep * 0.5f * ageSeconds) * 0.15f);
                var kickSample = SoftClip(thump * 0.92f) * envelope * 0.44f;
                AddPan(ref drumsLeft, ref drumsRight, kickSample, 0f);
            }

            var shiftedSnareBeat = beat - 1f;
            if (shiftedSnareBeat >= 0f)
            {
                var ageBeats = shiftedSnareBeat % 2f;
                if (ageBeats <= 0.33f)
                {
                    var snareHitIndex = (int)MathF.Floor(shiftedSnareBeat / 2f);
                    var ageSeconds = ageBeats * secondsPerBeat;
                    var envelope = BuildPluckEnvelope(ageSeconds, 0.002f, 15f);
                    var seed = (sampleIndex * 197) + (snareHitIndex * 61);
                    var noise =
                        (SignedNoise(seed) * 0.82f) +
                        (SignedNoise(seed + 11) * 0.38f);
                    var snap = MathF.Sin(2f * MathF.PI * 180f * ageSeconds) * 0.22f;
                    var snareSample = SoftClip((noise + snap) * 0.44f) * envelope * 0.26f;
                    AddPan(ref drumsLeft, ref drumsRight, snareSample, 0.02f);
                }
            }

            var shiftedHatBeat = beat - 0.5f;
            if (shiftedHatBeat >= 0f)
            {
                var ageBeats = shiftedHatBeat % 0.5f;
                if (ageBeats <= 0.12f)
                {
                    var hatHitIndex = (int)MathF.Floor(shiftedHatBeat / 0.5f);
                    var ageSeconds = ageBeats * secondsPerBeat;
                    var envelope = BuildPluckEnvelope(ageSeconds, 0.001f, 36f);
                    var seed = (sampleIndex * 239) + (hatHitIndex * 23);
                    var noise = SignedNoise(seed) - (SignedNoise(seed + 5) * 0.52f);
                    var accent = (hatHitIndex & 1) == 0 ? 1f : 0.72f;
                    var hatSample = SoftClip(noise * 0.20f) * envelope * 0.11f * accent;
                    AddPan(ref drumsLeft, ref drumsRight, hatSample, 0.32f);
                }
            }

            for (var openHatIndex = 0; openHatIndex < openHatPattern.Length; openHatIndex++)
            {
                var ageBeats = GetRepeatedPatternAge(phraseBeat, openHatPattern[openHatIndex], phraseBeats);
                if (ageBeats > 0.28f)
                {
                    continue;
                }

                var openHatHitIndex = (phraseIndex * openHatPattern.Length) + openHatIndex;
                var ageSeconds = ageBeats * secondsPerBeat;
                var envelope = BuildPluckEnvelope(ageSeconds, 0.001f, 20f);
                var seed = (sampleIndex * 281) + (openHatHitIndex * 71);
                var noise = SignedNoise(seed) - (SignedNoise(seed + 7) * 0.46f);
                var openHatSample = SoftClip(noise * 0.18f) * envelope * 0.12f;
                AddPan(ref drumsLeft, ref drumsRight, openHatSample, 0.36f);
            }

            var vinyl = SignedNoise((sampleIndex * 37) + 13) * 0.0030f;
            vinyl += SignedNoise((sampleIndex * 71) + 19) * 0.0018f;

            var leftSample = (musicLeft * (0.975f + (0.025f * MathF.Sin(time * 0.48f)))) + drumsLeft + (vinyl * 0.92f);
            var rightSample = (musicRight * (0.975f + (0.025f * MathF.Sin((time * 0.52f) + 0.7f)))) + drumsRight + (vinyl * 1.08f);

            var edgeFade = MathF.Min(1f, MathF.Min(time / edgeFadeSeconds, (durationSeconds - time) / edgeFadeSeconds));
            leftSample = SoftClip(leftSample * edgeFade);
            rightSample = SoftClip(rightSample * edgeFade);

            leftChannel[sampleIndex] = leftSample;
            rightChannel[sampleIndex] = rightSample;
            peak = Math.Max(peak, Math.Max(MathF.Abs(leftSample), MathF.Abs(rightSample)));
        }

        var normalization = peak > 0f ? 0.82f / peak : 1f;

        for (var sampleIndex = 0; sampleIndex < sampleCount; sampleIndex++)
        {
            var leftPcm = (short)(Math.Clamp(leftChannel[sampleIndex] * normalization, -0.95f, 0.95f) * short.MaxValue);
            var rightPcm = (short)(Math.Clamp(rightChannel[sampleIndex] * normalization, -0.95f, 0.95f) * short.MaxValue);
            var bufferIndex = sampleIndex * 4;

            buffer[bufferIndex] = (byte)(leftPcm & 0xff);
            buffer[bufferIndex + 1] = (byte)((leftPcm >> 8) & 0xff);
            buffer[bufferIndex + 2] = (byte)(rightPcm & 0xff);
            buffer[bufferIndex + 3] = (byte)((rightPcm >> 8) & 0xff);
        }

        return new SoundEffect(buffer, sampleRate, AudioChannels.Stereo);
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

    private static void AppendChordEvents(List<(float startBeat, int[] notes, float gain)> events, float offset, params (float beat, int[] notes, float gain)[] pattern)
    {
        foreach (var (beat, notes, gain) in pattern)
        {
            events.Add((offset + beat, notes, gain));
        }
    }

    private static void AppendBassEvents(List<(float startBeat, int midi, float gain)> events, float offset, params (float beat, int midi, float gain)[] pattern)
    {
        foreach (var (beat, midi, gain) in pattern)
        {
            events.Add((offset + beat, midi, gain));
        }
    }

    private static void AppendLeadEvents(List<(float startBeat, int midi, float gain)> events, float offset, params (float beat, int midi, float gain)[] pattern)
    {
        foreach (var (beat, midi, gain) in pattern)
        {
            events.Add((offset + beat, midi, gain));
        }
    }

    private static float GetRepeatedPatternAge(float position, float start, float patternLength)
    {
        var age = position - start;
        if (age < 0f)
        {
            age += patternLength;
        }

        return age;
    }

    private static float BuildPluckEnvelope(float time, float attackSeconds, float decayRate)
    {
        if (time < 0f)
        {
            return 0f;
        }

        var attack = attackSeconds <= 0f ? 1f : Math.Clamp(time / attackSeconds, 0f, 1f);
        return attack * MathF.Exp(-time * decayRate);
    }

    private static float MidiToFrequency(int midiNote)
    {
        return 440f * MathF.Pow(2f, (midiNote - 69) / 12f);
    }

    private static float RhodesVoice(float frequency, float time, float detune, float phaseOffset)
    {
        var baseFrequency = frequency * detune;
        var fundamental = MathF.Sin(2f * MathF.PI * baseFrequency * time);
        var overtone = MathF.Sin((2f * MathF.PI * baseFrequency * 2f * time) + phaseOffset) * 0.36f;
        var bell = MathF.Sin((2f * MathF.PI * baseFrequency * 3f * time) + (phaseOffset * 1.6f)) * 0.14f;
        return SoftClip((fundamental * 0.86f) + overtone + bell);
    }

    private static float SoftClip(float sample)
    {
        return MathF.Tanh(sample);
    }

    private static void AddPan(ref float left, ref float right, float sample, float pan)
    {
        var clampedPan = Math.Clamp(pan, -1f, 1f);
        var angle = (clampedPan + 1f) * 0.25f * MathF.PI;
        left += sample * MathF.Cos(angle);
        right += sample * MathF.Sin(angle);
    }

    private static float SignedNoise(int seed)
    {
        var value = MathF.Sin((seed * 12.9898f) + 78.233f) * 43758.5453f;
        return ((value - MathF.Floor(value)) * 2f) - 1f;
    }
}
