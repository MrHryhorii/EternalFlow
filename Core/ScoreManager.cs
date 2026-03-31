namespace EternalFlow.Core;

/// <summary>
/// Tracks the player's performance. 
/// Rewards sustained perfection with high score multipliers and tracks the longest "Perfect Flow" duration.
/// Penalizes critical stress by burning away accumulated points.
/// </summary>
public class ScoreManager
{
    public float CurrentScore { get; private set; } = 0f;
    public float PeakScore { get; private set; } = 0f;
    public float CurrentMultiplier { get; private set; } = 1f;

    public bool IsGameOver { get; private set; } = false;

    // Metrics tracking how long the player stays perfectly centered on the path
    public float CurrentPerfectFlowTime { get; private set; } = 0f;
    public float PeakPerfectFlowTime { get; private set; } = 0f;

    private const float BASE_SCORE_RATE = 5f;

    // Thresholds defining the state of the player based on current stress
    private const float PERFECT_FLOW_THRESHOLD = 0.01f;
    private const float FLOW_THRESHOLD = 0.1f;
    private const float BURN_THRESHOLD = 0.75f;

    // Multiplier limits and growth rates
    private const float MAX_MULTIPLIER = 5f;
    private const float PERFECT_MAX_MULTIPLIER = 10f;
    private const float MULTIPLIER_GROWTH = 0.5f;
    private const float MULTIPLIER_DROP = 2.5f;
    private const float MAX_BURN_RATE = 30f;

    public void Update(float currentStress, float deltaTime)
    {
        if (IsGameOver) return;

        // --- PERFECT FLOW TIMER ---
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
            // Instantly break the perfect streak if stress rises even slightly above the threshold
            CurrentPerfectFlowTime = 0f;
        }

        // --- CRITICAL STRESS BURNOUT ---
        // If the player is far off the path, stop giving points and start actively burning their score
        if (currentStress >= BURN_THRESHOLD)
        {
            float burnFactor = (currentStress - BURN_THRESHOLD) / (1f - BURN_THRESHOLD);
            CurrentMultiplier = 1f;
            CurrentScore -= MAX_BURN_RATE * burnFactor * deltaTime;

            // Trigger game over if the score is entirely depleted
            if (CurrentScore <= 0f)
            {
                CurrentScore = 0f;
                IsGameOver = true;
            }
        }
        else
        {
            // --- SCORING AND MULTIPLIER LOGIC ---
            // Rapidly build up to x10 multiplier if in the Perfect Flow state
            if (currentStress <= PERFECT_FLOW_THRESHOLD)
            {
                CurrentMultiplier += MULTIPLIER_GROWTH * 2f * deltaTime;
                if (CurrentMultiplier > PERFECT_MAX_MULTIPLIER) CurrentMultiplier = PERFECT_MAX_MULTIPLIER;
            }
            // Maintain up to x5 multiplier if generally on the path but not perfectly centered
            else if (currentStress <= FLOW_THRESHOLD)
            {
                if (CurrentMultiplier <= MAX_MULTIPLIER)
                {
                    CurrentMultiplier += MULTIPLIER_GROWTH * deltaTime;
                    if (CurrentMultiplier > MAX_MULTIPLIER) CurrentMultiplier = MAX_MULTIPLIER;
                }
                else
                {
                    // Decay the multiplier down to x5 if they lose perfect accuracy
                    CurrentMultiplier -= MULTIPLIER_DROP * 0.5f * deltaTime;
                    if (CurrentMultiplier < MAX_MULTIPLIER) CurrentMultiplier = MAX_MULTIPLIER;
                }
            }
            // Rapidly drop the multiplier back to x1 if stress begins to rise significantly
            else
            {
                CurrentMultiplier -= MULTIPLIER_DROP * deltaTime;
                if (CurrentMultiplier < 1f) CurrentMultiplier = 1f;
            }

            // Award points based on the current multiplier
            CurrentScore += BASE_SCORE_RATE * CurrentMultiplier * deltaTime;

            if (CurrentScore > PeakScore)
            {
                PeakScore = CurrentScore;
            }
        }
    }
}