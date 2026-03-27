using Raylib_cs;
using System.Numerics;

namespace EternalFlow.Scenes;

public class SettingsScene : Scene
{
    private int selectedOption = 0;
    private readonly string[] menuOptions = ["Роздільна здатність", "На весь екран", "Назад"];

    // Варіанти роздільної здатності
    private readonly (int width, int height)[] resolutions = [
        (1280, 720),
        (1600, 900),
        (1920, 1080),
        (2560, 1440)
    ];

    private int currentResIndex = 0;
    private bool isFullscreen;

    public SettingsScene(Game game, Font font) : base(game, font)
    {
        isFullscreen = Raylib.IsWindowFullscreen();

        // Знаходимо поточну роздільну здатність при відкритті меню
        int currentW = Raylib.GetScreenWidth();
        int currentH = Raylib.GetScreenHeight();

        for (int i = 0; i < resolutions.Length; i++)
        {
            if (resolutions[i].width == currentW && resolutions[i].height == currentH)
            {
                currentResIndex = i;
                break;
            }
        }
    }

    public override void Update()
    {
        // Навігація вгору/вниз
        if (Raylib.IsKeyPressed(KeyboardKey.Up) || Raylib.IsKeyPressed(KeyboardKey.W))
        {
            selectedOption = (selectedOption - 1 + menuOptions.Length) % menuOptions.Length;
        }
        if (Raylib.IsKeyPressed(KeyboardKey.Down) || Raylib.IsKeyPressed(KeyboardKey.S))
        {
            selectedOption = (selectedOption + 1) % menuOptions.Length;
        }

        // Зміна значень (Вліво/Вправо/Enter)
        if (selectedOption == 0) // Роздільна здатність
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Right) || Raylib.IsKeyPressed(KeyboardKey.D))
            {
                currentResIndex = (currentResIndex + 1) % resolutions.Length;
                ApplyResolution();
            }
            if (Raylib.IsKeyPressed(KeyboardKey.Left) || Raylib.IsKeyPressed(KeyboardKey.A))
            {
                currentResIndex = (currentResIndex - 1 + resolutions.Length) % resolutions.Length;
                ApplyResolution();
            }
        }
        else if (selectedOption == 1) // На весь екран
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Enter) || Raylib.IsKeyPressed(KeyboardKey.Space) ||
                Raylib.IsKeyPressed(KeyboardKey.Right) || Raylib.IsKeyPressed(KeyboardKey.Left))
            {
                ToggleFullscreenMode();
            }
        }
        else if (selectedOption == 2) // Назад
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Enter) || Raylib.IsKeyPressed(KeyboardKey.Space))
            {
                game.ChangeScene(new MenuScene(game, font));
            }
        }
    }

    private void ApplyResolution()
    {
        // Якщо ми в повноекранному режимі, спочатку виходимо з нього
        if (Raylib.IsWindowFullscreen())
        {
            Raylib.ToggleFullscreen();
            isFullscreen = false;
        }

        int targetWidth = resolutions[currentResIndex].width;
        int targetHeight = resolutions[currentResIndex].height;

        Raylib.SetWindowSize(targetWidth, targetHeight);

        // Центруємо вікно після зміни розміру
        int monitor = Raylib.GetCurrentMonitor();
        int monW = Raylib.GetMonitorWidth(monitor);
        int monH = Raylib.GetMonitorHeight(monitor);

        Raylib.SetWindowPosition((monW - targetWidth) / 2, (monH - targetHeight) / 2);
    }

    private void ToggleFullscreenMode()
    {
        Raylib.ToggleFullscreen();
        isFullscreen = Raylib.IsWindowFullscreen();
    }

    public override void Draw()
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.Black);

        Raylib.DrawTextEx(font, "НАЛАШТУВАННЯ", new Vector2(280, 150), 72, 6, Color.Violet);

        for (int i = 0; i < menuOptions.Length; i++)
        {
            Color color = (i == selectedOption) ? Color.Lime : Color.White;
            float fontSize = (i == selectedOption) ? 48 : 40;
            Vector2 position = new(350, 300 + i * 70);

            string text = menuOptions[i];

            // Додаємо поточні значення до пунктів меню
            if (i == 0)
            {
                text += $":  < {resolutions[currentResIndex].width}x{resolutions[currentResIndex].height} >";
            }
            else if (i == 1)
            {
                text += isFullscreen ? ":  [ ТАК ]" : ":  [ НІ ]";
            }

            Raylib.DrawTextEx(font, text, position, fontSize, 2, color);

            if (i == selectedOption)
            {
                Raylib.DrawTextEx(font, "►", new Vector2(300, 300 + i * 70 + (fontSize == 48 ? 4 : 0)), fontSize, 2, Color.Lime);
            }
        }

        Raylib.DrawTextEx(font, "Вгору/Вниз: Вибір  |  Вліво/Вправо: Зміна", new Vector2(350, 600), 24, 2, Color.Gray);

        // Малюємо наш плавний перехід
        game.DrawTransitionOverlay();

        Raylib.EndDrawing();
    }
}