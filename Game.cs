using Raylib_cs;
using EternalFlow.Scenes;
using EternalFlow.Core;

namespace EternalFlow;

/// <summary>
/// The core manager of the application. 
/// Holds global state (high scores, audio, stress level) and handles smooth transitions between scenes.
/// </summary>
public class Game
{
    private Scene? currentScene;
    private Scene? nextScene;
    private Font globalFont;

    // Transition state variables
    private bool isTransitioning = false;
    private bool isFadingOut = false;
    private float transitionAlpha = 0f;
    private const float TRANSITION_SPEED = 2.5f;

    // Global persistence variables (kept alive across multiple gameplay sessions)
    public int HighScore { get; set; } = 0;
    public float BestPerfectFlowTime { get; set; } = 0f;

    // Global audio manager and overall game stress level
    public AudioManager Audio { get; private set; }
    public float GlobalStress { get; set; } = 0f;

    public Game(Font font)
    {
        globalFont = font;

        // Initialize audio system as soon as the game starts
        Audio = new AudioManager();

        // Set the initial scene to the Main Menu
        currentScene = new MenuScene(this, globalFont);
    }

    public void Update()
    {
        float deltaTime = Raylib.GetFrameTime();

        // Audio needs to be updated globally every frame regardless of the active scene
        Audio.Update(GlobalStress, deltaTime);

        // Handle the smooth fade-in and fade-out scene transition logic
        if (isTransitioning)
        {
            if (isFadingOut)
            {
                // Increase darkness until the screen is fully black
                transitionAlpha += TRANSITION_SPEED * deltaTime;
                if (transitionAlpha >= 1f)
                {
                    transitionAlpha = 1f;

                    // Safely swap the scenes while the screen is black
                    currentScene?.Unload();
                    currentScene = nextScene;
                    nextScene = null;

                    // Start fading back in
                    isFadingOut = false;
                }
            }
            else
            {
                // Decrease darkness to reveal the new scene
                transitionAlpha -= TRANSITION_SPEED * deltaTime;
                if (transitionAlpha <= 0f)
                {
                    transitionAlpha = 0f;
                    isTransitioning = false; // Transition complete
                }
            }
        }

        // Only update the active scene logic if the screen is not fully black
        if (transitionAlpha < 1f)
        {
            currentScene?.Update();
        }
    }

    public void Draw()
    {
        // Let the current scene render its specific graphics
        currentScene?.Draw();
    }

    /// <summary>
    /// Renders a black rectangle over the entire screen during scene transitions.
    /// This should be called at the very end of the active scene's Draw method.
    /// </summary>
    public void DrawTransitionOverlay()
    {
        if (transitionAlpha > 0f)
        {
            int screenWidth = Raylib.GetScreenWidth();
            int screenHeight = Raylib.GetScreenHeight();

            int alpha = (int)Math.Clamp(transitionAlpha * 255f, 0, 255);
            Raylib.DrawRectangle(0, 0, screenWidth, screenHeight, new Color(0, 0, 0, alpha));
        }
    }

    /// <summary>
    /// Triggers a smooth transition to a new scene.
    /// </summary>
    public void ChangeScene(Scene newScene)
    {
        // Ignore scene change requests if a transition is already in progress
        if (isTransitioning) return;

        nextScene = newScene;
        isTransitioning = true;
        isFadingOut = true;
    }

    public Font GetFont() => globalFont;

    public void Unload()
    {
        // Free audio streams and hardware devices on exit
        Audio.Unload();
    }
}