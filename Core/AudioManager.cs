using Raylib_cs;

namespace EternalFlow.Core;

/// <summary>
/// Відповідає за фонову музику, плавні переходи (crossfade) та генерацію даних для аудіореактивного візуалу.
/// Поточна версія оптимізована виключно для Desktop. Від ідеї портування на WebAssembly (WASM) 
/// відмовилися через жорсткі обмеження браузерів на багатопотоковий доступ до пам'яті (краші при AttachAudioStreamProcessor) 
/// та проблеми з кешуванням.
/// </summary>
public class AudioManager
{
    // Динамічний список треків. На десктопі ми маємо повний доступ до файлової системи, 
    // тому просто скануємо папку, замість того щоб жорстко прописувати імена файлів у коді.
    private readonly List<string> playlist = [];

    private int currentTrackIndex = 0;

    // Використовуємо Music (потокове читання з диска по шматочках), а не Sound (завантаження всього файлу в оперативку).
    // Це економить ресурси комп'ютера і є стандартом для довгих фонових треків.
    private Music currentTrack;
    private Music nextTrack;

    // Параметри для плавного переходу (мікшування) між треками
    private bool isCrossfading = false;
    private float crossfadeTimer = 0f;
    private const float CROSSFADE_DURATION = 3f; // Скільки секунд два треки грають одночасно під час переходу

    // Прапорець безпеки. Дозволяє уникнути крашу гри, якщо папка зі звуками порожня або файл пошкоджено.
    private bool isAudioReady = false;

    // Глобальні налаштування звуку
    public float MasterVolume { get; set; } = 0.5f;

    // Значення поточної гучності (амплітуди) музики в реальному часі.
    // Використовується іншими класами (гравцем, фоном) для пульсації в такт.
    public static float RealtimeAmplitude { get; private set; } = 0f;

    public AudioManager()
    {
        // Ініціалізуємо звукову карту перед будь-якими операціями з аудіо
        Raylib.InitAudioDevice();

        LoadPlaylist();

        // Якщо в папці знайшлися треки, беремо випадковий і запускаємо
        if (playlist.Count > 0)
        {
            currentTrackIndex = Random.Shared.Next(playlist.Count);
            LoadAndPlayCurrentTrack();
        }
    }

    /// <summary>
    /// Автоматично знаходить всі .mp3 файли у папці sound і додає їх у плейлист.
    /// </summary>
    private void LoadPlaylist()
    {
        if (Directory.Exists("sound"))
        {
            playlist.AddRange(Directory.GetFiles("sound", "*.mp3"));
        }
    }

    /// <summary>
    /// Перевіряє, чи Raylib зміг коректно прочитати аудіофайл.
    /// </summary>
    private static bool IsMusicValid(Music music)
    {
        return music.FrameCount > 0;
    }

    /// <summary>
    /// Завантажує поточний трек у пам'ять, встановлює гучність і підключає аналізатор ритму.
    /// </summary>
    private void LoadAndPlayCurrentTrack()
    {
        string trackPath = playlist[currentTrackIndex];

        // Додатковий захист: якщо файл раптово видалили під час гри
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

            // Підключаємо наш метод аналізу до аудіопотоку Raylib.
            // Блок unsafe необхідний, оскільки ми передаємо вказівник на функцію C# у двигун, написаний на C.
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

    /// <summary>
    /// Головний цикл оновлення аудіо. Підтримує відтворення, реагує на стрес гравця та керує переходами.
    /// </summary>
    public void Update(float stress, float deltaTime)
    {
        if (playlist.Count == 0 || !isAudioReady) return;

        // Потокова музика вимагає постійного оновлення буферів кожен кадр
        Raylib.UpdateMusicStream(currentTrack);
        if (isCrossfading)
        {
            Raylib.UpdateMusicStream(nextTrack);
        }

        // Вплив ігрового стресу на музику: чим вищий стрес, тим швидше грає трек (до +30% швидкості)
        float currentPitch = 1f + (stress * 0.3f);
        Raylib.SetMusicPitch(currentTrack, currentPitch);
        if (isCrossfading) Raylib.SetMusicPitch(nextTrack, currentPitch);

        float timePlayed = Raylib.GetMusicTimePlayed(currentTrack);
        float timeLength = Raylib.GetMusicTimeLength(currentTrack);

        // Якщо переходу немає, просто підтримуємо майстер-гучність (якщо гравець змінив її в налаштуваннях)
        if (!isCrossfading)
        {
            Raylib.SetMusicVolume(currentTrack, MasterVolume);
        }

        // Тригер початку переходу: коли до кінця треку залишається час, рівний CROSSFADE_DURATION
        if (!isCrossfading && timePlayed >= timeLength - CROSSFADE_DURATION)
        {
            isCrossfading = true;
            crossfadeTimer = 0f;

            // Беремо наступний трек по колу
            currentTrackIndex = (currentTrackIndex + 1) % playlist.Count;
            string nextTrackPath = playlist[currentTrackIndex];

            nextTrack = Raylib.LoadMusicStream(nextTrackPath);

            if (IsMusicValid(nextTrack))
            {
                Raylib.PlayMusicStream(nextTrack);
                Raylib.SetMusicVolume(nextTrack, 0f); // Наступний трек починає грати з нульовою гучністю

                unsafe
                {
                    Raylib.AttachAudioStreamProcessor(nextTrack.Stream, &AudioProcessor);
                }
            }
            else
            {
                // Якщо наступний трек битий, скасовуємо перехід
                isCrossfading = false;
            }
        }

        // Процес змішування (Crossfade) двох треків
        if (isCrossfading)
        {
            // pitch впливає на швидкість часу у грі, тому множимо deltaTime на поточну швидкість треку
            crossfadeTimer += deltaTime * currentPitch;
            float t = Math.Clamp(crossfadeTimer / CROSSFADE_DURATION, 0f, 1f);

            // Старий трек поступово затихає, новий стає гучнішим
            Raylib.SetMusicVolume(currentTrack, (1f - t) * MasterVolume);
            Raylib.SetMusicVolume(nextTrack, t * MasterVolume);

            // Завершення переходу
            if (t >= 1f)
            {
                isCrossfading = false;

                // Відключаємо аналізатор від старого треку, щоб уникнути витоку пам'яті
                unsafe
                {
                    Raylib.DetachAudioStreamProcessor(currentTrack.Stream, &AudioProcessor);
                }

                Raylib.StopMusicStream(currentTrack);
                Raylib.UnloadMusicStream(currentTrack);

                // Робимо новий трек основним
                currentTrack = nextTrack;
                Raylib.SetMusicVolume(currentTrack, MasterVolume);
            }
        }
    }

    /// <summary>
    /// Низькорівневий колбек, який Raylib викликає автоматично під час обробки аудіобуфера.
    /// Дозволяє читати сирі байти звуку для визначення гучності (амплітуди) в конкретний момент часу.
    /// UnmanagedCallersOnly гарантує, що C-двигун зможе безпечно викликати цю C#-функцію.
    /// </summary>
    [System.Runtime.InteropServices.UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    private static unsafe void AudioProcessor(void* bufferData, uint frames)
    {
        // Музика стерео (2 канали), тому семплів вдвічі більше за кадри
        int sampleCount = (int)frames * 2;

        // Створюємо безпечну обгортку над сирою ділянкою пам'яті
        ReadOnlySpan<float> samples = new(bufferData, sampleCount);

        float maxAmplitude = 0f;

        // Шукаємо найгучніший звук у цьому мікро-фрагменті
        foreach (float sample in samples)
        {
            float currentSample = Math.Abs(sample);
            if (currentSample > maxAmplitude)
            {
                maxAmplitude = currentSample;
            }
        }

        // Згладжуємо результат (інтерполяція), щоб візуальні елементи не смикалися надто різко
        RealtimeAmplitude = (RealtimeAmplitude * 0.8f) + (maxAmplitude * 0.2f);
    }

    /// <summary>
    /// Звільняє всі ресурси звукової карти при закритті гри або сцени.
    /// </summary>
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