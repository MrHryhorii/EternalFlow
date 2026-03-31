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

    // ПІДКЛЮЧИЛИ МЕНЕДЖЕР РАХУНКУ
    private readonly ScoreManager scoreManager = new();

    private bool isPaused = false;
    private float pauseAlpha = 0f;
    private float timeScale = 1f;

    // Змінна для загального часу у грі
    private float totalPlayTime = 0f;

    public override void Update()
    {
        if (IsKeyPressed(KeyboardKey.Escape))
        {
            game.ChangeScene(new MenuScene(game, font));
            return;
        }

        float realDeltaTime = GetFrameTime();

        if (IsKeyPressed(KeyboardKey.Space))
        {
            isPaused = !isPaused;
        }

        if (isPaused)
        {
            pauseAlpha = Math.Clamp(pauseAlpha + realDeltaTime * 4f, 0f, 1f);
            timeScale = 0f;
        }
        else
        {
            pauseAlpha = Math.Clamp(pauseAlpha - realDeltaTime * 6f, 0f, 1f);
            timeScale = Math.Clamp(timeScale + realDeltaTime * 0.6f, 0f, 1f);
        }

        if (isPaused && timeScale == 0f) return;

        float gameDeltaTime = realDeltaTime * timeScale;

        // Додаємо час гри (тільки коли не на паузі)
        totalPlayTime += gameDeltaTime;

        int screenHeight = GetScreenHeight();

        playerController.Update(player, gameDeltaTime, screenHeight);
        stressManager.Update(player, path, screenHeight, gameDeltaTime);
        float currentStress = stressManager.CurrentStress;

        game.GlobalStress = currentStress;

        // ОНОВЛЮЄМО РАХУНОК
        scoreManager.Update(currentStress, gameDeltaTime);

        // ПЕРЕВІРКА НА ПРОГРАШ
        if (scoreManager.IsGameOver)
        {
            // Передаємо ПІКОВИЙ рахунок і загальний час на екран результатів
            int finalScore = (int)scoreManager.PeakScore;
            game.ChangeScene(new EndScene(game, font, finalScore, totalPlayTime));
            return;
        }

        path.Update(currentStress, gameDeltaTime);
        colorManager.Update(path, screenHeight, currentStress, gameDeltaTime);
        backgroundShapes.Update(gameDeltaTime);
        player.Update(gameDeltaTime, currentStress);
    }

    public override void Draw()
    {
        int screenWidth = GetScreenWidth();
        int screenHeight = GetScreenHeight();
        float currentStress = stressManager.CurrentStress;

        BeginDrawing();
        ClearBackground(colorManager.BackgroundColor);

        backgroundShapes.Draw(colorManager.CurrentHue, colorManager.CurrentLightness, currentStress);
        path.Draw(screenWidth, screenHeight, colorManager.CurrentHue, currentStress);
        player.Draw(currentStress);

        // Лівий кут - інфо
        DrawTextEx(font, "ВІЧНИЙ ПОТІК", new Vector2(20, 20), 40, 2, Color.DarkGray);
        int stressPercent = (int)(currentStress * 100);
        Color stressColor = ColorLerp(Color.Lime, Color.Red, currentStress);
        DrawTextEx(font, $"СТРЕС: {stressPercent}%", new Vector2(20, 70), 30, 2, stressColor);

        // --- ПРАВИЙ КУТ - РАХУНОК ТА МНОЖНИК ---
        int score = (int)scoreManager.CurrentScore;
        string scoreText = $"{score}";
        Vector2 scoreSize = MeasureTextEx(font, scoreText, 40, 2);

        Vector2 scorePos = new(screenWidth - scoreSize.X - 20, 20);
        Color drawScoreColor = Color.White;

        // Поступове почервоніння та вібрація від 75% до 100%
        if (currentStress > 0.75f)
        {
            float burnFactor = (currentStress - 0.75f) / 0.25f; // Перетворюємо в діапазон 0.0 - 1.0

            drawScoreColor = ColorLerp(Color.White, Color.Red, burnFactor);

            int shakeIntensity = (int)(3f * burnFactor);
            if (shakeIntensity > 0)
            {
                // Вібруємо позицію
                scorePos.X += Random.Shared.Next(-shakeIntensity, shakeIntensity + 1);
                scorePos.Y += Random.Shared.Next(-shakeIntensity, shakeIntensity + 1);
            }
        }

        // Малюємо тінь для рахунку (зміщена на 3 пікселі, прозорість 70)
        Color shadowColor = new(0, 0, 0, 70);
        DrawTextEx(font, scoreText, new Vector2(scorePos.X + 3f, scorePos.Y + 3f), 40, 2, shadowColor);
        // Малюємо сам рахунок
        DrawTextEx(font, scoreText, scorePos, 40, 2, drawScoreColor);

        // --- МНОЖНИК ---
        float multiplier = scoreManager.CurrentMultiplier;
        string multText = $"x{multiplier:F1}";

        float multLerp = Math.Clamp((multiplier - 1f) / 4f, 0f, 1f);
        Color multColor = ColorLerp(Color.LightGray, Color.Gold, multLerp);

        // РАЗОМ З ПОЧЕРВОНІННЯМ РАХУНКУ - РОЗЧИНЯЄМО МНОЖНИК
        if (currentStress > 0.75f)
        {
            float burnFactor = (currentStress - 0.75f) / 0.25f;
            int alpha = 255 - (int)(255 * burnFactor * 2f);
            multColor.A = (byte)Math.Clamp(alpha, 0, 255);
        }
        else if (multiplier <= 1.01f && currentStress > 0.1f)
        {
            multColor.A = 100;
        }

        // Малюємо множник, ТІЛЬКИ якщо він хоч трохи видимий
        if (multColor.A > 0)
        {
            Vector2 multSize = MeasureTextEx(font, multText, 30, 2);
            Vector2 multPos = new(screenWidth - multSize.X - 20, 65);

            // Тінь для множника (її прозорість залежить від прозорості самого множника)
            // ВИПРАВЛЕННЯ: використовуємо int замість byte
            int shadowAlpha = (int)(70f * (multColor.A / 255f));
            Color multShadowColor = new(0, 0, 0, shadowAlpha);

            // Малюємо тінь множника
            DrawTextEx(font, multText, new Vector2(multPos.X + 2f, multPos.Y + 2f), 30, 2, multShadowColor);
            // Малюємо сам текст множника
            DrawTextEx(font, multText, multPos, 30, 2, multColor);
        }

        // Пауза
        if (pauseAlpha > 0f)
        {
            int overlayAlpha = (int)(120 * pauseAlpha);
            DrawRectangle(0, 0, screenWidth, screenHeight, new Color(0, 0, 0, overlayAlpha));

            int textAlpha = (int)(255 * pauseAlpha);
            Color textColor = new(255, 255, 255, textAlpha);
            Color hintColor = new(200, 200, 200, textAlpha);

            string pauseText = "ПАУЗА";
            float fontSize = 80f;
            Vector2 textSize = MeasureTextEx(font, pauseText, fontSize, 4f);
            Vector2 textPos = new((screenWidth - textSize.X) / 2f, (screenHeight - textSize.Y) / 2f);

            DrawTextEx(font, pauseText, textPos, fontSize, 4f, textColor);

            string hintText = "Натисніть ПРОБІЛ, щоб продовжити";
            float hintSize = 24f;
            Vector2 hintSizeVec = MeasureTextEx(font, hintText, hintSize, 2f);
            Vector2 hintPos = new((screenWidth - hintSizeVec.X) / 2f, textPos.Y + textSize.Y + 20f);

            DrawTextEx(font, hintText, hintPos, hintSize, 2f, hintColor);
        }

        // Перехід між сценами
        game.DrawTransitionOverlay();

        EndDrawing();
    }

    private static Color ColorLerp(Color a, Color b, float t)
    {
        // Всі змінні надійно конвертуються в int
        return new Color(
            (int)(a.R + (b.R - a.R) * t),
            (int)(a.G + (b.G - a.G) * t),
            (int)(a.B + (b.B - a.B) * t),
            255
        );
    }
}