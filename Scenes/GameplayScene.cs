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

    private bool isPaused = false;
    private float pauseAlpha = 0f; // Прозорість меню паузи (0 - невидиме, 1 - повністю видиме)
    private float timeScale = 1f;  // Масштаб часу (1 - норма, 0 - зупинка)

    public override void Update()
    {
        if (IsKeyPressed(KeyboardKey.Escape))
        {
            game.ChangeScene(new MenuScene(game, font));
            return;
        }

        // РЕАЛЬНИЙ час, який пройшов у реальному світі
        float realDeltaTime = GetFrameTime();

        if (IsKeyPressed(KeyboardKey.Space))
        {
            isPaused = !isPaused;
        }

        // ЛОГІКА ПЛАВНИХ ПЕРЕХОДІВ ПАУЗИ
        if (isPaused)
        {
            // Меню з'являється швидко (за ~0.25 сек)
            pauseAlpha = Math.Clamp(pauseAlpha + realDeltaTime * 4f, 0f, 1f);
            timeScale = 0f; // Час зупиняється миттєво
        }
        else
        {
            // Меню зникає ще швидше
            pauseAlpha = Math.Clamp(pauseAlpha - realDeltaTime * 6f, 0f, 1f);

            // ЧАС ПОВЕРТАЄТЬСЯ ПЛАВНО (Slow-motion ефект після паузи, займає ~1.5 сек)
            timeScale = Math.Clamp(timeScale + realDeltaTime * 0.6f, 0f, 1f);
        }

        // Якщо гра на паузі І меню вже повністю з'явилося - взагалі нічого не оновлюємо
        if (isPaused && timeScale == 0f) return;

        // ІГРОВИЙ ЧАС (Реальний час * масштаб). Якщо ми виходимо з паузи, час буде сповільненим!
        float gameDeltaTime = realDeltaTime * timeScale;
        int screenHeight = GetScreenHeight();

        // Передаємо СПОВІЛЬНЕНИЙ або ЗВИЧАЙНИЙ час усім системам гри
        playerController.Update(player, gameDeltaTime, screenHeight);
        stressManager.Update(player, path, screenHeight, gameDeltaTime);
        float currentStress = stressManager.CurrentStress;

        path.Update(currentStress, gameDeltaTime);
        colorManager.Update(path, screenHeight, currentStress, gameDeltaTime);
        backgroundShapes.Update(gameDeltaTime);
        player.Update(gameDeltaTime);
    }

    public override void Draw()
    {
        int screenWidth = GetScreenWidth();
        int screenHeight = GetScreenHeight();
        float time = (float)GetTime(); // GetTime() не залежить від timeScale, але це ок для шейдерів/фону
        float currentStress = stressManager.CurrentStress;

        BeginDrawing();
        ClearBackground(colorManager.BackgroundColor);

        backgroundShapes.Draw(colorManager.CurrentHue, colorManager.CurrentLightness, currentStress);
        path.Draw(screenWidth, screenHeight, colorManager.CurrentHue, currentStress);
        player.Draw();

        DrawTextEx(font, "ВІЧНИЙ ПОТІК", new Vector2(20, 20), 40, 2, Color.DarkGray);
        int stressPercent = (int)(currentStress * 100);
        Color stressColor = ColorLerp(Color.Lime, Color.Red, currentStress);
        DrawTextEx(font, $"СТРЕС: {stressPercent}%", new Vector2(20, 70), 30, 2, stressColor);

        // МАЛЮЄМО ПАУЗУ З УРАХУВАННЯМ ПРОЗОРОСТІ (pauseAlpha)
        if (pauseAlpha > 0f)
        {
            // Рахуємо альфу одразу як int
            int overlayAlpha = (int)(120 * pauseAlpha);
            DrawRectangle(0, 0, screenWidth, screenHeight, new Color(0, 0, 0, overlayAlpha));

            // Прозорість тексту теж як int
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

        EndDrawing();
    }

    private static Color ColorLerp(Color a, Color b, float t)
    {
        return new Color(
            (byte)(a.R + (b.R - a.R) * t),
            (byte)(a.G + (b.G - a.G) * t),
            (byte)(a.B + (b.B - a.B) * t),
            (byte)255
        );
    }
}