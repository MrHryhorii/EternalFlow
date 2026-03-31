using Raylib_cs;
using System.Numerics;
using EternalFlow.Core;

namespace EternalFlow.Scenes;

/// <summary>
/// Allows the player to configure screen resolution, fullscreen mode, and master audio volume.
/// </summary>
public class SettingsScene : Scene
{
    private int selectedOption = 0;

    private readonly string[] menuOptions = ["Resolution", "Fullscreen", "Music Volume", "Back"];

    // Static list of supported resolutions
    private static readonly (int width, int height)[] resolutions = [
        (1024, 768),
        (1280, 720),
        (1280, 800),
        (1440, 900),
        (1600, 900),
        (1680, 1050),
        (1920, 1080),
        (1920, 1200),
        (2560, 1440),
        (2560, 1600)
    ];

    private static int currentResIndex = -1;
    private bool isFullscreen;

    private readonly FloatingShapes backgroundShapes;
    private readonly ColorManager colorManager;
    private readonly PathGenerator dummyPath;

    public SettingsScene(Game game, Font font) : base(game, font)
    {
        isFullscreen = Raylib.IsWindowFullscreen();

        // Detect the current resolution index upon entering the scene for the first time
        if (currentResIndex == -1)
        {
            int currentW = Raylib.GetScreenWidth();
            int currentH = Raylib.GetScreenHeight();

            currentResIndex = 1; // Default to 1280x720 if not found

            for (int i = 0; i < resolutions.Length; i++)
            {
                if (resolutions[i].width == currentW && resolutions[i].height == currentH)
                {
                    currentResIndex = i;
                    break;
                }
            }
        }

        backgroundShapes = new FloatingShapes(Raylib.GetScreenWidth(), Raylib.GetScreenHeight(), 15);
        colorManager = new ColorManager();
        dummyPath = new PathGenerator();
    }

    public override void Update()
    {
        float deltaTime = Raylib.GetFrameTime();
        int screenHeight = Raylib.GetScreenHeight();

        game.GlobalStress = Math.Max(0f, game.GlobalStress - deltaTime * 0.5f);

        dummyPath.Update(0f, deltaTime);
        colorManager.Update(dummyPath, screenHeight, 0f, deltaTime);
        backgroundShapes.Update(deltaTime);

        // Handle quick exit via Escape key
        if (Raylib.IsKeyPressed(KeyboardKey.Escape))
        {
            game.ChangeScene(new MenuScene(game, font));
            return;
        }

        // Handle vertical navigation
        if (Raylib.IsKeyPressed(KeyboardKey.Up) || Raylib.IsKeyPressed(KeyboardKey.W))
        {
            selectedOption = (selectedOption - 1 + menuOptions.Length) % menuOptions.Length;
        }
        if (Raylib.IsKeyPressed(KeyboardKey.Down) || Raylib.IsKeyPressed(KeyboardKey.S))
        {
            selectedOption = (selectedOption + 1) % menuOptions.Length;
        }

        // Handle setting adjustments based on the selected option
        if (selectedOption == 0) // Resolution
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Right) || Raylib.IsKeyPressed(KeyboardKey.D))
            {
                currentResIndex = (currentResIndex + 1) % resolutions.Length;
                ApplyResolution();
            }
            if (Raylib.IsKeyPressed(KeyboardKey.Left) || Raylib.IsKeyPressed(KeyboardKey.A))
            {
                currentResIndex = (currentResIndex - 1 + resolutions.Length) % resolutions.Length;
                ApplyResolution();
            }
        }
        else if (selectedOption == 1) // Fullscreen
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Enter) || Raylib.IsKeyPressed(KeyboardKey.Space) ||
                Raylib.IsKeyPressed(KeyboardKey.Right) || Raylib.IsKeyPressed(KeyboardKey.Left))
            {
                ToggleFullscreenMode();
            }
        }
        else if (selectedOption == 2) // Music Volume
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Right) || Raylib.IsKeyPressed(KeyboardKey.D))
            {
                game.Audio.MasterVolume = Math.Min(1.0f, game.Audio.MasterVolume + 0.1f);
            }
            if (Raylib.IsKeyPressed(KeyboardKey.Left) || Raylib.IsKeyPressed(KeyboardKey.A))
            {
                game.Audio.MasterVolume = Math.Max(0.0f, game.Audio.MasterVolume - 0.1f);
            }
        }
        else if (selectedOption == 3) // Back
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Enter) || Raylib.IsKeyPressed(KeyboardKey.Space))
            {
                game.ChangeScene(new MenuScene(game, font));
            }
        }
    }

    private void ApplyResolution()
    {
        // Exit fullscreen before changing resolution to avoid display glitches
        if (Raylib.IsWindowFullscreen())
        {
            Raylib.ToggleFullscreen();
            isFullscreen = false;
        }

        int targetWidth = resolutions[currentResIndex].width;
        int targetHeight = resolutions[currentResIndex].height;

        Raylib.SetWindowSize(targetWidth, targetHeight);

        // Center the window on the current monitor
        int monitor = Raylib.GetCurrentMonitor();
        int monW = Raylib.GetMonitorWidth(monitor);
        int monH = Raylib.GetMonitorHeight(monitor);

        Raylib.SetWindowPosition((monW - targetWidth) / 2, (monH - targetHeight) / 2);
    }

    private void ToggleFullscreenMode()
    {
        Raylib.ToggleFullscreen();
        isFullscreen = Raylib.IsWindowFullscreen();
    }

    public override void Draw()
    {
        Raylib.BeginDrawing();

        Raylib.ClearBackground(colorManager.BackgroundColor);
        backgroundShapes.Draw(colorManager.CurrentHue, colorManager.CurrentLightness, 0f);

        DrawTextWithShadow("SETTINGS", new Vector2(280, 150), 72, 6, Palette.Accent.WithAlpha(220));

        // Render menu options with current values appended
        for (int i = 0; i < menuOptions.Length; i++)
        {
            Color itemColor = (i == selectedOption) ? Palette.Highlight : Palette.TextSecondary;
            itemColor = itemColor.WithAlpha((byte)((i == selectedOption) ? 220 : 160));

            float fontSize = (i == selectedOption) ? 48 : 40;
            Vector2 position = new(350, 300 + i * 70);

            string text = menuOptions[i];

            if (i == 0)
            {
                text += $":  < {resolutions[currentResIndex].width}x{resolutions[currentResIndex].height} >";
            }
            else if (i == 1)
            {
                text += isFullscreen ? ":  < YES >" : ":  < NO >";
            }
            else if (i == 2)
            {
                int volumePercent = (int)Math.Round(game.Audio.MasterVolume * 100);
                text += $":  < {volumePercent}% >";
            }

            DrawTextWithShadow(text, position, fontSize, 2, itemColor);

            if (i == selectedOption)
            {
                DrawTextWithShadow("►", new Vector2(300, 300 + i * 70 + (fontSize == 48 ? 4 : 0)), fontSize, 2, Palette.Highlight.WithAlpha(220));
            }
        }

        Raylib.DrawTextEx(font, "Up/Down: Select  |  Left/Right: Change  |  ESC: Back", new Vector2(280, 600), 24, 2, Palette.TextHint);

        game.DrawTransitionOverlay();

        Raylib.EndDrawing();
    }

    private void DrawTextWithShadow(string text, Vector2 position, float fontSize, float spacing, Color textColor)
    {
        float shadowOffset = fontSize > 40 ? 3f : 2f;
        Raylib.DrawTextEx(font, text, new Vector2(position.X + shadowOffset, position.Y + shadowOffset), fontSize, spacing, Palette.ShadowLight);
        Raylib.DrawTextEx(font, text, position, fontSize, spacing, textColor);
    }
}