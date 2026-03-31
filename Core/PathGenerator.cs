using Raylib_cs;
using System.Numerics;

namespace EternalFlow.Core;

/// <summary>
/// Procedurally generates the main golden path the player must follow.
/// Uses a "Printer Effect" design to create a stable, predictable landscape 
/// that scrolls across the screen without altering its shape mid-flight.
/// </summary>
public class PathGenerator
{
    private const float INTRO_FLAT_DURATION = 3f;
    private const float INTRO_GROWTH_DURATION = 5f;

    private float time = 0f;

    // Controls how fast the generated landscape slides towards the left side of the screen
    private readonly float scrollSpeed = 500f;

    private float internalStress = 0f;

    public void Update(float currentStress, float deltaTime)
    {
        time += deltaTime;

        // Ensure visual transformations caused by stress happen smoothly rather than instantly
        internalStress += (currentStress - internalStress) * deltaTime * 2f;
    }

    public float GetPathY(float x, int screenHeight)
    {
        float centerY = screenHeight / 2f;

        // The core of the Printer Effect.
        // globalX ties the path generation to absolute world space rather than the screen pixel.
        // This ensures the terrain remains static in shape as it scrolls toward the player.
        float globalX = x + (time * scrollSpeed);

        // Gradually transition the path from a flat line into full waves during the introduction
        float growthTime = time - INTRO_FLAT_DURATION;
        float introProgress = Math.Clamp(growthTime / INTRO_GROWTH_DURATION, 0f, 1f);
        float introMultiplier = introProgress * introProgress * (3f - 2f * introProgress);

        // Calculate a slow, massive wave that shifts the entire horizon up and down over time
        float driftFactor = MathF.Sin(globalX * 0.0003f);
        float driftOffset = driftFactor * (screenHeight * 0.25f) * introMultiplier;
        float dynamicCenterY = centerY + driftOffset;

        // Compress the amplitude of waves automatically when the path drifts near the screen edges
        float edgeDampening = 1f - (Math.Abs(driftFactor) * 0.4f);

        // Expand the general amplitude of the path when the player starts losing control
        float amplitudeStressMod = 1f + (internalStress * 1.5f);

        // Generate the core geometry of the path using layered sine waves locked to the global world position
        float wave1 = MathF.Sin(globalX * 0.0015f) * 140f;
        float wave2 = MathF.Sin(globalX * 0.004f) * 70f;
        float wave3 = MathF.Cos(globalX * 0.007f) * 50f;

        // Introduce a harsh, high-frequency jitter when stress is elevated
        // This explicitly uses 'time' instead of globalX to act like screen shake or electrical interference
        float noise = MathF.Sin(x * 0.05f - time * 15f) * (15f * internalStress);

        // Compile all modifiers together to determine the final vertical position for this specific point
        float totalWave = (wave1 + wave2 + wave3) * introMultiplier * amplitudeStressMod * edgeDampening + noise;

        return dynamicCenterY + totalWave;
    }

    public void Draw(int screenWidth, int screenHeight, float currentHue, float stress)
    {
        int step = 15;

        float lightness = 0.98f - (stress * 0.38f);
        float chroma = 0.015f + (stress * 0.135f);
        float alphaFloat = Math.Clamp(160f + (stress * 95f), 0f, 255f);

        // Simulate a burnout effect by draining color and brightness when the player is failing
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

        // Render the connected line segments across the width of the screen
        for (int x = step; x <= screenWidth + step; x += step)
        {
            float y = GetPathY(x, screenHeight);
            Vector2 currentPoint = new(x, y);

            // Physically thicken the path under stress to offer a larger target for recovery
            float thickness = 6f + (stress * 4f);

            Raylib.DrawLineEx(prevPoint, currentPoint, thickness, lineColor);

            prevPoint = currentPoint;
        }
    }
}