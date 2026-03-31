using Raylib_cs;
using static Raylib_cs.Raylib;

namespace EternalFlow;

class Program
{
    static void Main(string[] args)
    {
        const int screenWidth = 1280;
        const int screenHeight = 720;

        InitWindow(screenWidth, screenHeight, "Eternal Flow");
        SetTargetFPS(60);

        // Unbind the Escape key from automatically closing the game window.
        // We handle the Escape key manually inside our scenes to pause or go to the menu.
        SetExitKey(KeyboardKey.Null);

        // Prepare the list of characters our font needs to support
        List<int> codepoints = [];

        // Add basic Latin characters and standard punctuation
        for (int i = 32; i < 127; i++) codepoints.Add(i);

        // Add Cyrillic characters for multi-language support
        for (int i = 1024; i < 1280; i++) codepoints.Add(i);

        // Add custom Unicode symbols used in the UI
        codepoints.Add(9658); // Selection cursor (►)
        codepoints.Add(8593); // Up arrow hint (↑)
        codepoints.Add(8595); // Down arrow hint (↓)

        int[] fontChars = [.. codepoints];

        // Load the font with all the specified glyphs
        Font globalFont = LoadFontEx("fonts/DejaVuSans.ttf", 48, fontChars, fontChars.Length);

        if (globalFont.Texture.Id == 0)
        {
            Console.WriteLine("Error: Failed to load the global font!");
        }
        else
        {
            // Enable bilinear filtering to make scaled text look smooth instead of pixelated
            SetTextureFilter(globalFont.Texture, TextureFilter.Bilinear);
        }

        // Initialize the core game manager
        Game game = new(globalFont);

        // Main game loop
        while (!WindowShouldClose())
        {
            game.Update();
            game.Draw();
        }

        // Clean up unmanaged memory before exiting
        game.Unload();
        UnloadFont(globalFont);
        CloseWindow();
    }
}