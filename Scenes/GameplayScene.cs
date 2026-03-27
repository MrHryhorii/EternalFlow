using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;
using System;
using EternalFlow.Core;

namespace EternalFlow.Scenes;

public class GameplayScene : Scene
{
    private readonly PathGenerator path;
    private readonly ColorManager colorManager;
    private readonly FloatingShapes backgroundShapes;

    // ТЕСТОВИЙ РЕГУЛЯТОР БЕЗУМСТВА (від 0.0 до 1.0)
    private float mockStress = 0f;

    public GameplayScene(Game game, Font font) : base(game, font)
    {
        path = new PathGenerator();
        colorManager = new ColorManager();
        backgroundShapes = new FloatingShapes(GetScreenWidth(), GetScreenHeight(), 20);
    }

    public override void Update()
    {
        if (IsKeyPressed(KeyboardKey.Escape))
        {
            game.ChangeScene(new MenuScene(game, font));
        }

        // Керування стресом для тесту
        if (IsKeyDown(KeyboardKey.Up) || IsKeyDown(KeyboardKey.W)) mockStress += 0.5f * GetFrameTime();
        if (IsKeyDown(KeyboardKey.Down) || IsKeyDown(KeyboardKey.S)) mockStress -= 0.5f * GetFrameTime();
        mockStress = Math.Clamp(mockStress, 0f, 1f);

        path.Update(mockStress);
        colorManager.Update(path, GetScreenHeight());
        backgroundShapes.Update();
    }

    public override void Draw()
    {
        int screenWidth = GetScreenWidth();
        int screenHeight = GetScreenHeight();
        float time = (float)GetTime();

        BeginDrawing();
        ClearBackground(colorManager.BackgroundColor);

        // Малюємо фігури (вони самі знають, як мутувати при стресі)
        backgroundShapes.Draw(colorManager.CurrentHue, colorManager.CurrentLightness, mockStress, time);

        // Малюємо криву
        path.Draw(screenWidth, screenHeight, colorManager.CurrentHue, mockStress);

        // Інтерфейс
        DrawTextEx(font, "ВІЧНИЙ ПОТІК", new Vector2(20, 20), 40, 2, Color.DarkGray);
        DrawTextEx(font, $"Стрес (Стрілки Вгору/Вниз): {mockStress:F2}", new Vector2(20, 70), 30, 2, mockStress > 0.5f ? Color.Red : Color.DarkGray);
        EndDrawing();
    }
}