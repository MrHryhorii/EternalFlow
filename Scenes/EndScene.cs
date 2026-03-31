using Raylib_cs;
using System.Numerics;

namespace EternalFlow.Scenes;

public class EndScene : Scene
{
    private readonly int peakScore;
    private readonly float timePlayed;
    private readonly float peakPerfectFlowTime; // Додали змінну
    private float alpha = 0f;

    // Оновлений конструктор
    public EndScene(Game game, Font font, int score, float time, float perfectFlowTime) : base(game, font)
    {
        peakScore = score;
        timePlayed = time;
        peakPerfectFlowTime = perfectFlowTime;

        // --- ЗБЕРІГАЄМО РЕКОРДИ ---
        if (peakScore > game.HighScore)
        {
            game.HighScore = peakScore;
        }

        // Зберігаємо рекорд ідеального потоку
        if (peakPerfectFlowTime > game.BestPerfectFlowTime)
        {
            game.BestPerfectFlowTime = peakPerfectFlowTime;
        }
    }

    public override void Update()
    {
        float deltaTime = Raylib.GetFrameTime();
        alpha = Math.Clamp(alpha + deltaTime * 0.5f, 0f, 1f);

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
        Raylib.ClearBackground(new Color(10, 10, 10, 255));

        int screenWidth = Raylib.GetScreenWidth();
        int screenHeight = Raylib.GetScreenHeight();

        int textAlpha = (int)(255 * alpha);
        Color textColor = new(255, 255, 255, textAlpha);
        Color accentColor = new(180, 180, 180, textAlpha);
        Color goldColor = new(255, 215, 0, textAlpha);
        Color cyanColor = new(0, 255, 255, textAlpha); // Колір для часу потоку

        string title = "ПОТІК ПЕРЕРВАНО";
        Vector2 titleSize = Raylib.MeasureTextEx(font, title, 60, 2);
        Raylib.DrawTextEx(font, title, new Vector2((screenWidth - titleSize.X) / 2, screenHeight * 0.2f), 60, 2, textColor);

        TimeSpan time = TimeSpan.FromSeconds(timePlayed);
        string timeString = string.Format("{0:D2}:{1:D2}", time.Minutes, time.Seconds);

        // Форматуємо час ідеального потоку
        TimeSpan perfectTime = TimeSpan.FromSeconds(peakPerfectFlowTime);
        string perfectTimeString = string.Format("{0:D2}:{1:D2}", perfectTime.Minutes, perfectTime.Seconds);

        // Статистика (змістив трохи вгору, щоб влізло)
        DrawCenteredText($"Час у грі: {timeString}", screenHeight * 0.40f, 35, accentColor, screenWidth);
        DrawCenteredText($"Піковий рахунок: {peakScore}", screenHeight * 0.50f, 50, goldColor, screenWidth);
        DrawCenteredText($"Ідеальний потік: {perfectTimeString}", screenHeight * 0.60f, 35, cyanColor, screenWidth);

        if (alpha >= 1f)
        {
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