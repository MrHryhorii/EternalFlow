using Raylib_cs;
using System.Numerics;

namespace EternalFlow.Core;

public class PathGenerator
{
    // --- КОНСТАНТИ ВСТУПУ ---
    // Скільки секунд на початку лінія абсолютно пряма
    private const float INTRO_FLAT_DURATION = 3f;

    // Скільки секунд після цього вона плавно розгойдується до максимуму
    private const float INTRO_GROWTH_DURATION = 5f;
    // ------------------------

    private float time = 0f;
    private readonly float scrollSpeed = 1.5f;

    // Зберігаємо плавний стрес, щоб лінія не розривалася від різких рухів
    private float internalStress = 0f;

    public void Update(float currentStress, float deltaTime)
    {
        time += deltaTime * scrollSpeed;

        // Плавно наближаємо внутрішній стрес до цільового
        internalStress += (currentStress - internalStress) * deltaTime * 2f;
    }

    public float GetPathY(float x, int screenHeight)
    {
        float centerY = screenHeight / 2f;

        // ФАЗА ВСТУПУ (Пряма -> Маленькі поштовхи -> Повна крива)
        // Віднімаємо час "прямої" фази. Якщо результат < 0, то прогрес буде 0.
        float growthTime = time - INTRO_FLAT_DURATION;
        float introProgress = Math.Clamp(growthTime / INTRO_GROWTH_DURATION, 0f, 1f);

        // Використовуємо SmoothStep для дуже м'якого старту
        float introMultiplier = introProgress * introProgress * (3f - 2f * introProgress);

        // ВПЛИВ СТРЕСУ НА РОЗМАХ
        // Якщо стресу немає = 1.0 (як зараз). При повному стресі = 2.5 (дуже широкі гойдалки)
        float amplitudeStressMod = 1f + (internalStress * 1.5f);

        // БАЗОВІ ХВИЛІ
        float wave1 = MathF.Sin((x * 0.0015f) + (time * 0.8f)) * 140f;
        float wave2 = MathF.Sin((x * 0.004f) + (time * 1.5f)) * 70f;

        float chaosModulator = MathF.Sin(time * 0.3f) * 0.5f + 0.5f;
        float wave3 = MathF.Cos((x * 0.007f) + (time * 2.5f)) * (50f * chaosModulator);

        // НЕРВОВА ВІБРАЦІЯ ЛІНІЇ (Тільки при стресі)
        // Додаємо дрібну "пилку", яка робить маршрут візуально нестабільним
        float noise = MathF.Sin(x * 0.05f - time * 15f) * (15f * internalStress);

        // Збираємо все до купи!
        float totalWave = (wave1 + wave2 + wave3) * introMultiplier * amplitudeStressMod + noise;

        return centerY + totalWave;
    }

    public void Draw(int screenWidth, int screenHeight, float currentHue, float stress)
    {
        int step = 15;

        // Базові налаштування кольору (залежать від стресу)
        float lightness = 0.98f - (stress * 0.38f);
        float chroma = 0.015f + (stress * 0.135f);
        float alphaFloat = Math.Clamp(160f + (stress * 95f), 0f, 255f);

        // --- ВТРАТА ПОТОКУ (Згасання у темряву) ---
        if (stress > 0.75f)
        {
            float burnFactor = (stress - 0.75f) / 0.25f; // Від 0.0 до 1.0

            // Силою тягнемо світлоту до нуля (чорний колір)
            lightness *= (1f - burnFactor);

            // Силою вбиваємо насиченість
            chroma *= (1f - burnFactor);

            // Плавно робимо лінію майже прозорою (залишаємо привид лінії = 15/255)
            alphaFloat = alphaFloat - (alphaFloat - 15f) * burnFactor;
        }

        float hue = (currentHue + time * 10f) % 360f;
        Color lineColor = ColorConverter.OklchToColor(lightness, chroma, hue);
        lineColor.A = (byte)alphaFloat;

        float startY = GetPathY(0, screenHeight);
        Vector2 prevPoint = new(0, startY);

        for (int x = step; x <= screenWidth + step; x += step)
        {
            float y = GetPathY(x, screenHeight);
            Vector2 currentPoint = new(x, y);

            // Товщина лінії при стресі залишається великою
            float thickness = 6f + (stress * 4f);

            // Малюємо суцільну, але згасаючу лінію
            Raylib.DrawLineEx(prevPoint, currentPoint, thickness, lineColor);

            prevPoint = currentPoint;
        }
    }
}