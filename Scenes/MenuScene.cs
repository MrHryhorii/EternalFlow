using Raylib_cs;
using System.Numerics;
using EternalFlow.Core;

namespace EternalFlow.Scenes;

/// <summary>
/// The main entry point of the game, displaying options to start, change settings, or exit.
/// Renders a calm, stress-free background.
/// </summary>
public class MenuScene(Game game, Font font) : Scene(game, font)
{
    private int selectedOption = 0;
    private readonly string[] menuOptions = ["Play", "Settings", "Exit"];

    private readonly FloatingShapes backgroundShapes = new(Raylib.GetScreenWidth(), Raylib.GetScreenHeight(), 15);
    private readonly ColorManager colorManager = new();
    private readonly PathGenerator dummyPath = new();

    public override void Update()
    {
        float deltaTime = Raylib.GetFrameTime();
        int screenHeight = Raylib.GetScreenHeight();

        // Smoothly reduce any lingering global stress from a previous game session
        game.GlobalStress = Math.Max(0f, game.GlobalStress - deltaTime * 0.5f);

        // Update background elements with zero stress
        dummyPath.Update(0f, deltaTime);
        colorManager.Update(dummyPath, screenHeight, 0f, deltaTime);
        backgroundShapes.Update(deltaTime);

        // Handle menu navigation
        if (Raylib.IsKeyPressed(KeyboardKey.Up) || Raylib.IsKeyPressed(KeyboardKey.W))
        {
            selectedOption = (selectedOption - 1 + menuOptions.Length) % menuOptions.Length;
        }
        if (Raylib.IsKeyPressed(KeyboardKey.Down) || Raylib.IsKeyPressed(KeyboardKey.S))
        {
            selectedOption = (selectedOption + 1) % menuOptions.Length;
        }

        // Handle selection
        if (Raylib.IsKeyPressed(KeyboardKey.Enter) || Raylib.IsKeyPressed(KeyboardKey.Space))
        {
            switch (selectedOption)
            {
                case 0: game.ChangeScene(new GameplayScene(game, font)); break;
                case 1: game.ChangeScene(new SettingsScene(game, font)); break;
                case 2: Raylib.CloseWindow(); break;
            }
        }
    }

    public override void Draw()
    {
        Raylib.BeginDrawing();

        Raylib.ClearBackground(colorManager.BackgroundColor);
        backgroundShapes.Draw(colorManager.CurrentHue, colorManager.CurrentLightness, 0f);

        // Render main title
        DrawTextWithShadow("ETERNAL FLOW", new Vector2(280, 150), 72, 6, Palette.Accent.WithAlpha(220));

        // Display global records if they exist
        if (game.HighScore > 0)
        {
            string scoreText = $"BEST FLOW: {game.HighScore}";
            DrawTextWithShadow(scoreText, new Vector2(280, 230), 30, 2, Palette.RecordScore.WithAlpha(220));

            if (game.BestPerfectFlowTime > 0)
            {
                TimeSpan time = TimeSpan.FromSeconds(game.BestPerfectFlowTime);
                string timeText = $"PERFECT TIME: {string.Format("{0:D2}:{1:D2}", time.Minutes, time.Seconds)}";
                DrawTextWithShadow(timeText, new Vector2(280, 265), 24, 2, Palette.RecordFlow.WithAlpha(220));
            }
        }

        // Render menu options
        for (int i = 0; i < menuOptions.Length; i++)
        {
            Color itemColor = (i == selectedOption) ? Palette.Highlight : Palette.TextSecondary;
            itemColor = itemColor.WithAlpha((byte)((i == selectedOption) ? 220 : 160));

            float fontSize = (i == selectedOption) ? 48 : 40;
            Vector2 position = new(500, 320 + i * 70);

            DrawTextWithShadow(menuOptions[i], position, fontSize, 2, itemColor);

            // Draw selection cursor next to the active option
            if (i == selectedOption)
            {
                DrawTextWithShadow("►", new Vector2(450, 320 + i * 70 + (fontSize == 48 ? 4 : 0)), fontSize, 2, Palette.Highlight.WithAlpha(220));
            }
        }

        // Render control hints at the bottom
        Raylib.DrawTextEx(font, "Use ↑ ↓ and ENTER", new Vector2(420, 600), 24, 2, Palette.TextHint);

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