using Raylib_cs;
using System.Numerics;

namespace EternalFlow.Core;

public class PathGenerator
{
    // --- КОНСТАНТИ ВСТУПУ ---
    private const float INTRO_FLAT_DURATION = 3f;
    private const float INTRO_GROWTH_DURATION = 5f;

    private float time = 0f;

    // Швидкість прокрутки світу (пікселів за секунду). 
    // Це швидкість, з якою "ландшафт" летить на гравця.
    private readonly float scrollSpeed = 500f;

    private float internalStress = 0f;

    public void Update(float currentStress, float deltaTime)
    {
        time += deltaTime; // Тут time - це просто загальний час гри
        internalStress += (currentStress - internalStress) * deltaTime * 2f;
    }

    public float GetPathY(float x, int screenHeight)
    {
        float centerY = screenHeight / 2f;

        // ГОЛОВНА МАГІЯ ТУТ: globalX - це фізична точка у "світі" гри.
        // x - це піксель на екрані монітора (від 0 до 1280).
        // Додаючи time * scrollSpeed, ми змушуємо весь ландшафт єдиним монолітом сунути вліво.
        float globalX = x + (time * scrollSpeed);

        // ФАЗА ВСТУПУ
        float growthTime = time - INTRO_FLAT_DURATION;
        float introProgress = Math.Clamp(growthTime / INTRO_GROWTH_DURATION, 0f, 1f);
        float introMultiplier = introProgress * introProgress * (3f - 2f * introProgress);

        // --- МАКРО-ХВИЛЯ (ЗАТЯЖНІ ПІДЙОМИ ТА СПУСКИ) ---
        // Тепер вона залежить ТІЛЬКИ від globalX. Ти побачиш, як гора насувається з правого краю.
        float driftFactor = MathF.Sin(globalX * 0.0003f);
        float driftOffset = driftFactor * (screenHeight * 0.25f) * introMultiplier;
        float dynamicCenterY = centerY + driftOffset;

        // Коли лінія під стелею або біля землі, ми сплющуємо хвилі, щоб вони не вилізали за екран
        float edgeDampening = 1f - (Math.Abs(driftFactor) * 0.4f);

        // ВПЛИВ СТРЕСУ НА РОЗМАХ
        float amplitudeStressMod = 1f + (internalStress * 1.5f);

        // --- БАЗОВІ ХВИЛІ (УСІ прив'язані до globalX) ---
        // Оскільки вони використовують тільки globalX, лінія БІЛЬШЕ НЕ ЗМІНЮЄ ФОРМУ по дорозі до тебе!
        float wave1 = MathF.Sin(globalX * 0.0015f) * 140f;
        float wave2 = MathF.Sin(globalX * 0.004f) * 70f;
        float wave3 = MathF.Cos(globalX * 0.007f) * 50f;

        // Вібрація (Глітч) при високому стресі. Вона єдина має залежати від time, 
        // щоб виглядати як ефект "тремтіння камери/енергії", а не як рельєф.
        float noise = MathF.Sin(x * 0.05f - time * 15f) * (15f * internalStress);

        // Збираємо весь рельєф до купи
        float totalWave = (wave1 + wave2 + wave3) * introMultiplier * amplitudeStressMod * edgeDampening + noise;

        return dynamicCenterY + totalWave;
    }

    public void Draw(int screenWidth, int screenHeight, float currentHue, float stress)
    {
        int step = 15;

        float lightness = 0.98f - (stress * 0.38f);
        float chroma = 0.015f + (stress * 0.135f);
        float alphaFloat = Math.Clamp(160f + (stress * 95f), 0f, 255f);

        if (stress > 0.75f)
        {
            float burnFactor = (stress - 0.75f) / 0.25f;

            lightness *= 1f - burnFactor;
            chroma *= 1f - burnFactor;
            alphaFloat -= (alphaFloat - 15f) * burnFactor;
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

            float thickness = 6f + (stress * 4f);

            Raylib.DrawLineEx(prevPoint, currentPoint, thickness, lineColor);

            prevPoint = currentPoint;
        }
    }
}