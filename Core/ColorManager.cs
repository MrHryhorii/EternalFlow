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

    public void Update(PathGenerator path, int screenHeight)
    {
        float deltaTime = Raylib.GetFrameTime();

        // 1. ЛОГІКА СТРИБКІВ ВІДТІНКУ
        if (!isTransitioning)
        {
            hueTimer -= deltaTime;
            if (hueTimer <= 0)
            {
                isTransitioning = true;
                transitionProgress = 0f;
                startHue = CurrentHue;

                // Гарантуємо відчутну зміну: стрибок від 70 до 160 градусів
                float jumpDelta = Random.Shared.NextSingle() * 90f + 70f;

                // Випадково вибираємо напрямок (вправо чи вліво по колу)
                if (Random.Shared.Next(2) == 0) jumpDelta = -jumpDelta;

                targetHue = startHue + jumpDelta;

                // ЧАС ПЕРЕХОДУ залежить від відстані. 
                // Наприклад, швидкість = 90 градусів за секунду
                float distance = Math.Abs(jumpDelta);
                transitionDuration = distance / 90f;
            }
        }
        else
        {
            // Рухаємо прогрес залежно від вирахованого часу
            transitionProgress += deltaTime / transitionDuration;

            if (transitionProgress >= 1f)
            {
                transitionProgress = 1f;
                isTransitioning = false;

                CurrentHue = targetHue % 360f;
                if (CurrentHue < 0) CurrentHue += 360f;

                // Залипаємо на новому кольорі на 3-5 секунд
                hueTimer = Random.Shared.NextSingle() * 2f + 3f;
            }
            else
            {
                // SmoothStep для змазаного ривка
                float t = transitionProgress * transitionProgress * (3f - 2f * transitionProgress);
                CurrentHue = startHue + (targetHue - startHue) * t;
            }
        }

        // 2. ПЛАВНА СВІТЛОТА ВІД КРИВОЇ
        float pathY = path.GetPathY(100, screenHeight);
        float normalizedY = Math.Clamp(pathY / screenHeight, 0f, 1f);
        float targetLightness = 0.92f - (normalizedY * 0.17f);

        // Зменшили множник з 0.5f до 0.15f для максимальної плавності
        CurrentLightness += (targetLightness - CurrentLightness) * deltaTime * 0.15f;

        // Нормалізуємо для конвертера
        float finalHue = CurrentHue % 360f;
        if (finalHue < 0) finalHue += 360f;

        // Конвертуємо в готовий колір фону
        BackgroundColor = ColorConverter.OklchToColor(CurrentLightness, 0.06f, finalHue);
    }
}