using Raylib_cs;

namespace EternalFlow.Core;

/// <summary>
/// Handles user input and applies smooth, "floaty" physics to the player orb.
/// The physics are designed to feel like gliding underwater or in zero gravity,
/// requiring the player to anticipate curves rather than making twitch movements.
/// </summary>
public class PlayerController
{
    // Physics parameters tuned for a relaxing, momentum-based feel
    private readonly float acceleration = 1200f;
    private readonly float maxSpeed = 700f;
    private readonly float drag = 3f;            // Low friction means the player glides significantly after releasing a key
    private readonly float bounceFactor = 0.6f;  // Retains 60% of vertical velocity when bouncing off the screen edges

    public void Update(Player player, float deltaTime, int screenHeight)
    {
        float moveInput = 0f;

        // Support both W/S and Up/Down arrow keys
        bool moveUp = Raylib.IsKeyDown(KeyboardKey.W) || Raylib.IsKeyDown(KeyboardKey.Up);
        bool moveDown = Raylib.IsKeyDown(KeyboardKey.S) || Raylib.IsKeyDown(KeyboardKey.Down);

        if (moveUp) moveInput -= 1f;
        if (moveDown) moveInput += 1f;

        // Apply input-driven acceleration
        if (moveInput != 0f)
        {
            player.VelocityY += moveInput * acceleration * deltaTime;
            player.VelocityY = Math.Clamp(player.VelocityY, -maxSpeed, maxSpeed);
        }
        else
        {
            // Apply drag to gradually decelerate when no keys are pressed
            player.VelocityY = Lerp(player.VelocityY, 0f, drag * deltaTime);
        }

        // Update physical position based on velocity
        player.Position.Y += player.VelocityY * deltaTime;

        // --- SCREEN BOUNDARY COLLISION ---
        float padding = 30f;

        // Bounce off the top edge
        if (player.Position.Y < padding)
        {
            player.Position.Y = padding;
            player.VelocityY = -player.VelocityY * bounceFactor;
        }
        // Bounce off the bottom edge
        else if (player.Position.Y > screenHeight - padding)
        {
            player.Position.Y = screenHeight - padding;
            player.VelocityY = -player.VelocityY * bounceFactor;
        }
    }

    private static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }
}