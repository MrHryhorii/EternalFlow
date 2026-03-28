namespace EternalFlow.Core;

public class ScoreManager
{
    public float CurrentScore { get; private set; } = 0f;
    public float CurrentMultiplier { get; private set; } = 1f;

    // --- НАЛАШТУВАННЯ ---
    private const float BASE_SCORE_RATE = 5f;     // Базові очки в секунду (при x1)
    private const float MAX_MULTIPLIER = 5f;       // Максимальний множник
    private const float MULTIPLIER_GROWTH = 0.5f;  // Як швидко росте множник (сек)
    private const float MULTIPLIER_DROP = 2.5f;    // Як різко він падає при помилці
    private const float FLOW_THRESHOLD = 0.1f;     // Поріг стресу (10%), нижче якого ми в "потоці"

    public void Update(float currentStress, float deltaTime)
    {
        // Керуємо множником
        if (currentStress <= FLOW_THRESHOLD)
        {
            // Плавно нарощуємо множник
            CurrentMultiplier += MULTIPLIER_GROWTH * deltaTime;
            if (CurrentMultiplier > MAX_MULTIPLIER) CurrentMultiplier = MAX_MULTIPLIER;
        }
        else
        {
            // Швидко здуваємо множник, якщо вийшли з потоку
            CurrentMultiplier -= MULTIPLIER_DROP * deltaTime;
            if (CurrentMultiplier < 1f) CurrentMultiplier = 1f;
        }

        // Нараховуємо очки
        CurrentScore += BASE_SCORE_RATE * CurrentMultiplier * deltaTime;
    }
}