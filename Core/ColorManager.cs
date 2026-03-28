using Raylib_cs;
using System;

namespace EternalFlow.Core;

public class ColorManager
{
    // --- КОНСТАНТИ ФАЗИ КОЛЬОРУ ---
    // Межі того, наскільки довго колір може "затягнутися" (у секундах)
    private const float MIN_PHASE_DURATION = 15f;
    private const float MAX_PHASE_DURATION = 30f;

    // Наскільки ріжеться цей час при повному (100%) стресі (0.0 - 1.0)
    // 0.7f означає, що час скоротиться на 70% (кольори змінюватимуться дуже швидко)
    private const float MAX_STRESS_TIME_REDUCTION = 0.7f;
    // ------------------------------

    public Color BackgroundColor { get; private set; }
    public float CurrentHue { get; private set; }
    public float CurrentLightness { get; private set; }

    private float targetHue;
    private float startHue;

    private float hueTimer = MIN_PHASE_DURATION;
    private bool isTransitioning = false;
    private float transitionProgress = 0f;
    private float transitionDuration = 1f;

    public ColorManager()
    {
        CurrentHue = Random.Shared.NextSingle() * 360f;
        CurrentLightness = 0.9f;
    }

    public void Update(PathGenerator path, int screenHeight, float stress, float deltaTime)
    {
        // ЛОГІКА СТРИБКІВ ВІДТІНКУ ТА ФАЗИ
        if (!isTransitioning)
        {
            // Рахуємо множник швидкості часу.
            // Якщо reduction = 0.7 і stress = 1, дільник = 0.3. Час для кольору йде в 3.3 рази швидше!
            float timeMultiplier = 1f / (1f - (stress * MAX_STRESS_TIME_REDUCTION));

            // Таймер спадає плавно, але швидше, якщо стрес високий
            hueTimer -= deltaTime * timeMultiplier;

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
            // Сам перехід між кольорами йде зі своєю швидкістю
            transitionProgress += deltaTime / transitionDuration;

            if (transitionProgress >= 1f)
            {
                transitionProgress = 1f;
                isTransitioning = false;

                CurrentHue = targetHue % 360f;
                if (CurrentHue < 0) CurrentHue += 360f;

                // Задаємо новий базовий час фази в межах наших констант
                hueTimer = Random.Shared.NextSingle() * (MAX_PHASE_DURATION - MIN_PHASE_DURATION) + MIN_PHASE_DURATION;
            }
            else
            {
                float t = transitionProgress * transitionProgress * (3f - 2f * transitionProgress);
                CurrentHue = startHue + (targetHue - startHue) * t;
            }
        }

        // ПЛАВНА СВІТЛОТА З УРАХУВАННЯМ СТРЕСУ
        float pathY = path.GetPathY(100, screenHeight);
        float normalizedY = Math.Clamp(pathY / screenHeight, 0f, 1f);

        float darkeningPower = 0.17f + (stress * 0.3f);
        float targetLightness = 0.92f - (normalizedY * darkeningPower);
        float targetChroma = 0.06f; // Базова насиченість фону

        float lightnessLerpSpeed = 0.15f; // Звичайна розслаблена швидкість зміни

        // --- ЗАНУРЕННЯ В ТЕМРЯВУ (Стрес > 75%) ---
        if (stress > 0.75f)
        {
            float burnFactor = (stress - 0.75f) / 0.25f; // Від 0.0 до 1.0

            // Силою тягнемо світлоту та кольоровість до чорного
            targetLightness *= (1f - burnFactor);
            targetChroma *= (1f - burnFactor);

            // Коли ми горимо, темрява має наступати швидко і агресивно
            lightnessLerpSpeed = 2.0f + (burnFactor * 5f);
        }

        CurrentLightness += (targetLightness - CurrentLightness) * deltaTime * lightnessLerpSpeed;

        float finalHue = CurrentHue % 360f;
        if (finalHue < 0) finalHue += 360f;

        // Використовуємо наш targetChroma, який згасає при стресі
        BackgroundColor = ColorConverter.OklchToColor(CurrentLightness, targetChroma, finalHue);
    }
}