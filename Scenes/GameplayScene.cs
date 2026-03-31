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

    private readonly Player player = new(GetScreenWidth(), GetScreenHeight());
    private readonly PlayerController playerController = new();
    private readonly StressManager stressManager = new();

    private readonly ScoreManager scoreManager = new();

    private bool isPaused = false;
    private float pauseAlpha = 0f;
    private float timeScale = 1f;

    private float totalPlayTime = 0f;

    public override void Update()
    {
        if (IsKeyPressed(KeyboardKey.Escape))
        {
            int currentScore = (int)scoreManager.PeakScore;
            float perfectTime = scoreManager.PeakPerfectFlowTime;
            game.ChangeScene(new EndScene(game, font, currentScore, totalPlayTime, perfectTime));
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

        totalPlayTime += gameDeltaTime;

        int screenHeight = GetScreenHeight();

        playerController.Update(player, gameDeltaTime, screenHeight);
        stressManager.Update(player, path, screenHeight, gameDeltaTime);
        float currentStress = stressManager.CurrentStress;

        game.GlobalStress = currentStress;

        scoreManager.Update(currentStress, gameDeltaTime);

        if (scoreManager.IsGameOver)
        {
            int finalScore = (int)scoreManager.PeakScore;
            float perfectTime = scoreManager.PeakPerfectFlowTime;
            game.ChangeScene(new EndScene(game, font, finalScore, totalPlayTime, perfectTime));
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

        // ВИКОРИСТОВУЄМО ПАЛІТРУ
        DrawTextEx(font, "ВІЧНИЙ ПОТІК", new Vector2(20, 20), 40, 2, Palette.TextHint);

        int stressPercent = (int)(currentStress * 100);
        Color stressColor = ColorLerp(Palette.Highlight, Palette.Danger, currentStress);
        DrawTextEx(font, $"СТРЕС: {stressPercent}%", new Vector2(20, 70), 30, 2, stressColor);

        int score = (int)scoreManager.CurrentScore;
        string scoreText = $"{score}";
        Vector2 scoreSize = MeasureTextEx(font, scoreText, 40, 2);

        Vector2 scorePos = new(screenWidth - scoreSize.X - 20, 20);
        Color drawScoreColor = Palette.TextMain;

        if (currentStress > 0.75f)
        {
            float burnFactor = (currentStress - 0.75f) / 0.25f;

            drawScoreColor = ColorLerp(Palette.TextMain, Palette.Danger, burnFactor);

            int shakeIntensity = (int)(3f * burnFactor);
            if (shakeIntensity > 0)
            {
                scorePos.X += Random.Shared.Next(-shakeIntensity, shakeIntensity + 1);
                scorePos.Y += Random.Shared.Next(-shakeIntensity, shakeIntensity + 1);
            }
        }

        DrawTextEx(font, scoreText, new Vector2(scorePos.X + 3f, scorePos.Y + 3f), 40, 2, Palette.ShadowLight);
        DrawTextEx(font, scoreText, scorePos, 40, 2, drawScoreColor);

        float multiplier = scoreManager.CurrentMultiplier;
        string multText = $"x{multiplier:F1}";

        float multLerp = Math.Clamp((multiplier - 1f) / 4f, 0f, 1f);
        Color multColor = ColorLerp(Palette.TextSecondary, Palette.RecordScore, multLerp);

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

        if (multColor.A > 0)
        {
            Vector2 multSize = MeasureTextEx(font, multText, 30, 2);
            Vector2 multPos = new(screenWidth - multSize.X - 20, 65);

            int shadowAlpha = (int)(70f * (multColor.A / 255f));
            Color multShadowColor = new(0, 0, 0, shadowAlpha);

            DrawTextEx(font, multText, new Vector2(multPos.X + 2f, multPos.Y + 2f), 30, 2, multShadowColor);
            DrawTextEx(font, multText, multPos, 30, 2, multColor);
        }

        if (pauseAlpha > 0f)
        {
            // Використовуємо колір з палітри для оверлею
            Color overlayColor = Palette.OverlayDark;
            overlayColor.A = (byte)(overlayColor.A * pauseAlpha);
            DrawRectangle(0, 0, screenWidth, screenHeight, overlayColor);

            byte textAlpha = (byte)(255 * pauseAlpha);
            Color textColor = Palette.TextMain.WithAlpha(textAlpha);
            Color hintColor = Palette.TextSecondary.WithAlpha(textAlpha);

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

        game.DrawTransitionOverlay();

        EndDrawing();
    }

    private static Color ColorLerp(Color a, Color b, float t)
    {
        return new Color(
            (int)(a.R + (b.R - a.R) * t),
            (int)(a.G + (b.G - a.G) * t),
            (int)(a.B + (b.B - a.B) * t),
            255
        );
    }
}