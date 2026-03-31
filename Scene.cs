using Raylib_cs;

namespace EternalFlow;

/// <summary>
/// Abstract base class for all game screens (Menu, Gameplay, Settings, EndScreen).
/// Enforces a strict structure so the Game class can easily switch between them.
/// </summary>
public abstract class Scene(Game game, Font font)
{
    // Protected fields allow inherited scenes to access the main game state and global font
    protected Game game = game;
    protected Font font = font;

    // Handles game logic, math, and input processing
    public abstract void Update();

    // Handles all visual rendering (must be called between BeginDrawing and EndDrawing)
    public abstract void Draw();

    // Optional method to clean up resources (like textures or local sounds) when leaving the scene
    public virtual void Unload() { }
}