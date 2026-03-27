using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;

namespace EternalFlow.Scenes;

public class MenuScene(Game game, Font font) : Scene(game, font)
{
    private int selectedOption = 0;
    private readonly string[] menuOptions = ["Грати", "Налаштування", "Вийти"];

    public override void Update()
    {
        if (IsKeyPressed(KeyboardKey.Up) || IsKeyPressed(KeyboardKey.W))
        {
            selectedOption = (selectedOption - 1 + menuOptions.Length) % menuOptions.Length;
        }
        if (IsKeyPressed(KeyboardKey.Down) || IsKeyPressed(KeyboardKey.S))
        {
            selectedOption = (selectedOption + 1) % menuOptions.Length;
        }

        if (IsKeyPressed(KeyboardKey.Enter) || IsKeyPressed(KeyboardKey.Space))
        {
            switch (selectedOption)
            {
                case 0: // Грати
                    game.ChangeScene(new GameplayScene(game, font));
                    break;
                case 1: // Налаштування
                    System.Console.WriteLine("Налаштування ще не реалізовані");
                    break;
                case 2: // Вийти
                    CloseWindow();
                    break;
            }
        }
    }

    public override void Draw()
    {
        // ВАЖЛИВО: Меню тепер самостійно відкриває полотно!
        BeginDrawing();
        ClearBackground(Color.Black);

        DrawTextEx(font, "ETERNAL FLOW", new Vector2(280, 150), 72, 6, Color.Violet);

        for (int i = 0; i < menuOptions.Length; i++)
        {
            Color color = (i == selectedOption) ? Color.Lime : Color.White;
            float fontSize = (i == selectedOption) ? 48 : 40;

            Vector2 position = new(500, 300 + i * 70);
            DrawTextEx(font, menuOptions[i], position, fontSize, 2, color);

            if (i == selectedOption)
            {
                DrawTextEx(font, "►", new Vector2(450, 300 + i * 70 + (fontSize == 48 ? 4 : 0)), fontSize, 2, Color.Lime);
            }
        }

        DrawTextEx(font, "Використовуй ↑ ↓ та ENTER", new Vector2(420, 580), 24, 2, Color.Gray);

        game.DrawTransitionOverlay();

        // ВАЖЛИВО: Меню самостійно закриває полотно!
        EndDrawing();
    }
}