using Raylib_cs;

namespace EternalFlow.Core;

/// <summary>
/// Manages background music playback, seamless crossfading between tracks, 
/// and real-time audio analysis. Includes a custom DSP Low-Pass Filter 
/// to create an "underwater" auditory effect when player stress is critical.
/// </summary>
public class AudioManager
{
    private readonly List<string> playlist = [];
    private int currentTrackIndex = 0;

    private Music currentTrack;
    private Music nextTrack;

    private bool isCrossfading = false;
    private float crossfadeTimer = 0f;
    private const float CROSSFADE_DURATION = 3f;

    private bool isAudioReady = false;

    public float MasterVolume { get; set; } = 0.5f;

    // Exposes the current volume spike (beat) to other systems like background shapes
    public static float RealtimeAmplitude { get; private set; } = 0f;

    // Static variables required for the unmanaged audio callback to read filter states
    private static float currentMuffleFactor = 0f;

    // Retains the previous audio sample state to prevent popping/clicking when the filter is active
    private static float filterStateL = 0f;
    private static float filterStateR = 0f;

    public AudioManager()
    {
        Raylib.InitAudioDevice();
        LoadPlaylist();

        if (playlist.Count > 0)
        {
            currentTrackIndex = Random.Shared.Next(playlist.Count);
            LoadAndPlayCurrentTrack();
        }
    }

    private void LoadPlaylist()
    {
        if (Directory.Exists("sound"))
        {
            playlist.AddRange(Directory.GetFiles("sound", "*.mp3"));
        }
    }

    private static bool IsMusicValid(Music music)
    {
        return music.FrameCount > 0;
    }

    private void LoadAndPlayCurrentTrack()
    {
        string trackPath = playlist[currentTrackIndex];

        if (!File.Exists(trackPath))
        {
            isAudioReady = false;
            return;
        }

        currentTrack = Raylib.LoadMusicStream(trackPath);

        if (IsMusicValid(currentTrack))
        {
            Raylib.SetMusicVolume(currentTrack, MasterVolume);
            Raylib.PlayMusicStream(currentTrack);

            // Attach a custom processor to intercept raw audio bytes before they reach the speakers
            unsafe
            {
                Raylib.AttachAudioStreamProcessor(currentTrack.Stream, &AudioProcessor);
            }

            isAudioReady = true;
        }
        else
        {
            isAudioReady = false;
        }
    }

    public void Update(float stress, float deltaTime)
    {
        if (playlist.Count == 0 || !isAudioReady) return;

        // Streams must be constantly updated to keep the audio buffer filled
        Raylib.UpdateMusicStream(currentTrack);
        if (isCrossfading)
        {
            Raylib.UpdateMusicStream(nextTrack);
        }

        // Increase the track playback speed slightly as stress rises to build tension
        float currentPitch = 1f + (stress * 0.3f);
        Raylib.SetMusicPitch(currentTrack, currentPitch);
        if (isCrossfading) Raylib.SetMusicPitch(nextTrack, currentPitch);

        // Calculate the intensity of the underwater filter based on critical stress
        if (stress > 0.75f)
        {
            currentMuffleFactor = (stress - 0.75f) / 0.25f;
        }
        else
        {
            currentMuffleFactor = 0f;
        }

        float timePlayed = Raylib.GetMusicTimePlayed(currentTrack);
        float timeLength = Raylib.GetMusicTimeLength(currentTrack);

        if (!isCrossfading)
        {
            Raylib.SetMusicVolume(currentTrack, MasterVolume);
        }

        // Initiate a crossfade when nearing the end of the current track
        if (!isCrossfading && timePlayed >= timeLength - CROSSFADE_DURATION)
        {
            isCrossfading = true;
            crossfadeTimer = 0f;

            currentTrackIndex = (currentTrackIndex + 1) % playlist.Count;
            string nextTrackPath = playlist[currentTrackIndex];

            nextTrack = Raylib.LoadMusicStream(nextTrackPath);

            if (IsMusicValid(nextTrack))
            {
                Raylib.PlayMusicStream(nextTrack);
                Raylib.SetMusicVolume(nextTrack, 0f);

                unsafe
                {
                    Raylib.AttachAudioStreamProcessor(nextTrack.Stream, &AudioProcessor);
                }
            }
            else
            {
                isCrossfading = false;
            }
        }

        // Process the volume mixing during a crossfade transition
        if (isCrossfading)
        {
            crossfadeTimer += deltaTime * currentPitch;
            float t = Math.Clamp(crossfadeTimer / CROSSFADE_DURATION, 0f, 1f);

            Raylib.SetMusicVolume(currentTrack, (1f - t) * MasterVolume);
            Raylib.SetMusicVolume(nextTrack, t * MasterVolume);

            if (t >= 1f)
            {
                isCrossfading = false;

                unsafe
                {
                    Raylib.DetachAudioStreamProcessor(currentTrack.Stream, &AudioProcessor);
                }

                Raylib.StopMusicStream(currentTrack);
                Raylib.UnloadMusicStream(currentTrack);

                currentTrack = nextTrack;
                Raylib.SetMusicVolume(currentTrack, MasterVolume);
            }
        }
    }

    /// <summary>
    /// Low-level callback executing directly on Raylib's audio thread.
    /// Analyzes raw amplitude for visual beats and applies the DSP low-pass filter to muffle audio.
    /// </summary>
    [System.Runtime.InteropServices.UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    private static unsafe void AudioProcessor(void* bufferData, uint frames)
    {
        float* samples = (float*)bufferData;
        int sampleCount = (int)frames * 2;
        float maxAmplitude = 0f;

        // Determines how much high-frequency sound is allowed to pass through the filter
        float alpha = 1.0f - (currentMuffleFactor * 0.95f);

        // Process each stereo pair
        for (int i = 0; i < sampleCount; i += 2)
        {
            float rawL = samples[i];
            float rawR = samples[i + 1];

            // Evaluate the clean audio signal for visual beat detection
            float absL = Math.Abs(rawL);
            float absR = Math.Abs(rawR);
            if (absL > maxAmplitude) maxAmplitude = absL;
            if (absR > maxAmplitude) maxAmplitude = absR;

            // Apply the low-pass filter to physically alter the audio bytes in memory
            if (currentMuffleFactor > 0.01f)
            {
                filterStateL += alpha * (rawL - filterStateL);
                filterStateR += alpha * (rawR - filterStateR);

                samples[i] = filterStateL;
                samples[i + 1] = filterStateR;
            }
            else
            {
                // Keep the filter state synchronized with the raw audio to prevent popping when activated
                filterStateL = rawL;
                filterStateR = rawR;
            }
        }

        // Smooth out the detected amplitude to prevent jittery visual reactions
        RealtimeAmplitude = (RealtimeAmplitude * 0.8f) + (maxAmplitude * 0.2f);
    }

    public void Unload()
    {
        if (playlist.Count == 0) return;

        unsafe
        {
            if (IsMusicValid(currentTrack)) Raylib.DetachAudioStreamProcessor(currentTrack.Stream, &AudioProcessor);
            if (isCrossfading && IsMusicValid(nextTrack)) Raylib.DetachAudioStreamProcessor(nextTrack.Stream, &AudioProcessor);
        }

        if (IsMusicValid(currentTrack)) Raylib.UnloadMusicStream(currentTrack);
        if (isCrossfading && IsMusicValid(nextTrack)) Raylib.UnloadMusicStream(nextTrack);

        Raylib.CloseAudioDevice();
    }
}