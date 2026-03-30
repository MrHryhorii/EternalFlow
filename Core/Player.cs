using Raylib_cs;
using System.Numerics;

namespace EternalFlow.Core;

public class Player(int screenHeight)
{
    public Vector2 Position = new(100, screenHeight / 2f);
    public float VelocityY = 0f;

    private readonly float baseRadius = 25f;
    private float time = 0f;

    // --- АУДІОРЕАКТИВНІ ЗМІННІ ---
    private float previousAmplitude = 0f;
    private float beatCooldown = 0f;
    private float fallbackTimer = 0f; // Таймер на випадок, якщо музика дуже тиха

    private class TrailEcho
    {
        public Vector2 Position;
        public float Life;
        public float Radius;
        public float InitialAmplitude; // Запам'ятовуємо силу біту, щоб робити гучні біти більшими
    }

    private readonly List<TrailEcho> trail = [];

    public void Update(float deltaTime)
    {
        time += deltaTime;

        // ЧИТАЄМО МУЗИКУ
        float currentAmp = AudioManager.RealtimeAmplitude;
        if (beatCooldown > 0) beatCooldown -= deltaTime;
        fallbackTimer -= deltaTime;

        // ДЕТЕКТОР БІТУ (СКАЧОК АМПЛІТУДИ)
        // Шукаємо різкий стрибок амплітуди. Якщо його немає довго - спрацьовує fallbackTimer
        bool isBeat = currentAmp > 0.3f && currentAmp > previousAmplitude + 0.05f && beatCooldown <= 0f;

        if (isBeat || fallbackTimer <= 0)
        {
            trail.Add(new TrailEcho
            {
                Position = this.Position,
                Life = 1f,
                Radius = baseRadius,
                InitialAmplitude = isBeat ? currentAmp : 0.2f // Гучний біт дасть яскравіше відлуння
            });

            // Якщо це був справжній біт музики, ставимо кулдаун, щоб не спамити кільцями
            if (isBeat) beatCooldown = 0.2f;

            // Скидаємо страхувальний таймер (щоб кільця пускалися хоча б раз на 0.8 сек у тиші)
            fallbackTimer = 0.8f;
        }

        previousAmplitude = currentAmp;

        // ФІЗИКА ШЛЕЙФУ
        for (int i = trail.Count - 1; i >= 0; i--)
        {
            trail[i].Life -= deltaTime * 0.6f;
            trail[i].Position.X -= 300f * deltaTime;

            // Швидкість розширення кільця тепер залежить від того, наскільки сильним був біт!
            float expansionSpeed = 10f + (trail[i].InitialAmplitude * 30f);
            trail[i].Radius += deltaTime * expansionSpeed;

            if (trail[i].Life <= 0)
            {
                trail.RemoveAt(i);
            }
        }
    }

    public void Draw()
    {
        float currentAmp = AudioManager.RealtimeAmplitude;

        // --- ГІБРИДНИЙ ПУЛЬС ---
        // Легке математичне дихання (завжди є)
        float baseBreathing = MathF.Sin(time * 3f) * 0.05f;
        // Аудіореактивний пульс (різко реагує на бас/гучність)
        float audioPulse = currentAmp * 0.4f; // До 40% збільшення розміру від музики!

        float currentMembraneRadius = baseRadius * (1f + baseBreathing + audioPulse);

        Color coreColor = ColorConverter.OklchToColor(0.98f, 0.02f, 60f);
        Color membraneColor = Color.White;

        // --- МАЛЮЄМО ШЛЕЙФ (ВІДЛУННЯ) ---
        foreach (var echo in trail)
        {
            // Робимо відлуння від сильних бітів трохи яскравішим
            float alphaMultiplier = 0.5f + (echo.InitialAmplitude * 0.5f);
            byte alpha = (byte)(50 * echo.Life * alphaMultiplier);

            Color echoMembrane = membraneColor;
            echoMembrane.A = alpha;

            Color echoCore = coreColor;
            echoCore.A = (byte)(20 * echo.Life * alphaMultiplier);

            Raylib.DrawPolyLinesEx(echo.Position, 40, echo.Radius, 0f, 2f * echo.Life, echoMembrane);
            Raylib.DrawCircleV(echo.Position, echo.Radius * 0.4f, echoCore);
        }

        // --- МАЛЮЄМО САМОГО ГРАВЦЯ ---
        coreColor.A = 200;
        Raylib.DrawCircleV(Position, baseRadius * 0.4f, coreColor);

        coreColor.A = 80;
        Raylib.DrawCircleV(Position, baseRadius * 0.7f, coreColor);

        // Ця аура теж може трохи "стрибати" від музики
        coreColor.A = 30;
        Raylib.DrawCircleV(Position, baseRadius * (1.1f + audioPulse * 0.5f), coreColor);

        membraneColor.A = 150;
        Raylib.DrawPolyLinesEx(Position, 40, currentMembraneRadius, 0f, 2.0f, membraneColor);
    }
}