namespace EternalFlow.Core;

public class ScoreManager
{
    public float CurrentScore { get; private set; } = 0f;
    public float PeakScore { get; private set; } = 0f;
    public float CurrentMultiplier { get; private set; } = 1f;

    public bool IsGameOver { get; private set; } = false;

    // --- НОВІ ЗМІННІ ДЛЯ ЧАСУ ПОТОКУ ---
    public float CurrentPerfectFlowTime { get; private set; } = 0f;
    public float PeakPerfectFlowTime { get; private set; } = 0f;

    private const float BASE_SCORE_RATE = 5f;
    private const float FLOW_THRESHOLD = 0.1f;
    private const float MAX_MULTIPLIER = 5f;
    private const float PERFECT_FLOW_THRESHOLD = 0.01f;
    private const float PERFECT_MAX_MULTIPLIER = 10f;
    private const float MULTIPLIER_GROWTH = 0.5f;
    private const float MULTIPLIER_DROP = 2.5f;
    private const float BURN_THRESHOLD = 0.75f;
    private const float MAX_BURN_RATE = 30f;

    public void Update(float currentStress, float deltaTime)
    {
        if (IsGameOver) return;

        // Підрахунок часу ідеального потоку
        if (currentStress <= PERFECT_FLOW_THRESHOLD)
        {
            CurrentPerfectFlowTime += deltaTime;
            if (CurrentPerfectFlowTime > PeakPerfectFlowTime)
            {
                PeakPerfectFlowTime = CurrentPerfectFlowTime;
            }
        }
        else
        {
            // Якщо стрес перевищив поріг, лічильник поточного часу скидається
            CurrentPerfectFlowTime = 0f;
        }

        if (currentStress >= BURN_THRESHOLD)
        {
            float burnFactor = (currentStress - BURN_THRESHOLD) / (1f - BURN_THRESHOLD);
            CurrentMultiplier = 1f;
            CurrentScore -= MAX_BURN_RATE * burnFactor * deltaTime;

            if (CurrentScore <= 0f)
            {
                CurrentScore = 0f;
                IsGameOver = true;
            }
        }
        else
        {
            if (currentStress <= PERFECT_FLOW_THRESHOLD)
            {
                CurrentMultiplier += MULTIPLIER_GROWTH * 2f * deltaTime;
                if (CurrentMultiplier > PERFECT_MAX_MULTIPLIER) CurrentMultiplier = PERFECT_MAX_MULTIPLIER;
            }
            else if (currentStress <= FLOW_THRESHOLD)
            {
                if (CurrentMultiplier <= MAX_MULTIPLIER)
                {
                    CurrentMultiplier += MULTIPLIER_GROWTH * deltaTime;
                    if (CurrentMultiplier > MAX_MULTIPLIER) CurrentMultiplier = MAX_MULTIPLIER;
                }
                else
                {
                    CurrentMultiplier -= MULTIPLIER_DROP * 0.5f * deltaTime;
                    if (CurrentMultiplier < MAX_MULTIPLIER) CurrentMultiplier = MAX_MULTIPLIER;
                }
            }
            else
            {
                CurrentMultiplier -= MULTIPLIER_DROP * deltaTime;
                if (CurrentMultiplier < 1f) CurrentMultiplier = 1f;
            }

            CurrentScore += BASE_SCORE_RATE * CurrentMultiplier * deltaTime;

            if (CurrentScore > PeakScore)
            {
                PeakScore = CurrentScore;
            }
        }
    }
}