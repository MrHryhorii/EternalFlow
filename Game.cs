using Raylib_cs;
using EternalFlow.Scenes;

namespace EternalFlow;

public class Game
{
    private Scene? currentScene;
    private Font globalFont;

    public Game(Font font)
    {
        globalFont = font;
        currentScene = new MenuScene(this, globalFont);
    }

    public void Update()
    {
        currentScene?.Update();
    }

    public void Draw()
    {
        // ТУТ БІЛЬШЕ НЕМАЄ BeginDrawing() та EndDrawing()
        currentScene?.Draw();
    }

    public void ChangeScene(Scene newScene)
    {
        currentScene?.Unload();
        currentScene = newScene;
    }

    public Font GetFont() => globalFont;
}