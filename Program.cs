using Raylib_cs;
using static Raylib_cs.Raylib;

namespace EternalFlow;

class Program
{
    static void Main(string[] args)
    {
        const int screenWidth = 1280;
        const int screenHeight = 720;

        InitWindow(screenWidth, screenHeight, "Eternal Flow - Вічний Потік");
        SetTargetFPS(60);

        // ВІДВ'ЯЗУЄМО клавішу Esc від автоматичного закриття гри!
        SetExitKey(KeyboardKey.Null);

        List<int> codepoints = [];
        for (int i = 32; i < 127; i++) codepoints.Add(i); // Латиниця + знаки
        for (int i = 1024; i < 1280; i++) codepoints.Add(i); // Кирилиця

        // Додаємо наші спецсимволи:
        codepoints.Add(9658); // ► (трикутник вибору)
        codepoints.Add(8593); // ↑ (стрілка вгору)
        codepoints.Add(8595); // ↓ (стрілка вниз)

        int[] fontChars = [.. codepoints];

        Font globalFont = LoadFontEx("fonts/DejaVuSans.ttf", 48, fontChars, fontChars.Length);

        if (globalFont.Texture.Id == 0)
        {
            Console.WriteLine("Помилка: Не вдалося завантажити шрифт!");
        }
        else
        {
            // Вмикаємо білінійну фільтрацію текстури шрифту для м'якого згладжування
            SetTextureFilter(globalFont.Texture, TextureFilter.Bilinear);
        }

        Game game = new(globalFont);

        while (!WindowShouldClose())
        {
            game.Update();
            game.Draw();
        }

        game.Unload();
        UnloadFont(globalFont);
        CloseWindow();
    }
}