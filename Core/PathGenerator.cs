using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;

namespace EternalFlow.Core;

public class PathGenerator
{
    private float time = 0f;
    private readonly float scrollSpeed = 1.5f; // Трохи зменшили загальну швидкість часу для плавності

    public void Update()
    {
        time += GetFrameTime() * scrollSpeed;
    }

    // Цей метод тепер рахує висоту лінії для будь-якої точки X
    public float GetPathY(float x, int screenHeight)
    {
        float centerY = screenHeight / 2f;

        // 1. Повільна хвиля (задає довгі підйоми та спуски)
        float wave1 = (float)Math.Sin((x * 0.0015f) + (time * 0.8f)) * 140f;

        // 2. Середня хвиля (стандартні вигини)
        float wave2 = (float)Math.Sin((x * 0.004f) + (time * 1.5f)) * 70f;

        // 3. Динамічна хвиля, яка іноді зникає
        // Math.Sin(time * 0.3f) дає значення від -1 до 1. 
        // Трохи математики, і ми отримуємо множник від 0 до 1, який періодично "вимикає" третю хвилю
        float chaosModulator = (float)Math.Sin(time * 0.3f) * 0.5f + 0.5f;
        float wave3 = (float)Math.Cos((x * 0.007f) + (time * 2.5f)) * (50f * chaosModulator);

        // Складаємо всі хвилі разом
        return centerY + wave1 + wave2 + wave3;
    }

    public void Draw(int screenWidth, int screenHeight, float currentHue, float stress)
    {
        int step = 15;

        float lightness = 0.98f - (stress * 0.38f);
        float chroma = 0.015f + (stress * 0.135f);
        float hue = (currentHue + time * 10f) % 360f;

        Color lineColor = ColorConverter.OklchToColor(lightness, chroma, hue);

        // ДОДАЄМО ПРОЗОРІСТЬ:
        // Альфа-канал в Raylib вимірюється від 0 (невидимий) до 255 (суцільний).
        // Якщо stress = 0, прозорість буде 160 (це близько 60% видимості - достатньо для легкого злиття).
        // Якщо stress = 1, прозорість стане 255 (абсолютно тверда лінія).
        byte alpha = (byte)Math.Clamp(160 + (stress * 95), 0, 255);
        lineColor.A = alpha;

        float startY = GetPathY(0, screenHeight);
        Vector2 prevPoint = new(0, startY);

        for (int x = step; x <= screenWidth + step; x += step)
        {
            float y = GetPathY(x, screenHeight);
            Vector2 currentPoint = new(x, y);

            DrawLineEx(prevPoint, currentPoint, 6f, lineColor);

            prevPoint = currentPoint;
        }
    }
}