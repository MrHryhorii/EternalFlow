using Raylib_cs;
using System.Numerics;
using EternalFlow.Core;

namespace EternalFlow.Scenes;

public class MenuScene : Scene
{
    private int selectedOption = 0;
    private readonly string[] menuOptions = ["Грати", "Налаштування", "Вийти"];

    private readonly FloatingShapes backgroundShapes;
    private readonly ColorManager colorManager;
    private readonly PathGenerator dummyPath;

    public MenuScene(Game game, Font font) : base(game, font)
    {
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

        if (Raylib.IsKeyPressed(KeyboardKey.Up) || Raylib.IsKeyPressed(KeyboardKey.W))
        {
            selectedOption = (selectedOption - 1 + menuOptions.Length) % menuOptions.Length;
        }
        if (Raylib.IsKeyPressed(KeyboardKey.Down) || Raylib.IsKeyPressed(KeyboardKey.S))
        {
            selectedOption = (selectedOption + 1) % menuOptions.Length;
        }

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

        // ВИКОРИСТОВУЄМО ПАЛІТРУ
        DrawTextWithShadow("ETERNAL FLOW", new Vector2(280, 150), 72, 6, Palette.Accent.WithAlpha(220));

        if (game.HighScore > 0)
        {
            string scoreText = $"НАЙКРАЩИЙ ПОТІК: {game.HighScore}";
            DrawTextWithShadow(scoreText, new Vector2(280, 230), 30, 2, Palette.RecordScore.WithAlpha(220));

            if (game.BestPerfectFlowTime > 0)
            {
                TimeSpan time = TimeSpan.FromSeconds(game.BestPerfectFlowTime);
                string timeText = $"ІДЕАЛЬНИЙ ЧАС: {string.Format("{0:D2}:{1:D2}", time.Minutes, time.Seconds)}";
                DrawTextWithShadow(timeText, new Vector2(280, 265), 24, 2, Palette.RecordFlow.WithAlpha(220));
            }
        }

        for (int i = 0; i < menuOptions.Length; i++)
        {
            Color itemColor = (i == selectedOption) ? Palette.Highlight : Palette.TextSecondary;
            itemColor = itemColor.WithAlpha((byte)((i == selectedOption) ? 220 : 160));

            float fontSize = (i == selectedOption) ? 48 : 40;
            Vector2 position = new(500, 320 + i * 70);

            DrawTextWithShadow(menuOptions[i], position, fontSize, 2, itemColor);

            if (i == selectedOption)
            {
                DrawTextWithShadow("►", new Vector2(450, 320 + i * 70 + (fontSize == 48 ? 4 : 0)), fontSize, 2, Palette.Highlight.WithAlpha(220));
            }
        }

        Raylib.DrawTextEx(font, "Використовуй ↑ ↓ та ENTER", new Vector2(420, 600), 24, 2, Palette.TextHint);

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