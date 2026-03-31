namespace EternalFlow.Core;

/// <summary>
/// Calculates how stressed the player currently is based on their distance from the golden path.
/// This value dictates the music speed, audio muffling, visual distortions, and score loss.
/// </summary>
public class StressManager
{
    // The final smoothed stress value applied globally to all systems
    public float CurrentStress { get; private set; } = 0f;

    public void Update(Player player, PathGenerator path, int screenHeight, float deltaTime)
    {
        // Extract the target Y position of the path directly underneath the player's X coordinate
        float pathY = path.GetPathY(player.Position.X, screenHeight);
        float deltaY = pathY - player.Position.Y;
        float absDistance = Math.Abs(deltaY);

        // Define the "death zone". 
        // Being off the path by 1/3 of the screen height constitutes 100% maximum stress.
        float maxDistance = screenHeight / 3f;
        float normalizedDist = Math.Clamp(absDistance / maxDistance, 0f, 1f);

        // Use a quadratic curve rather than linear interpolation.
        // This makes small inaccuracies highly forgiving (e.g., 0.1^2 = 0.01 stress), 
        // but punishes being moderately far away very severely (e.g., 0.5^2 = 0.25 stress).
        float targetStress = normalizedDist * normalizedDist;

        // Check if the player is actively trying to correct their mistake.
        // True if their vertical velocity is strongly directed toward the path.
        bool isMovingTowardsPath = (Math.Sign(player.VelocityY) == Math.Sign(deltaY)) && Math.Abs(player.VelocityY) > 50f;

        // Apply stress buildup and recovery with different speeds (viscosity)
        if (targetStress > CurrentStress)
        {
            // Stress builds up faster the further away the player is
            float buildUpSpeed = 1.0f + (targetStress * 2.0f);
            CurrentStress += (targetStress - CurrentStress) * deltaTime * buildUpSpeed;
        }
        else
        {
            // Base recovery is slow, requiring the player to stay on the path to calm down
            float recoverySpeed = 0.4f;

            // Bonus: If the player was highly stressed but is now actively plunging toward the path,
            // provide immediate relief to make the control feel responsive and rewarding
            if (CurrentStress > 0.3f && isMovingTowardsPath)
            {
                recoverySpeed = 2.0f;
            }

            CurrentStress += (targetStress - CurrentStress) * deltaTime * recoverySpeed;
        }

        // Ensure the global stress value never breaks out of the intended 0.0 - 1.0 range
        CurrentStress = Math.Clamp(CurrentStress, 0f, 1f);
    }
}