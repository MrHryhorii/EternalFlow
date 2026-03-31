namespace EternalFlow.Core;

public class ScoreManager
{
    public float CurrentScore { get; private set; } = 0f;
    public float PeakScore { get; private set; } = 0f; // НАЙВИЩИЙ РАХУНОК ЗА ЗАБІГ
    public float CurrentMultiplier { get; private set; } = 1f;

    // Прапорець, який скаже нашій сцені, що час завершувати гру
    public bool IsGameOver { get; private set; } = false;

    // --- НАЛАШТУВАННЯ ---
    private const float BASE_SCORE_RATE = 5f;      // Базові очки в секунду (при x1)

    private const float FLOW_THRESHOLD = 0.1f;     // Поріг стресу (10%), нижче якого ми в "потоці"
    private const float MAX_MULTIPLIER = 5f;       // Максимальний множник для звичайного потоку

    // --- НАЛАШТУВАННЯ ІДЕАЛЬНОГО ПОТОКУ ---
    private const float PERFECT_FLOW_THRESHOLD = 0.01f; // Поріг для ідеального потоку (1%)
    private const float PERFECT_MAX_MULTIPLIER = 10f;   // Супер-множник для ідеального потоку

    private const float MULTIPLIER_GROWTH = 0.5f;  // Як швидко росте множник (сек)
    private const float MULTIPLIER_DROP = 2.5f;    // Як різко він падає при помилці

    // --- НАЛАШТУВАННЯ КРИТИЧНОГО СТАНУ ---
    private const float BURN_THRESHOLD = 0.75f;    // Починаємо горіти після 75% стресу
    private const float MAX_BURN_RATE = 30f;       // Максимальна швидкість згоряння (при теоретичних 100%)

    public void Update(float currentStress, float deltaTime)
    {
        // Якщо гра вже закінчилася, більше нічого не рахуємо
        if (IsGameOver) return;

        // Якщо стрес у "червоній зоні" (більше 75%)
        if (currentStress >= BURN_THRESHOLD)
        {
            // Вираховуємо, наскільки глибоко ми в червоній зоні (від 0.0 до 1.0)
            float burnFactor = (currentStress - BURN_THRESHOLD) / (1f - BURN_THRESHOLD);

            // Множник миттєво збивається до бази
            CurrentMultiplier = 1f;

            // Чим ближче до 100% стресу, тим швидше горять очки
            CurrentScore -= MAX_BURN_RATE * burnFactor * deltaTime;

            // Якщо рахунок згорів до нуля — потік перервано остаточно
            if (CurrentScore <= 0f)
            {
                CurrentScore = 0f;
                IsGameOver = true;
            }
        }
        else
        {
            // --- НОРМАЛЬНИЙ ПОЛІТ ---

            // Керуємо множником залежно від рівня стресу
            if (currentStress <= PERFECT_FLOW_THRESHOLD)
            {
                // ІДЕАЛЬНИЙ ПОТІК (< 1% стресу)
                // Множник росте вдвічі швидше і може сягнути x10
                CurrentMultiplier += MULTIPLIER_GROWTH * 2f * deltaTime;
                if (CurrentMultiplier > PERFECT_MAX_MULTIPLIER) CurrentMultiplier = PERFECT_MAX_MULTIPLIER;
            }
            else if (currentStress <= FLOW_THRESHOLD)
            {
                // ЗВИЧАЙНИЙ ПОТІК (< 10% стресу)
                if (CurrentMultiplier <= MAX_MULTIPLIER)
                {
                    // Ростемо до x5
                    CurrentMultiplier += MULTIPLIER_GROWTH * deltaTime;
                    if (CurrentMultiplier > MAX_MULTIPLIER) CurrentMultiplier = MAX_MULTIPLIER;
                }
                else
                {
                    // Якщо гравець випав з ідеального потоку (наприклад, мав x8),
                    // множник не обнуляється, а плавно здувається до x5
                    CurrentMultiplier -= MULTIPLIER_DROP * 0.5f * deltaTime;
                    if (CurrentMultiplier < MAX_MULTIPLIER) CurrentMultiplier = MAX_MULTIPLIER;
                }
            }
            else
            {
                // СТРЕС (> 10%) - Множник стрімко падає до x1
                CurrentMultiplier -= MULTIPLIER_DROP * deltaTime;
                if (CurrentMultiplier < 1f) CurrentMultiplier = 1f;
            }

            // Нараховуємо очки
            CurrentScore += BASE_SCORE_RATE * CurrentMultiplier * deltaTime;

            // ЗАПАМ'ЯТОВУЄМО РЕКОРД ЦЬОГО ЗАБІГУ
            if (CurrentScore > PeakScore)
            {
                PeakScore = CurrentScore;
            }
        }
    }
}