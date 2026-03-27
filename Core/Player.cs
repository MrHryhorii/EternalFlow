using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace EternalFlow.Core;

public class Player
{
    public Vector2 Position;
    public float VelocityY = 0f; // ДОДАНО: Щоб інші класи знали куди ми летимо

    private readonly float baseRadius = 25f;
    private float time = 0f;
    private readonly float pulseSpeed = 4.0f; // Швидкість пульсу винесена в змінну класу

    // Клас для частинок шлейфу (Відлуння)
    private class TrailEcho
    {
        public Vector2 Position;
        public float Life;   // Життя від 1.0 (нове) до 0.0 (зникло)
        public float Radius; // Радіус, який буде трохи рости
    }

    private readonly List<TrailEcho> trail = [];
    private float echoSpawnTimer = 0f;

    public Player(int screenHeight)
    {
        Position = new Vector2(100, screenHeight / 2f);
    }

    public void Update(float deltaTime)
    {
        time += deltaTime;

        // 1. ОНОВЛЕННЯ ТАЙМЕРА ШЛЕЙФУ (Прив'язка до пульсу)
        echoSpawnTimer -= deltaTime;
        if (echoSpawnTimer <= 0)
        {
            // Додаємо нове кільце-відлуння на поточній позиції гравця
            trail.Add(new TrailEcho
            {
                Position = this.Position,
                Life = 1f,
                Radius = baseRadius
            });

            // Налаштовуємо таймер так, щоб він скидав відлуння рівно 2 рази за один цикл пульсу
            // Один повний цикл синусоїди = 2 * PI. Ділимо на швидкість пульсу.
            echoSpawnTimer = MathF.PI * 2f / (pulseSpeed * 2f);
        }

        // 2. ФІЗИКА ШЛЕЙФУ
        // Проходимо список з кінця, щоб можна було безпечно видаляти старі частинки
        for (int i = trail.Count - 1; i >= 0; i--)
        {
            trail[i].Life -= deltaTime * 0.6f;        // Відлуння живе приблизно 1.6 секунди
            trail[i].Position.X -= 300f * deltaTime;  // Відлітає назад, створюючи ілюзію руху вперед
            trail[i].Radius += deltaTime * 12f;       // Кільце злегка розширюється, розчиняючись у просторі

            if (trail[i].Life <= 0)
            {
                trail.RemoveAt(i);
            }
        }
    }

    public void Draw()
    {
        float pulseAmp = 0.05f;
        float pulsation = MathF.Sin(time * pulseSpeed) * pulseAmp;
        float currentMembraneRadius = baseRadius * (1f + pulsation);

        Color coreColor = ColorConverter.OklchToColor(0.98f, 0.02f, 60f);
        Color membraneColor = Color.White;

        // --- МАЛЮЄМО ШЛЕЙФ (ВІДЛУННЯ) ---
        // Малюємо його ДО самого гравця, щоб шлейф був під ним
        foreach (var echo in trail)
        {
            // Прозорість залежить від того, скільки "життя" залишилося
            // Максимальна прозорість дуже слабка (50 із 255), щоб не заважати огляду
            byte alpha = (byte)(50 * echo.Life);

            Color echoMembrane = membraneColor;
            echoMembrane.A = alpha;

            Color echoCore = coreColor;
            echoCore.A = (byte)(20 * echo.Life); // Ядро відлуння ще слабше

            // Малюємо кільце-мембрану відлуння (воно плавно тоншає зі зменшенням Life)
            Raylib.DrawPolyLinesEx(echo.Position, 40, echo.Radius, 0f, 2f * echo.Life, echoMembrane);

            // Ледь помітний залишок світла всередині
            Raylib.DrawCircleV(echo.Position, echo.Radius * 0.4f, echoCore);
        }

        // --- МАЛЮЄМО САМОГО ГРАВЦЯ ---
        coreColor.A = 200;
        Raylib.DrawCircleV(Position, baseRadius * 0.4f, coreColor);

        coreColor.A = 80;
        Raylib.DrawCircleV(Position, baseRadius * 0.7f, coreColor);

        coreColor.A = 30;
        Raylib.DrawCircleV(Position, baseRadius * 1.1f, coreColor);

        membraneColor.A = 150;
        Raylib.DrawPolyLinesEx(Position, 40, currentMembraneRadius, 0f, 2.0f, membraneColor);
    }
}