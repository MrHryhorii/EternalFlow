using Raylib_cs;
using System;
using System.Numerics;

namespace EternalFlow.Core;

public class PathGenerator
{
    private float time = 0f;
    private readonly float scrollSpeed = 1.5f;

    // Зберігаємо плавний стрес, щоб лінія не розривалася від різких рухів
    private float internalStress = 0f;

    // Тепер Update приймає поточний стрес від гравця/сцени
    public void Update(float currentStress)
    {
        float deltaTime = Raylib.GetFrameTime();
        time += deltaTime * scrollSpeed;

        // Плавно наближаємо внутрішній стрес до цільового
        internalStress += (currentStress - internalStress) * deltaTime * 2f;
    }

    public float GetPathY(float x, int screenHeight)
    {
        float centerY = screenHeight / 2f;

        // 1. ФАЗА ВСТУПУ (Пряма -> Маленькі поштовхи -> Повна крива)
        // Перші 6 секунд гри розмах росте від 0 до 1
        float introProgress = Math.Clamp(time / 6f, 0f, 1f);

        // Використовуємо SmoothStep для дуже м'якого старту
        float introMultiplier = introProgress * introProgress * (3f - 2f * introProgress);

        // 2. ВПЛИВ СТРЕСУ НА РОЗМАХ
        // Якщо стресу немає = 1.0 (як зараз). При повному стресі = 2.5 (дуже широкі гойдалки)
        float amplitudeStressMod = 1f + (internalStress * 1.5f);

        // 3. БАЗОВІ ХВИЛІ
        float wave1 = MathF.Sin((x * 0.0015f) + (time * 0.8f)) * 140f;
        float wave2 = MathF.Sin((x * 0.004f) + (time * 1.5f)) * 70f;

        float chaosModulator = MathF.Sin(time * 0.3f) * 0.5f + 0.5f;
        float wave3 = MathF.Cos((x * 0.007f) + (time * 2.5f)) * (50f * chaosModulator);

        // 4. НЕРВОВА ВІБРАЦІЯ ЛІНІЇ (Тільки при стресі)
        // Додаємо дрібну "пилку", яка робить маршрут візуально нестабільним
        float noise = MathF.Sin(x * 0.05f - time * 15f) * (15f * internalStress);

        // Збираємо все до купи!
        float totalWave = (wave1 + wave2 + wave3) * introMultiplier * amplitudeStressMod + noise;

        return centerY + totalWave;
    }

    public void Draw(int screenWidth, int screenHeight, float currentHue, float stress)
    {
        int step = 15;

        float lightness = 0.98f - (stress * 0.38f);
        float chroma = 0.015f + (stress * 0.135f);
        float hue = (currentHue + time * 10f) % 360f;

        Color lineColor = ColorConverter.OklchToColor(lightness, chroma, hue);
        lineColor.A = (byte)Math.Clamp(160 + (stress * 95), 0, 255);

        float startY = GetPathY(0, screenHeight);
        Vector2 prevPoint = new Vector2(0, startY);

        for (int x = step; x <= screenWidth + step; x += step)
        {
            float y = GetPathY(x, screenHeight);
            Vector2 currentPoint = new Vector2(x, y);

            // При стресі лінія стає не тільки яскравішою, але й трохи товстішою і грубішою
            float thickness = 6f + (stress * 4f);
            Raylib.DrawLineEx(prevPoint, currentPoint, thickness, lineColor);

            prevPoint = currentPoint;
        }
    }
}