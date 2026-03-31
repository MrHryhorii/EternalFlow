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

        Color titleColor = Color.Violet;
        titleColor.A = 220;
        DrawTextWithShadow("ETERNAL FLOW", new Vector2(280, 150), 72, 6, titleColor);

        // --- МАЛЮЄМО РЕКОРДИ ---
        if (game.HighScore > 0)
        {
            Color goldColor = Color.Gold;
            goldColor.A = 220;
            string scoreText = $"НАЙКРАЩИЙ ПОТІК: {game.HighScore}";
            DrawTextWithShadow(scoreText, new Vector2(280, 230), 30, 2, goldColor);

            // Якщо є рекорд часу, малюємо його нижче
            if (game.BestPerfectFlowTime > 0)
            {
                Color cyanColor = Color.Lime;
                cyanColor.A = 220;
                TimeSpan time = TimeSpan.FromSeconds(game.BestPerfectFlowTime);
                string timeText = $"ІДЕАЛЬНИЙ ЧАС: {string.Format("{0:D2}:{1:D2}", time.Minutes, time.Seconds)}";
                DrawTextWithShadow(timeText, new Vector2(280, 265), 24, 2, cyanColor);
            }
        }

        for (int i = 0; i < menuOptions.Length; i++)
        {
            Color itemColor = (i == selectedOption) ? Color.Lime : new Color(200, 200, 200, 255);
            itemColor.A = (byte)((i == selectedOption) ? 220 : 160);

            float fontSize = (i == selectedOption) ? 48 : 40;
            Vector2 position = new(500, 320 + i * 70); // Трохи опустив меню, щоб влізли рекорди

            DrawTextWithShadow(menuOptions[i], position, fontSize, 2, itemColor);

            if (i == selectedOption)
            {
                Color cursorColor = Color.Lime;
                cursorColor.A = 220;
                DrawTextWithShadow("►", new Vector2(450, 320 + i * 70 + (fontSize == 48 ? 4 : 0)), fontSize, 2, cursorColor);
            }
        }

        Color hintColor = Color.DarkGray;
        hintColor.A = 120;
        Raylib.DrawTextEx(font, "Використовуй ↑ ↓ та ENTER", new Vector2(420, 600), 24, 2, hintColor);

        game.DrawTransitionOverlay();
        Raylib.EndDrawing();
    }

    private void DrawTextWithShadow(string text, Vector2 position, float fontSize, float spacing, Color textColor)
    {
        float shadowOffset = fontSize > 40 ? 3f : 2f;
        Color shadowColor = new(0, 0, 0, 70);

        Raylib.DrawTextEx(font, text, new Vector2(position.X + shadowOffset, position.Y + shadowOffset), fontSize, spacing, shadowColor);
        Raylib.DrawTextEx(font, text, position, fontSize, spacing, textColor);
    }
}