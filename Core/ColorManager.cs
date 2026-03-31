using Raylib_cs;

namespace EternalFlow.Core;

public class ColorManager
{
    // --- КОНСТАНТИ ФАЗИ КОЛЬОРУ ---
    // Межі того, наскільки довго колір може "затягнутися" (у секундах)
    private const float MIN_PHASE_DURATION = 15f;
    private const float MAX_PHASE_DURATION = 30f;

    // Наскільки ріжеться цей час при повному (100%) стресі (0.0 - 1.0)
    private const float MAX_STRESS_TIME_REDUCTION = 0.7f;

    public Color BackgroundColor { get; private set; }
    public float CurrentHue { get; private set; }
    public float CurrentLightness { get; private set; }

    private float targetHue;
    private float startHue;

    private float hueTimer = MIN_PHASE_DURATION;
    private bool isTransitioning = false;
    private float transitionProgress = 0f;
    private float transitionDuration = 1f;

    // --- ЗМІННІ ДЛЯ ДЕТЕКТУВАННЯ БІТУ ---
    private float previousAmplitude = 0f;
    private float beatCooldown = 0f;

    public ColorManager()
    {
        CurrentHue = Random.Shared.NextSingle() * 360f;
        CurrentLightness = 0.9f;
    }

    public void Update(PathGenerator path, int screenHeight, float stress, float deltaTime)
    {
        // --- ДЕТЕКТОР БІТУ ДЛЯ СИНХРОНІЗАЦІЇ ПЕРЕХОДУ ---
        float currentAmp = AudioManager.RealtimeAmplitude;
        if (beatCooldown > 0) beatCooldown -= deltaTime;

        // Вважаємо бітом різкий стрибок амплітуди.
        // Це ідеально спрацює на потужний бас або на різкий вступ нового треку після тиші.
        bool isBeat = currentAmp > 0.3f && currentAmp > previousAmplitude + 0.05f && beatCooldown <= 0f;

        if (isBeat)
        {
            beatCooldown = 0.2f;
        }

        previousAmplitude = currentAmp;

        // --- ЛОГІКА СТРИБКІВ ВІДТІНКУ ТА ФАЗИ ---
        if (!isTransitioning)
        {
            // Рахуємо множник швидкості часу залежно від стресу
            float timeMultiplier = 1f / (1f - (stress * MAX_STRESS_TIME_REDUCTION));

            // Таймер спадає, але якщо він впав нижче нуля - він просто залишається нулем і чекає.
            hueTimer -= deltaTime * timeMultiplier;
            if (hueTimer < 0) hueTimer = 0;

            // СУПЕР-УМОВА: Перехід починається ТІЛЬКИ якщо таймер вийшов І прямо зараз лунає біт!
            if (hueTimer <= 0 && isBeat)
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
            // Сам перехід між кольорами йде зі своєю швидкістю
            transitionProgress += deltaTime / transitionDuration;

            if (transitionProgress >= 1f)
            {
                transitionProgress = 1f;
                isTransitioning = false;

                CurrentHue = targetHue % 360f;
                if (CurrentHue < 0) CurrentHue += 360f;

                // Задаємо новий базовий час для наступної фази
                hueTimer = Random.Shared.NextSingle() * (MAX_PHASE_DURATION - MIN_PHASE_DURATION) + MIN_PHASE_DURATION;
            }
            else
            {
                // Плавна інтерполяція (Ease In-Out)
                float t = transitionProgress * transitionProgress * (3f - 2f * transitionProgress);
                CurrentHue = startHue + (targetHue - startHue) * t;
            }
        }

        // --- ПЛАВНА СВІТЛОТА З УРАХУВАННЯМ СТРЕСУ ---
        float pathY = path.GetPathY(100, screenHeight);
        float normalizedY = Math.Clamp(pathY / screenHeight, 0f, 1f);

        float darkeningPower = 0.17f + (stress * 0.3f);
        float targetLightness = 0.92f - (normalizedY * darkeningPower);
        float targetChroma = 0.06f; // Базова насиченість фону

        float lightnessLerpSpeed = 0.15f;

        // Занурення в темряву (Стрес > 75%)
        if (stress > 0.75f)
        {
            float burnFactor = (stress - 0.75f) / 0.25f;

            // Силою тягнемо світлоту та кольоровість до чорного
            targetLightness *= 1f - burnFactor;
            targetChroma *= 1f - burnFactor;

            lightnessLerpSpeed = 2.0f + (burnFactor * 5f);
        }

        CurrentLightness += (targetLightness - CurrentLightness) * deltaTime * lightnessLerpSpeed;

        float finalHue = CurrentHue % 360f;
        if (finalHue < 0) finalHue += 360f;

        BackgroundColor = ColorConverter.OklchToColor(CurrentLightness, targetChroma, finalHue);
    }
}