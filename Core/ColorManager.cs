using Raylib_cs;

namespace EternalFlow.Core;

public class ColorManager
{
    public Color BackgroundColor { get; private set; }
    public float CurrentHue { get; private set; }
    public float CurrentLightness { get; private set; }

    private float targetHue;
    private float startHue;

    private float hueTimer = 2f;
    private bool isTransitioning = false;
    private float transitionProgress = 0f;
    private float transitionDuration = 1f; // Динамічний час переходу

    public ColorManager()
    {
        CurrentHue = Random.Shared.NextSingle() * 360f;
        CurrentLightness = 0.9f;
    }

    // Тепер ми приймаємо stress як параметр
    public void Update(PathGenerator path, int screenHeight, float stress, float deltaTime)
    {
        // 1. ЛОГІКА СТРИБКІВ ВІДТІНКУ (залишається без змін)
        if (!isTransitioning)
        {
            hueTimer -= deltaTime;
            if (hueTimer <= 0)
            {
                isTransitioning = true;
                transitionProgress = 0f;
                startHue = CurrentHue;

                float jumpDelta = Random.Shared.NextSingle() * 90f + 70f;
                if (Random.Shared.Next(2) == 0) jumpDelta = -jumpDelta;

                targetHue = startHue + jumpDelta;
                transitionDuration = Math.Abs(jumpDelta) / 90f;
            }
        }
        else
        {
            transitionProgress += deltaTime / transitionDuration;

            if (transitionProgress >= 1f)
            {
                transitionProgress = 1f;
                isTransitioning = false;

                CurrentHue = targetHue % 360f;
                if (CurrentHue < 0) CurrentHue += 360f;

                hueTimer = Random.Shared.NextSingle() * 2f + 3f;
            }
            else
            {
                float t = transitionProgress * transitionProgress * (3f - 2f * transitionProgress);
                CurrentHue = startHue + (targetHue - startHue) * t;
            }
        }

        // 2. ПЛАВНА СВІТЛОТА З УРАХУВАННЯМ СТРЕСУ
        float pathY = path.GetPathY(100, screenHeight);
        float normalizedY = Math.Clamp(pathY / screenHeight, 0f, 1f);

        // Базове затемнення = 0.17. При максимальному стресі воно збільшується на 0.3 (стає 0.47)
        // Тобто в "ямах" фон провалюватиметься від 0.92 аж до 0.45 (це дуже глибокий контрастний колір)
        float darkeningPower = 0.17f + (stress * 0.3f);
        float targetLightness = 0.92f - (normalizedY * darkeningPower);

        CurrentLightness += (targetLightness - CurrentLightness) * deltaTime * 0.15f;

        // Нормалізуємо для конвертера
        float finalHue = CurrentHue % 360f;
        if (finalHue < 0) finalHue += 360f;

        BackgroundColor = ColorConverter.OklchToColor(CurrentLightness, 0.06f, finalHue);
    }
}