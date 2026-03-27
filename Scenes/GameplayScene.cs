using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;
using EternalFlow.Core;

namespace EternalFlow.Scenes;

public class GameplayScene(Game game, Font font) : Scene(game, font)
{
    private readonly PathGenerator path = new();
    private readonly ColorManager colorManager = new();
    private readonly FloatingShapes backgroundShapes = new(GetScreenWidth(), GetScreenHeight(), 20);

    private readonly Player player = new(GetScreenHeight());
    private readonly PlayerController playerController = new();
    private readonly StressManager stressManager = new();

    public override void Update()
    {
        if (IsKeyPressed(KeyboardKey.Escape))
        {
            game.ChangeScene(new MenuScene(game, font));
        }

        float deltaTime = GetFrameTime();
        int screenHeight = GetScreenHeight();

        // 1. КЕРУВАННЯ ГРАВЦЕМ (W та S)
        playerController.Update(player, deltaTime, screenHeight);

        // 2. РОЗРАХУНОК РЕАЛЬНОГО СТРЕСУ
        stressManager.Update(player, path, screenHeight, deltaTime);
        float currentStress = stressManager.CurrentStress;

        // 3. ОНОВЛЕННЯ СВІТУ (Тепер усе залежить від реального стресу!)
        path.Update(currentStress);
        colorManager.Update(path, screenHeight, currentStress);
        backgroundShapes.Update();
        player.Update(deltaTime);
    }

    public override void Draw()
    {
        int screenWidth = GetScreenWidth();
        int screenHeight = GetScreenHeight();
        float time = (float)GetTime();

        // Беремо актуальний стрес для малювання
        float currentStress = stressManager.CurrentStress;

        BeginDrawing();
        ClearBackground(colorManager.BackgroundColor);

        // Передаємо реальний стрес усім візуальним елементам
        backgroundShapes.Draw(colorManager.CurrentHue, colorManager.CurrentLightness, currentStress, time);
        path.Draw(screenWidth, screenHeight, colorManager.CurrentHue, currentStress);

        // МАЛЮЄМО ГРАВЦЯ (Зверху лінії)
        player.Draw();

        // Інтерфейс
        DrawTextEx(font, "ВІЧНИЙ ПОТІК", new Vector2(20, 20), 40, 2, Color.DarkGray);

        // Красиве відображення реального стресу у відсотках
        int stressPercent = (int)(currentStress * 100);
        Color stressColor = ColorLerp(Color.Lime, Color.Red, currentStress);
        DrawTextEx(font, $"СТРЕС: {stressPercent}%", new Vector2(20, 70), 30, 2, stressColor);

        EndDrawing();
    }

    // Допоміжний метод для плавного переходу кольору тексту
    private static Color ColorLerp(Color a, Color b, float t)
    {
        return new Color(
            (byte)(a.R + (b.R - a.R) * t),
            (byte)(a.G + (b.G - a.G) * t),
            (byte)(a.B + (b.B - a.B) * t),
            (byte)255 // Виправили тут, додавши (byte)
        );
    }
}