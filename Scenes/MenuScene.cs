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

        // --- МАЛЮЄМО МЕНЮ ---

        // Заголовок (злегка прозорий)
        Color titleColor = Color.Violet;
        titleColor.A = 220; // Прозорість 220 з 255
        DrawTextWithShadow("ETERNAL FLOW", new Vector2(280, 150), 72, 6, titleColor);

        // --- НОВЕ: МАЛЮЄМО РЕКОРД (ЯКЩО ВІН Є) ---
        if (game.HighScore > 0)
        {
            Color goldColor = Color.Gold;
            goldColor.A = 220;
            string scoreText = $"НАЙКРАЩИЙ ПОТІК: {game.HighScore}";
            // Розміщуємо акуратно під заголовком
            DrawTextWithShadow(scoreText, new Vector2(280, 230), 30, 2, goldColor);
        }

        // Пункти меню
        for (int i = 0; i < menuOptions.Length; i++)
        {
            // Активний пункт яскравіший, неактивні - більш прозорі і сірі
            Color itemColor = (i == selectedOption) ? Color.Lime : new Color(200, 200, 200, 255);
            itemColor.A = (byte)((i == selectedOption) ? 220 : 160);

            float fontSize = (i == selectedOption) ? 48 : 40;
            Vector2 position = new(500, 300 + i * 70);

            DrawTextWithShadow(menuOptions[i], position, fontSize, 2, itemColor);

            if (i == selectedOption)
            {
                Color cursorColor = Color.Lime;
                cursorColor.A = 220;
                DrawTextWithShadow("►", new Vector2(450, 300 + i * 70 + (fontSize == 48 ? 4 : 0)), fontSize, 2, cursorColor);
            }
        }

        // Підказка (БЕЗ ТІНІ, напівпрозора, щоб не кидалася в очі)
        Color hintColor = Color.DarkGray;
        hintColor.A = 120; // Робимо її дуже делікатною
        Raylib.DrawTextEx(font, "Використовуй ↑ ↓ та ENTER", new Vector2(420, 580), 24, 2, hintColor);

        game.DrawTransitionOverlay();
        Raylib.EndDrawing();
    }

    private void DrawTextWithShadow(string text, Vector2 position, float fontSize, float spacing, Color textColor)
    {
        float shadowOffset = fontSize > 40 ? 3f : 2f;

        // Робимо тінь значно прозорішою (було 150, стало 70)
        Color shadowColor = new(0, 0, 0, 70);

        Raylib.DrawTextEx(font, text, new Vector2(position.X + shadowOffset, position.Y + shadowOffset), fontSize, spacing, shadowColor);
        Raylib.DrawTextEx(font, text, position, fontSize, spacing, textColor);
    }
}