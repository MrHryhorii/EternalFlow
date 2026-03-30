using Raylib_cs;
using System.Numerics;

namespace EternalFlow.Scenes;

public class EndScene : Scene
{
    private readonly int peakScore;
    private readonly float timePlayed;
    private float alpha = 0f;

    // ВИКОРИСТОВУЄМО КЛАСИЧНИЙ КОНСТРУКТОР
    public EndScene(Game game, Font font, int score, float time) : base(game, font)
    {
        peakScore = score;
        timePlayed = time;

        // --- ЗБЕРІГАЄМО РЕКОРД ---
        if (peakScore > game.HighScore)
        {
            game.HighScore = peakScore;
        }
    }

    public override void Update()
    {
        float deltaTime = Raylib.GetFrameTime();
        alpha = Math.Clamp(alpha + deltaTime * 0.5f, 0f, 1f); // Плавна поява з темряви

        // --- Плавне заспокоєння стресу (і музики) ---
        game.GlobalStress = Math.Max(0f, game.GlobalStress - deltaTime * 0.5f);

        if (alpha >= 1f)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Space) || Raylib.IsKeyPressed(KeyboardKey.Enter))
            {
                game.ChangeScene(new GameplayScene(game, font));
            }
            else if (Raylib.IsKeyPressed(KeyboardKey.Escape))
            {
                game.ChangeScene(new MenuScene(game, font));
            }
        }
    }

    public override void Draw()
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(new Color(10, 10, 10, 255)); // Майже чорний фон

        int screenWidth = Raylib.GetScreenWidth();
        int screenHeight = Raylib.GetScreenHeight();

        // Прозорість для плавного проявлення тексту (використовуємо int для уникнення помилок)
        int textAlpha = (int)(255 * alpha);
        Color textColor = new(255, 255, 255, textAlpha);
        Color accentColor = new(180, 180, 180, textAlpha);
        Color goldColor = new(255, 215, 0, textAlpha);

        // Заголовок
        string title = "ПОТІК ПЕРЕРВАНО";
        Vector2 titleSize = Raylib.MeasureTextEx(font, title, 60, 2);
        Raylib.DrawTextEx(font, title, new Vector2((screenWidth - titleSize.X) / 2, screenHeight * 0.2f), 60, 2, textColor);

        // Форматуємо час (Хвилини:Секунди)
        TimeSpan time = TimeSpan.FromSeconds(timePlayed);
        string timeString = string.Format("{0:D2}:{1:D2}", time.Minutes, time.Seconds);

        // Статистика
        DrawCenteredText($"Час у грі: {timeString}", screenHeight * 0.45f, 35, accentColor, screenWidth);
        DrawCenteredText($"Піковий рахунок: {peakScore}", screenHeight * 0.55f, 50, goldColor, screenWidth);

        // Підказки
        if (alpha >= 1f)
        {
            // Використовуємо int для блимання, щоб уникнути помилок з конструктором Color
            int pulse = (int)(Math.Sin(Raylib.GetTime() * 4f) * 127 + 128);
            Color hintColor = new(150, 150, 150, pulse);

            DrawCenteredText("[ПРОБІЛ] - Повернутися в потік", screenHeight * 0.8f, 25, hintColor, screenWidth);
            DrawCenteredText("[ESC] - Головне меню", screenHeight * 0.85f, 20, accentColor, screenWidth);
        }

        game.DrawTransitionOverlay();
        Raylib.EndDrawing();
    }

    private void DrawCenteredText(string text, float y, float fontSize, Color color, int screenWidth)
    {
        Vector2 size = Raylib.MeasureTextEx(font, text, fontSize, 2);
        Raylib.DrawTextEx(font, text, new Vector2((screenWidth - size.X) / 2, y), fontSize, 2, color);
    }
}