using Raylib_cs;
using System.Numerics;
using EternalFlow.Core;

namespace EternalFlow.Scenes;

public class MenuScene(Game game, Font font) : Scene(game, font)
{
    private int selectedOption = 0;
    private readonly string[] menuOptions = ["Грати", "Налаштування", "Вийти"];

    // Підключаємо наші ігрові системи для фону
    private readonly FloatingShapes backgroundShapes = new(Raylib.GetScreenWidth(), Raylib.GetScreenHeight(), 15);
    private readonly ColorManager colorManager = new();
    private readonly PathGenerator dummyPath = new();

    public override void Update()
    {
        // ОНОВЛЕННЯ ФОНУ (Стрес завжди 0)
        float deltaTime = Raylib.GetFrameTime();
        int screenHeight = Raylib.GetScreenHeight();

        dummyPath.Update(0f, deltaTime);
        colorManager.Update(dummyPath, screenHeight, 0f, deltaTime);
        backgroundShapes.Update(deltaTime);

        // ЛОГІКА МЕНЮ
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

        // МАЛЮЄМО ЖИВИЙ ФОН
        Raylib.ClearBackground(colorManager.BackgroundColor);
        backgroundShapes.Draw(colorManager.CurrentHue, colorManager.CurrentLightness, 0f); // stress = 0f

        // МАЛЮЄМО МЕНЮ З ТІНЯМИ
        // Допоміжний метод для тексту з тінню
        DrawTextWithShadow("ETERNAL FLOW", new Vector2(280, 150), 72, 6, Color.White);

        for (int i = 0; i < menuOptions.Length; i++)
        {
            Color color = (i == selectedOption) ? Color.Lime : new Color(220, 220, 220, 255);
            float fontSize = (i == selectedOption) ? 48 : 40;

            Vector2 position = new(500, 300 + i * 70);

            // Текст пунктів меню
            DrawTextWithShadow(menuOptions[i], position, fontSize, 2, color);

            // Курсор
            if (i == selectedOption)
            {
                DrawTextWithShadow("►", new Vector2(450, 300 + i * 70 + (fontSize == 48 ? 4 : 0)), fontSize, 2, Color.Lime);
            }
        }

        DrawTextWithShadow("Використовуй ↑ ↓ та ENTER", new Vector2(420, 580), 24, 2, Color.DarkGray);

        // Малюємо плавний перехід поверх усього
        game.DrawTransitionOverlay();

        Raylib.EndDrawing();
    }

    // НОВИЙ МЕТОД: Малює текст, підкладаючи під нього чорну тінь
    private void DrawTextWithShadow(string text, Vector2 position, float fontSize, float spacing, Color textColor)
    {
        // Зсув тіні. Для великого тексту робимо її трохи більшою
        float shadowOffset = fontSize > 40 ? 4f : 2f;
        Color shadowColor = new(0, 0, 0, 150); // Напівпрозорий чорний

        // Малюємо тінь (зсунуту вправо і вниз)
        Raylib.DrawTextEx(font, text, new Vector2(position.X + shadowOffset, position.Y + shadowOffset), fontSize, spacing, shadowColor);

        // Малюємо основний текст
        Raylib.DrawTextEx(font, text, position, fontSize, spacing, textColor);
    }
}