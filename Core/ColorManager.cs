using Raylib_cs;

namespace EternalFlow.Core;

/// <summary>
/// Dynamically manages the background color and the global hue of the game.
/// It synchronizes color palette shifts with the music's beat and dims the environment based on player stress.
/// </summary>
public class ColorManager
{
    // Boundaries for how long a specific color theme lasts before attempting to shift
    private const float MIN_PHASE_DURATION = 15f;
    private const float MAX_PHASE_DURATION = 30f;

    // Determines how much high stress shortens the wait time for the next color shift
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

    // Variables for detecting sudden spikes in audio volume (beats)
    private float previousAmplitude = 0f;
    private float beatCooldown = 0f;

    public ColorManager()
    {
        // Initialize with a random hue and high lightness for a bright start
        CurrentHue = Random.Shared.NextSingle() * 360f;
        CurrentLightness = 0.9f;
    }

    public void Update(PathGenerator path, int screenHeight, float stress, float deltaTime)
    {
        // --- BEAT DETECTION FOR SYNCHRONIZED TRANSITIONS ---
        // We read the global amplitude calculated by the AudioManager's DSP callback
        float currentAmp = AudioManager.RealtimeAmplitude;
        if (beatCooldown > 0) beatCooldown -= deltaTime;

        // A "beat" is defined as a sudden, significant jump in audio amplitude.
        // This effectively catches bass hits or the sudden start of a new track.
        bool isBeat = currentAmp > 0.3f && currentAmp > previousAmplitude + 0.05f && beatCooldown <= 0f;

        if (isBeat)
        {
            beatCooldown = 0.2f; // Prevent rapid-fire detections on sustained loud sounds
        }

        previousAmplitude = currentAmp;

        // --- HUE SHIFT LOGIC ---
        if (!isTransitioning)
        {
            // High stress accelerates the color timer, making the environment feel more restless
            float timeMultiplier = 1f / (1f - (stress * MAX_STRESS_TIME_REDUCTION));

            hueTimer -= deltaTime * timeMultiplier;
            if (hueTimer < 0) hueTimer = 0;

            // Trigger the transition ONLY if the timer is ready AND a beat is detected right now.
            // This ensures color shifts always feel musical and impactful.
            if (hueTimer <= 0 && isBeat)
            {
                isTransitioning = true;
                transitionProgress = 0f;
                startHue = CurrentHue;

                // Calculate a significant jump in hue to make the new color distinct
                float jumpDelta = Random.Shared.NextSingle() * 90f + 70f;
                if (Random.Shared.Next(2) == 0) jumpDelta = -jumpDelta;

                targetHue = startHue + jumpDelta;
                transitionDuration = Math.Abs(jumpDelta) / 90f;
            }
        }
        else
        {
            // Execute the smooth transition between the old and new hue
            transitionProgress += deltaTime / transitionDuration;

            if (transitionProgress >= 1f)
            {
                transitionProgress = 1f;
                isTransitioning = false;

                CurrentHue = targetHue % 360f;
                if (CurrentHue < 0) CurrentHue += 360f;

                // Reset the timer for the next phase
                hueTimer = Random.Shared.NextSingle() * (MAX_PHASE_DURATION - MIN_PHASE_DURATION) + MIN_PHASE_DURATION;
            }
            else
            {
                // Ease-In-Out interpolation for a natural color morph
                float t = transitionProgress * transitionProgress * (3f - 2f * transitionProgress);
                CurrentHue = startHue + (targetHue - startHue) * t;
            }
        }

        // --- DYNAMIC LIGHTNESS & STRESS IMPACT ---
        // The background slightly dims when the path moves lower on the screen, creating depth.
        float pathY = path.GetPathY(100, screenHeight);
        float normalizedY = Math.Clamp(pathY / screenHeight, 0f, 1f);

        float darkeningPower = 0.17f + (stress * 0.3f);
        float targetLightness = 0.92f - (normalizedY * darkeningPower);
        float targetChroma = 0.06f;

        float lightnessLerpSpeed = 0.15f;

        // Critical Stress Overide: Force the environment to fade into darkness
        if (stress > 0.75f)
        {
            float burnFactor = (stress - 0.75f) / 0.25f;

            targetLightness *= 1f - burnFactor;
            targetChroma *= 1f - burnFactor;

            // Increase interpolation speed to make the blackout feel sudden and dangerous
            lightnessLerpSpeed = 2.0f + (burnFactor * 5f);
        }

        CurrentLightness += (targetLightness - CurrentLightness) * deltaTime * lightnessLerpSpeed;

        float finalHue = CurrentHue % 360f;
        if (finalHue < 0) finalHue += 360f;

        // Convert the calculated OKLCH values into the final Raylib background color
        BackgroundColor = ColorConverter.OklchToColor(CurrentLightness, targetChroma, finalHue);
    }
}