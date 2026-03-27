using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;
using System;
using EternalFlow.Core;

namespace EternalFlow.Scenes;

public class GameplayScene(Game game, Font font) : Scene(game, font)
{
    private readonly PathGenerator path = new PathGenerator();
    private readonly ColorManager colorManager = new ColorManager();
    private readonly FloatingShapes backgroundShapes = new FloatingShapes(GetScreenWidth(), GetScreenHeight(), 20);

    // ТЕСТОВИЙ РЕГУЛЯТОР БЕЗУМСТВА (від 0.0 до 1.0)
    private float mockStress = 0f;

    private readonly Player player = new(GetScreenHeight()); // Додали гравця

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

        float deltaTime = GetFrameTime();
        path.Update(mockStress);
        // Передаємо стрес у ColorManager
        colorManager.Update(path, GetScreenHeight(), mockStress);
        backgroundShapes.Update();

        player.Update(deltaTime);
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

        // МАЛЮЄМО ГРАВЦЯ (Зверху лінії)
        player.Draw();

        // Інтерфейс
        DrawTextEx(font, "ВІЧНИЙ ПОТІК", new Vector2(20, 20), 40, 2, Color.DarkGray);
        DrawTextEx(font, $"Стрес (Стрілки Вгору/Вниз): {mockStress:F2}", new Vector2(20, 70), 30, 2, mockStress > 0.5f ? Color.Red : Color.DarkGray);
        EndDrawing();
    }
}