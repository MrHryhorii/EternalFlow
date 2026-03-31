using Raylib_cs;

namespace EternalFlow.Core;

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

    public static float RealtimeAmplitude { get; private set; } = 0f;

    // --- НОВІ ЗМІННІ ДЛЯ DSP ФІЛЬТРА ---
    // Статична змінна, щоб наш unmanaged колбек міг її читати
    private static float currentMuffleFactor = 0f;

    // Стан фільтра для лівого та правого каналів (щоб уникнути клацань звуку)
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

        Raylib.UpdateMusicStream(currentTrack);
        if (isCrossfading)
        {
            Raylib.UpdateMusicStream(nextTrack);
        }

        // Вплив ігрового стресу на музику
        float currentPitch = 1f + (stress * 0.3f);
        Raylib.SetMusicPitch(currentTrack, currentPitch);
        if (isCrossfading) Raylib.SetMusicPitch(nextTrack, currentPitch);

        // --- РОЗРАХУНОК ЕФЕКТУ "ПІД ВОДОЮ" ---
        if (stress > 0.75f)
        {
            // Плавно збільшуємо приглушення від 0.0 (на 75% стресу) до 1.0 (на 100%)
            currentMuffleFactor = (stress - 0.75f) / 0.25f;
        }
        else
        {
            // Якщо стрес впав, повертаємо чистий звук
            currentMuffleFactor = 0f;
        }

        float timePlayed = Raylib.GetMusicTimePlayed(currentTrack);
        float timeLength = Raylib.GetMusicTimeLength(currentTrack);

        if (!isCrossfading)
        {
            Raylib.SetMusicVolume(currentTrack, MasterVolume);
        }

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

    [System.Runtime.InteropServices.UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    private static unsafe void AudioProcessor(void* bufferData, uint frames)
    {
        // Перетворюємо void* на вказівник на масив float
        float* samples = (float*)bufferData;
        int sampleCount = (int)frames * 2; // Лівий і правий канали
        float maxAmplitude = 0f;

        // Коефіцієнт фільтра (альфа). 
        // 1.0 = звук проходить без змін. 0.05 = сильне приглушення високих частот.
        float alpha = 1.0f - (currentMuffleFactor * 0.95f);

        // Обробляємо кожен семпл попарно (Лівий, Правий, Лівий, Правий...)
        for (int i = 0; i < sampleCount; i += 2)
        {
            float rawL = samples[i];
            float rawR = samples[i + 1];

            // Обчислюємо амплітуду з ЧИСТОГО звуку
            float absL = Math.Abs(rawL);
            float absR = Math.Abs(rawR);
            if (absL > maxAmplitude) maxAmplitude = absL;
            if (absR > maxAmplitude) maxAmplitude = absR;

            // Застосовуємо Low-Pass Filter, якщо стрес високий
            if (currentMuffleFactor > 0.01f)
            {
                filterStateL += alpha * (rawL - filterStateL);
                filterStateR += alpha * (rawR - filterStateR);

                // ЗАПИСУЄМО змінений звук назад у буфер пам'яті звукової карти!
                samples[i] = filterStateL;
                samples[i + 1] = filterStateR;
            }
            else
            {
                // Якщо фільтр вимкнено, оновлюємо стан, щоб не було "клацання" при його увімкненні
                filterStateL = rawL;
                filterStateR = rawR;
            }
        }

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