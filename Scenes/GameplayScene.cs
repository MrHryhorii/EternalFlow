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

    // ТЕСТОВИЙ РЕГУЛЯТОР БЕЗУМСТВА (від 0.0 до 1.0)
    private float mockStress = 0f;

    private readonly Player player = new(GetScreenHeight()); // Додали гравця
    private readonly PlayerController playerController = new(); // Ініціалізуємо контролер
    private readonly StressManager stressManager = new();

    public override void Update()
    {
        if (IsKeyPressed(KeyboardKey.Escape))
        {
            game.ChangeScene(new MenuScene(game, font));
        }

        float deltaTime = GetFrameTime();
        int screenHeight = GetScreenHeight();

        /// КЕРУВАННЯ СТРЕСОМ (ТІЛЬКИ СТРІЛОЧКАМИ)
        if (IsKeyDown(KeyboardKey.Up)) mockStress += 0.5f * deltaTime;
        if (IsKeyDown(KeyboardKey.Down)) mockStress -= 0.5f * deltaTime;
        mockStress = Math.Clamp(mockStress, 0f, 1f);

        // КЕРУВАННЯ ГРАВЦЕМ (ТІЛЬКИ W та S)
        playerController.Update(player, deltaTime, screenHeight);

        stressManager.Update(player, path, screenHeight, deltaTime);

        path.Update(mockStress);
        colorManager.Update(path, screenHeight, mockStress);
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
        // Ручний стрес (стрілочки) - він зараз керує візуалом
        DrawTextEx(font, $"РУЧНИЙ СТРЕС (Взуал): {mockStress:F2}", new Vector2(20, 70), 30, 2, mockStress > 0.5f ? Color.Red : Color.DarkGray);
        // Новий розумний стрес (поки тільки цифра)
        DrawTextEx(font, $"РЕАЛЬНИЙ СТРЕС (Тест): {stressManager.CurrentStress:F2}", new Vector2(20, 110), 30, 2, stressManager.CurrentStress > 0.5f ? Color.Orange : Color.Lime);

        EndDrawing();
    }
}