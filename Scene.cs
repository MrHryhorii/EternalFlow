using Raylib_cs;

namespace EternalFlow;

public abstract class Scene(Game game, Font font)
{
    protected Game game = game;
    protected Font font = font;

    public abstract void Update();
    public abstract void Draw();
    public virtual void Unload() { }
}