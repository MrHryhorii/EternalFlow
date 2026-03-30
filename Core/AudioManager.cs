using Raylib_cs;

namespace EternalFlow.Core;

public class AudioManager
{
    // ТУТ ВПИШИ ТОЧНІ НАЗВИ СВОЇХ ФАЙЛІВ!
    // Важливо: для вебу краще називати файли англійською без пробілів (напр. "ambient1.mp3")
    private readonly string[] playlist = [
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

    public AudioManager()
    {
        Raylib.InitAudioDevice();

        // Завантажуємо і запускаємо перший трек
        if (playlist.Length > 0)
        {
            currentTrackIndex = Random.Shared.Next(playlist.Length); // Починаємо з випадкового
            currentTrack = Raylib.LoadMusicStream(playlist[currentTrackIndex]);
            Raylib.PlayMusicStream(currentTrack);
        }
    }

    public void Update(float stress, float deltaTime)
    {
        if (playlist.Length == 0) return;

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

        // Якщо до кінця треку залишилося менше CROSSFADE_DURATION секунд і ми ще не переходимо
        if (!isCrossfading && timePlayed >= timeLength - CROSSFADE_DURATION)
        {
            isCrossfading = true;
            crossfadeTimer = 0f;

            // Беремо наступний трек по колу
            currentTrackIndex = (currentTrackIndex + 1) % playlist.Length;
            nextTrack = Raylib.LoadMusicStream(playlist[currentTrackIndex]);
            Raylib.PlayMusicStream(nextTrack);
            Raylib.SetMusicVolume(nextTrack, 0f); // Починаємо з тиші
        }

        // Сам процес переходу (мікшер)
        if (isCrossfading)
        {
            // Оскільки pitch прискорює час, додаємо його до deltaTime
            crossfadeTimer += deltaTime * currentPitch;
            float t = Math.Clamp(crossfadeTimer / CROSSFADE_DURATION, 0f, 1f);

            Raylib.SetMusicVolume(currentTrack, 1f - t); // Старий затихає
            Raylib.SetMusicVolume(nextTrack, t);         // Новий стає гучнішим

            if (t >= 1f)
            {
                isCrossfading = false;
                Raylib.StopMusicStream(currentTrack);
                Raylib.UnloadMusicStream(currentTrack);

                currentTrack = nextTrack;
                Raylib.SetMusicVolume(currentTrack, 1f); // Переконуємось, що гучність 100%
            }
        }
    }

    // Обов'язково треба звільняти пам'ять при виході
    public void Unload()
    {
        if (playlist.Length == 0) return;

        Raylib.UnloadMusicStream(currentTrack);
        if (isCrossfading) Raylib.UnloadMusicStream(nextTrack);
        Raylib.CloseAudioDevice();
    }
}