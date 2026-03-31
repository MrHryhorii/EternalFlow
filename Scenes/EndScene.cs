using Raylib_cs;
using System.Numerics;
using EternalFlow.Core;

namespace EternalFlow.Scenes;

public class EndScene : Scene
{
    private readonly int peakScore;
    private readonly float timePlayed;
    private readonly float peakPerfectFlowTime;
    private float alpha = 0f;

    public EndScene(Game game, Font font, int score, float time, float perfectFlowTime) : base(game, font)
    {
        peakScore = score;
        timePlayed = time;
        peakPerfectFlowTime = perfectFlowTime;

        if (peakScore > game.HighScore)
        {
            game.HighScore = peakScore;
        }

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

        // ВИКОРИСТОВУЄМО ПАЛІТРУ
        Raylib.ClearBackground(Palette.BackgroundDark);

        int screenWidth = Raylib.GetScreenWidth();
        int screenHeight = Raylib.GetScreenHeight();

        byte textAlpha = (byte)(255 * alpha);

        string title = "ПОТІК ПЕРЕРВАНО";
        Vector2 titleSize = Raylib.MeasureTextEx(font, title, 60, 2);
        Raylib.DrawTextEx(font, title, new Vector2((screenWidth - titleSize.X) / 2, screenHeight * 0.2f), 60, 2, Palette.TextMain.WithAlpha(textAlpha));

        TimeSpan time = TimeSpan.FromSeconds(timePlayed);
        string timeString = string.Format("{0:D2}:{1:D2}", time.Minutes, time.Seconds);

        TimeSpan perfectTime = TimeSpan.FromSeconds(peakPerfectFlowTime);
        string perfectTimeString = string.Format("{0:D2}:{1:D2}", perfectTime.Minutes, perfectTime.Seconds);

        DrawCenteredText($"Час у грі: {timeString}", screenHeight * 0.40f, 35, Palette.TextSecondary.WithAlpha(textAlpha), screenWidth);
        DrawCenteredText($"Піковий рахунок: {peakScore}", screenHeight * 0.50f, 50, Palette.RecordScore.WithAlpha(textAlpha), screenWidth);
        DrawCenteredText($"Ідеальний потік: {perfectTimeString}", screenHeight * 0.60f, 35, Palette.RecordFlow.WithAlpha(textAlpha), screenWidth);

        if (alpha >= 1f)
        {
            int pulse = (int)(Math.Sin(Raylib.GetTime() * 4f) * 127 + 128);
            Color hintColor = Palette.TextHint.WithAlpha((byte)pulse);

            DrawCenteredText("[ПРОБІЛ] - Повернутися в потік", screenHeight * 0.8f, 25, hintColor, screenWidth);
            DrawCenteredText("[ESC] - Головне меню", screenHeight * 0.85f, 20, Palette.TextSecondary.WithAlpha(255), screenWidth);
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