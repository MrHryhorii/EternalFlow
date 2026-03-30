using Raylib_cs;

namespace EternalFlow.Core;

public class AudioManager
{
    // Список треків тепер динамічний, щоб на десктопі ми могли читати папку
    private readonly List<string> playlist = [];

    // Жорстко заданий список ТІЛЬКИ для браузера (WASM)
    private readonly string[] webFallbackPlaylist = [
        "sound/track1.mp3",
        "sound/track2.mp3",
        "sound/track3.mp3",
        "sound/track4.mp3",
        "sound/track5.mp3",
        "sound/track6.mp3",
        "sound/track7.mp3"
    ];

    private int currentTrackIndex = 0;
    private Music currentTrack;
    private Music nextTrack;

    private bool isCrossfading = false;
    private float crossfadeTimer = 0f;
    private const float CROSSFADE_DURATION = 3f; // Скільки секунд треки накладаються

    // Прапорець безпеки: чи успішно завантажився поточний трек
    private bool isAudioReady = false;

    // --- НАША НОВА ЗМІННА ГУЧНОСТІ ---
    public float MasterVolume { get; set; } = 0.5f; // Гучність від 0.0 до 1.0 (50% за замовчуванням)

    // --- СПРАВЖНЯ АМПЛІТУДА МУЗИКИ (від 0.0 до ~1.0) ---
    public static float RealtimeAmplitude { get; private set; } = 0f;

    public AudioManager()
    {
        Raylib.InitAudioDevice();

        // ЗАВАНТАЖУЄМО ПЛЕЙЛИСТ ЗАЛЕЖНО ВІД ПЛАТФОРМИ
        LoadPlaylist();

        // ЯКЩО МУЗИКА ЗНАЙДЕНА — ЗАПУСКАЄМО
        // Завантажуємо і запускаємо перший трек
        if (playlist.Count > 0)
        {
            currentTrackIndex = Random.Shared.Next(playlist.Count); // Починаємо з випадкового
            LoadAndPlayCurrentTrack();
        }
    }

    private void LoadPlaylist()
    {
        // Перевіряємо, чи запущено в браузері (WebAssembly)
        if (OperatingSystem.IsBrowser())
        {
            // Для вебу просто беремо наш заготовлений масив
            playlist.AddRange(webFallbackPlaylist);
        }
        else
        {
            // Для Десктопу: скануємо папку автоматично!
            if (Directory.Exists("sound"))
            {
                // Знаходимо всі mp3 файли у папці
                playlist.AddRange(Directory.GetFiles("sound", "*.mp3"));
            }
        }
    }

    // --- МЕТОД ПЕРЕВІРКИ ---
    private static bool IsMusicValid(Music music)
    {
        // Якщо кількість кадрів більше нуля, значить файл існує і Raylib зміг його прочитати
        return music.FrameCount > 0;
    }

    private void LoadAndPlayCurrentTrack()
    {
        string trackPath = playlist[currentTrackIndex];

        // Додатковий захист для десктопу: якщо файлу раптом нема, не намагаємося його вантажити
        if (!OperatingSystem.IsBrowser() && !File.Exists(trackPath))
        {
            isAudioReady = false;
            return;
        }

        currentTrack = Raylib.LoadMusicStream(trackPath);

        // ГОЛОВНИЙ ФОЛБЕК: Перевіряємо, чи Raylib зміг "проковтнути" цей файл
        if (IsMusicValid(currentTrack))
        {
            // Одразу ставимо правильну гучність при старті
            Raylib.SetMusicVolume(currentTrack, MasterVolume);
            Raylib.PlayMusicStream(currentTrack);

            // --- ЧІПЛЯЄМО АНАЛІЗАТОР ДО ТРЕКУ ---
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
        // Якщо плейлист порожній АБО трек не зміг завантажитися — нічого не робимо (гра не крашиться!)
        if (playlist.Count == 0 || !isAudioReady) return;

        // Raylib вимагає постійно оновлювати потік (стримінг)
        Raylib.UpdateMusicStream(currentTrack);
        if (isCrossfading)
        {
            Raylib.UpdateMusicStream(nextTrack);
        }

        // --- ВПЛИВ СТРЕСУ НА МУЗИКУ ---
        // Якщо стрес 0% -> pitch 1.0 (нормально)
        // Якщо стрес 100% -> pitch 1.3 (швидше і тривожніше)
        float currentPitch = 1f + (stress * 0.3f);
        Raylib.SetMusicPitch(currentTrack, currentPitch);
        if (isCrossfading) Raylib.SetMusicPitch(nextTrack, currentPitch);

        // --- ЛОГІКА ПЕРЕХОДУ (CROSSFADE) ---
        float timePlayed = Raylib.GetMusicTimePlayed(currentTrack);
        float timeLength = Raylib.GetMusicTimeLength(currentTrack);

        // ЯКЩО НЕМАЄ ПЕРЕХОДУ - просто тримаємо поточну гучність (якщо гравець її змінив у налаштуваннях)
        if (!isCrossfading)
        {
            Raylib.SetMusicVolume(currentTrack, MasterVolume);
        }

        // Якщо до кінця треку залишилося менше CROSSFADE_DURATION секунд і ми ще не переходимо
        if (!isCrossfading && timePlayed >= timeLength - CROSSFADE_DURATION)
        {
            isCrossfading = true;
            crossfadeTimer = 0f;

            // Беремо наступний трек по колу
            currentTrackIndex = (currentTrackIndex + 1) % playlist.Count;
            string nextTrackPath = playlist[currentTrackIndex];

            nextTrack = Raylib.LoadMusicStream(nextTrackPath);

            // Якщо наступний трек битий/не існує, пропускаємо мікшування
            if (IsMusicValid(nextTrack))
            {
                Raylib.PlayMusicStream(nextTrack);
                Raylib.SetMusicVolume(nextTrack, 0f); // Починаємо з тиші

                // --- ЧІПЛЯЄМО АНАЛІЗАТОР ДО НАСТУПНОГО ТРЕКУ ---
                unsafe
                {
                    Raylib.AttachAudioStreamProcessor(nextTrack.Stream, &AudioProcessor);
                }
            }
            else
            {
                isCrossfading = false; // Скасовуємо перехід, якщо помилка
            }
        }

        // Сам процес переходу (мікшер)
        if (isCrossfading)
        {
            // Оскільки pitch прискорює час, додаємо його до deltaTime
            crossfadeTimer += deltaTime * currentPitch;
            float t = Math.Clamp(crossfadeTimer / CROSSFADE_DURATION, 0f, 1f);

            // --- МНОЖИМО ПЕРЕХІД НА MASTER VOLUME ---
            Raylib.SetMusicVolume(currentTrack, (1f - t) * MasterVolume); // Старий затихає
            Raylib.SetMusicVolume(nextTrack, t * MasterVolume);         // Новий стає гучнішим

            if (t >= 1f)
            {
                isCrossfading = false;

                // --- ВІДЧІПЛЯЄМО АНАЛІЗАТОР ВІД СТАРОГО ТРЕКУ ---
                unsafe
                {
                    Raylib.DetachAudioStreamProcessor(currentTrack.Stream, &AudioProcessor);
                }

                Raylib.StopMusicStream(currentTrack);
                Raylib.UnloadMusicStream(currentTrack);

                currentTrack = nextTrack;
                // Встановлюємо фінальну гучність після завершення переходу
                Raylib.SetMusicVolume(currentTrack, MasterVolume);
            }
        }
    }

    // Цей метод підключається напряму до аудіорушія
    [System.Runtime.InteropServices.UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    private static unsafe void AudioProcessor(void* bufferData, uint frames)
    {
        // Стерео = 2 канали
        int sampleCount = (int)frames * 2;

        // --- Створюємо безпечний Span прямо поверх сирої пам'яті! ---
        ReadOnlySpan<float> samples = new(bufferData, sampleCount);

        float maxAmplitude = 0f;

        // Тепер ми можемо безпечно бігти по колекції (без індексів і вказівників)
        foreach (float sample in samples)
        {
            float currentSample = Math.Abs(sample);
            if (currentSample > maxAmplitude)
            {
                maxAmplitude = currentSample;
            }
        }

        // Згладжуємо значення
        RealtimeAmplitude = (RealtimeAmplitude * 0.8f) + (maxAmplitude * 0.2f);
    }

    // Обов'язково треба звільняти пам'ять при виході
    public void Unload()
    {
        if (playlist.Count == 0) return;

        // --- ВІДЧІПЛЯЄМО АНАЛІЗАТОРИ ПЕРЕД ВИХОДОМ ---
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