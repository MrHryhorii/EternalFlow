using Raylib_cs;
using System.Numerics;
using EternalFlow.Core;

namespace EternalFlow.Scenes;

public class SettingsScene : Scene
{
    private int selectedOption = 0;

    private readonly string[] menuOptions = ["Роздільна здатність", "На весь екран", "Гучність музики", "Назад"];

    // --- РОБИМО МАСИВ ТА ІНДЕКС СТАТИЧНИМИ ---
    // Тепер вони не будуть стиратися при виході в головне меню
    private static readonly (int width, int height)[] resolutions = [
        (1024, 768),   // 4:3
        (1280, 720),   // 16:9
        (1280, 800),   // 16:10
        (1440, 900),   // 16:10
        (1600, 900),   // 16:9
        (1680, 1050),  // 16:10
        (1920, 1080),  // 16:9
        (1920, 1200),  // 16:10
        (2560, 1440),  // 16:9
        (2560, 1600)   // 16:10
    ];

    // Значення -1 означає, що гра щойно запущена і ми ще не зберігали вибір
    private static int currentResIndex = -1;

    private bool isFullscreen;

    // --- Елементи для живого фону ---
    private readonly FloatingShapes backgroundShapes;
    private readonly ColorManager colorManager;
    private readonly PathGenerator dummyPath;

    public SettingsScene(Game game, Font font) : base(game, font)
    {
        isFullscreen = Raylib.IsWindowFullscreen();

        // Знаходимо поточний індекс ТІЛЬКИ при першому вході в налаштування
        if (currentResIndex == -1)
        {
            int currentW = Raylib.GetScreenWidth();
            int currentH = Raylib.GetScreenHeight();

            // За замовчуванням 1280x720 (індекс 1), якщо розмір екрана нестандартний
            currentResIndex = 1;

            for (int i = 0; i < resolutions.Length; i++)
            {
                if (resolutions[i].width == currentW && resolutions[i].height == currentH)
                {
                    currentResIndex = i;
                    break;
                }
            }
        }

        // Ініціалізуємо фон
        backgroundShapes = new FloatingShapes(Raylib.GetScreenWidth(), Raylib.GetScreenHeight(), 15);
        colorManager = new ColorManager();
        dummyPath = new PathGenerator();
    }

    public override void Update()
    {
        float deltaTime = Raylib.GetFrameTime();
        int screenHeight = Raylib.GetScreenHeight();

        // --- Плавне заспокоєння стресу (і музики) ---
        game.GlobalStress = Math.Max(0f, game.GlobalStress - deltaTime * 0.5f);

        // ОНОВЛЕННЯ ФОНУ (Стрес завжди 0)
        dummyPath.Update(0f, deltaTime);
        colorManager.Update(dummyPath, screenHeight, 0f, deltaTime);
        backgroundShapes.Update(deltaTime);

        // --- ЛОГІКА КЕРУВАННЯ ---

        // Вихід через ESC
        if (Raylib.IsKeyPressed(KeyboardKey.Escape))
        {
            game.ChangeScene(new MenuScene(game, font));
            return; // Виходимо з методу, щоб уникнути конфліктів
        }

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
        else if (selectedOption == 2) // Гучність музики
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Right) || Raylib.IsKeyPressed(KeyboardKey.D))
            {
                game.Audio.MasterVolume = Math.Min(1.0f, game.Audio.MasterVolume + 0.1f);
            }
            if (Raylib.IsKeyPressed(KeyboardKey.Left) || Raylib.IsKeyPressed(KeyboardKey.A))
            {
                game.Audio.MasterVolume = Math.Max(0.0f, game.Audio.MasterVolume - 0.1f);
            }
        }
        else if (selectedOption == 3) // Назад
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Enter) || Raylib.IsKeyPressed(KeyboardKey.Space))
            {
                game.ChangeScene(new MenuScene(game, font));
            }
        }
    }

    private void ApplyResolution()
    {
        if (Raylib.IsWindowFullscreen())
        {
            Raylib.ToggleFullscreen();
            isFullscreen = false;
        }

        int targetWidth = resolutions[currentResIndex].width;
        int targetHeight = resolutions[currentResIndex].height;

        Raylib.SetWindowSize(targetWidth, targetHeight);

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

        // МАЛЮЄМО ЖИВИЙ ФОН
        Raylib.ClearBackground(colorManager.BackgroundColor);
        backgroundShapes.Draw(colorManager.CurrentHue, colorManager.CurrentLightness, 0f);

        // ЗАГОЛОВОК
        Color titleColor = Color.Violet;
        titleColor.A = 220;
        DrawTextWithShadow("НАЛАШТУВАННЯ", new Vector2(280, 150), 72, 6, titleColor);

        // ПУНКТИ МЕНЮ
        for (int i = 0; i < menuOptions.Length; i++)
        {
            Color itemColor = (i == selectedOption) ? Color.Lime : new Color(200, 200, 200, 255);
            itemColor.A = (byte)((i == selectedOption) ? 220 : 160);

            float fontSize = (i == selectedOption) ? 48 : 40;
            Vector2 position = new(350, 300 + i * 70);

            string text = menuOptions[i];

            if (i == 0)
            {
                text += $":  < {resolutions[currentResIndex].width}x{resolutions[currentResIndex].height} >";
            }
            else if (i == 1)
            {
                text += isFullscreen ? ":  < ТАК >" : ":  < НІ >";
            }
            else if (i == 2)
            {
                int volumePercent = (int)Math.Round(game.Audio.MasterVolume * 100);
                text += $":  < {volumePercent}% >";
            }

            DrawTextWithShadow(text, position, fontSize, 2, itemColor);

            if (i == selectedOption)
            {
                Color cursorColor = Color.Lime;
                cursorColor.A = 220;
                DrawTextWithShadow("►", new Vector2(300, 300 + i * 70 + (fontSize == 48 ? 4 : 0)), fontSize, 2, cursorColor);
            }
        }

        // ПІДКАЗКА (БЕЗ ТІНІ)
        Color hintColor = Color.DarkGray;
        hintColor.A = 120;
        Raylib.DrawTextEx(font, "Вгору/Вниз: Вибір  |  Вліво/Вправо: Зміна  |  ESC: Назад", new Vector2(280, 600), 24, 2, hintColor);

        // ПЛАВНИЙ ПЕРЕХІД
        game.DrawTransitionOverlay();

        Raylib.EndDrawing();
    }

    private void DrawTextWithShadow(string text, Vector2 position, float fontSize, float spacing, Color textColor)
    {
        float shadowOffset = fontSize > 40 ? 3f : 2f;
        Color shadowColor = new(0, 0, 0, 70);

        Raylib.DrawTextEx(font, text, new Vector2(position.X + shadowOffset, position.Y + shadowOffset), fontSize, spacing, shadowColor);
        Raylib.DrawTextEx(font, text, position, fontSize, spacing, textColor);
    }
}