using Raylib_cs;
using System.Numerics;

namespace EternalFlow.Core;

/// <summary>
/// Represents the player entity (the glowing orb).
/// Handles its visual representation, the echo trail, and the generation of "Perfect Flow" dust particles.
/// </summary>
public class Player(int screenWidth, int screenHeight)
{
    public Vector2 Position = new Vector2(screenWidth * SCREEN_X_RATIO, screenHeight / 2f);
    public float VelocityY = 0f;

    private readonly float baseRadius = 25f;
    private float time = 0f;

    // Fixes the player horizontally at 25% of the screen width from the left edge.
    // This provides ample space on the right to see and react to the upcoming path.
    private const float SCREEN_X_RATIO = 0.25f;

    // Variables for synchronizing visual pulses and the echo trail with the music
    private float previousAmplitude = 0f;
    private float beatCooldown = 0f;
    private float fallbackTimer = 0f; // Failsafe timer to emit a trail even during very quiet sections

    private class TrailEcho
    {
        public Vector2 Position;
        public float Life;
        public float Radius;
        public float InitialAmplitude;
    }

    private readonly List<TrailEcho> trail = [];

    // Tracks how deeply the player is immersed in the "Perfect Flow" state (0.0 to 1.0)
    private float perfectGlow = 0f;

    private class DustParticle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Life;
        public float Size;
    }
    private readonly List<DustParticle> dustParticles = [];

    public void Update(float deltaTime, float stress)
    {
        time += deltaTime;

        // --- AUDIO REACTIVE TRAIL ---
        float currentAmp = AudioManager.RealtimeAmplitude;
        if (beatCooldown > 0) beatCooldown -= deltaTime;
        fallbackTimer -= deltaTime;

        // Detect a musical beat to spawn a major echo ring
        bool isBeat = currentAmp > 0.3f && currentAmp > previousAmplitude + 0.05f && beatCooldown <= 0f;

        if (isBeat || fallbackTimer <= 0)
        {
            trail.Add(new TrailEcho
            {
                Position = this.Position,
                Life = 1f,
                Radius = baseRadius,
                InitialAmplitude = isBeat ? currentAmp : 0.2f
            });

            if (isBeat) beatCooldown = 0.2f;
            fallbackTimer = 0.8f;
        }

        previousAmplitude = currentAmp;

        // Update physics for the visual echo trail behind the player
        for (int i = trail.Count - 1; i >= 0; i--)
        {
            trail[i].Life -= deltaTime * 0.6f;
            trail[i].Position.X -= 300f * deltaTime;

            // Rings created during louder beats expand faster
            float expansionSpeed = 10f + (trail[i].InitialAmplitude * 30f);
            trail[i].Radius += deltaTime * expansionSpeed;

            if (trail[i].Life <= 0)
            {
                trail.RemoveAt(i);
            }
        }

        // --- PERFECT FLOW STATE LOGIC ---
        // If stress is virtually zero, gradually increase the aura and dust emission
        if (stress < 0.01f)
        {
            perfectGlow += deltaTime * 0.6f;
        }
        else
        {
            // Rapidly lose the glow if the player deviates from the center of the path
            perfectGlow -= deltaTime * 1.2f;
        }

        perfectGlow = Math.Clamp(perfectGlow, 0f, 1f);

        // --- GENERATE SILVER DUST ---
        // Dust only appears when the player maintains high accuracy
        if (perfectGlow > 0f)
        {
            // Use a polynomial curve so the dust volume scales exponentially as perfection increases
            float dustCurve = MathF.Pow(perfectGlow, 1.5f);

            int particlesToSpawn = (int)(dustCurve * 2f);
            if (Random.Shared.NextSingle() < dustCurve) particlesToSpawn++;

            for (int i = 0; i < particlesToSpawn; i++)
            {
                dustParticles.Add(new DustParticle
                {
                    Position = new Vector2(
                        Position.X + (Random.Shared.NextSingle() - 0.5f) * baseRadius,
                        Position.Y + (Random.Shared.NextSingle() - 0.5f) * baseRadius
                    ),
                    Velocity = new Vector2(
                        -Random.Shared.NextSingle() * 100f - 50f,
                        (Random.Shared.NextSingle() - 0.5f) * 40f
                    ),
                    Life = 1f + Random.Shared.NextSingle(),
                    Size = Random.Shared.NextSingle() * 2f + 1f
                });
            }
        }

        // Update physics for the silver dust
        for (int i = dustParticles.Count - 1; i >= 0; i--)
        {
            dustParticles[i].Life -= deltaTime;
            dustParticles[i].Position += dustParticles[i].Velocity * deltaTime;
            if (dustParticles[i].Life <= 0) dustParticles.RemoveAt(i);
        }
    }

    public void Draw(float stress)
    {
        float currentAmp = AudioManager.RealtimeAmplitude;

        // Combine a slow, idle breathing animation with an aggressive audio pulse
        float baseBreathing = MathF.Sin(time * 3f) * 0.05f;
        float audioPulse = currentAmp * 0.4f;

        float currentMembraneRadius = baseRadius * (1f + baseBreathing + audioPulse);

        Color coreColor = ColorConverter.OklchToColor(0.98f, 0.02f, 60f);
        Color membraneColor = Color.White;

        // Render the echo trail first so it appears underneath the player
        foreach (var echo in trail)
        {
            float alphaMultiplier = 0.5f + (echo.InitialAmplitude * 0.5f);
            byte alpha = (byte)(50 * echo.Life * alphaMultiplier);

            Color echoMembrane = membraneColor;
            echoMembrane.A = alpha;

            Color echoCore = coreColor;
            echoCore.A = (byte)(20 * echo.Life * alphaMultiplier);

            Raylib.DrawPolyLinesEx(echo.Position, 40, echo.Radius, 0f, 2f * echo.Life, echoMembrane);
            Raylib.DrawCircleV(echo.Position, echo.Radius * 0.4f, echoCore);
        }

        // --- GLITCH / CHROMATIC ABERRATION EFFECT ---
        // Activates when stress is dangerously high, making the player orb visually unstable
        float glitchOffset = 0f;
        float verticalJitter = 0f;

        if (stress > 0.6f)
        {
            float glitchFactor = (stress - 0.6f) / 0.4f;
            glitchOffset = glitchFactor * 8f;

            if (Random.Shared.NextSingle() < glitchFactor * 0.5f)
            {
                glitchOffset += (Random.Shared.NextSingle() - 0.5f) * 4f;
                verticalJitter = (Random.Shared.NextSingle() - 0.5f) * (glitchFactor * 3f);
            }
        }

        Vector2 renderPos = new(Position.X, Position.Y + verticalJitter);

        if (glitchOffset > 0f)
        {
            Raylib.BeginBlendMode(BlendMode.Additive);

            // Red channel shifted left
            Color rColor = new(255, 30, 30, 180);
            Vector2 rPos = new(renderPos.X - glitchOffset, renderPos.Y);
            Raylib.DrawCircleV(rPos, baseRadius * 0.4f, rColor);
            Raylib.DrawPolyLinesEx(rPos, 40, currentMembraneRadius, 0f, 2.0f, new Color(255, 0, 0, 100));

            // Blue channel shifted right
            Color bColor = new(30, 100, 255, 180);
            Vector2 bPos = new(renderPos.X + glitchOffset, renderPos.Y);
            Raylib.DrawCircleV(bPos, baseRadius * 0.4f, bColor);
            Raylib.DrawPolyLinesEx(bPos, 40, currentMembraneRadius, 0f, 2.0f, new Color(0, 100, 255, 100));

            Raylib.EndBlendMode();
        }

        // --- PERFECT FLOW AURA ---
        if (perfectGlow > 0f)
        {
            float visualCurve = MathF.Pow(perfectGlow, 1.5f);

            Raylib.BeginBlendMode(BlendMode.Additive);

            // Render glowing silver dust
            foreach (var dust in dustParticles)
            {
                float lifeRatio = dust.Life;
                if (lifeRatio > 1f) lifeRatio = 1f;

                Color dustColor = new(200, 230, 255, (int)(140 * lifeRatio * visualCurve));
                Raylib.DrawCircleV(dust.Position, dust.Size, dustColor);
            }

            // Draw a soft, expanding blue-silver aura around the orb
            Color glowCore = new(200, 230, 255, (int)(25 * visualCurve));
            Color glowOuter = new(150, 200, 255, (int)(8 * visualCurve));

            float glowRadiusBoost = MathF.Sin(time * 5f) * 4f;

            Raylib.DrawCircleV(renderPos, baseRadius * 1.5f + glowRadiusBoost, glowCore);
            Raylib.DrawCircleV(renderPos, baseRadius * 2.2f + glowRadiusBoost, glowOuter);

            Raylib.EndBlendMode();
        }

        // --- DRAW MAIN ORB ---
        coreColor.A = 200;
        Raylib.DrawCircleV(renderPos, baseRadius * 0.4f, coreColor);

        coreColor.A = 80;
        Raylib.DrawCircleV(renderPos, baseRadius * 0.7f, coreColor);

        coreColor.A = 30;
        Raylib.DrawCircleV(renderPos, baseRadius * (1.1f + audioPulse * 0.5f), coreColor);

        membraneColor.A = 150;
        Raylib.DrawPolyLinesEx(renderPos, 40, currentMembraneRadius, 0f, 2.0f, membraneColor);
    }
}