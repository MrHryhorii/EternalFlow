using Raylib_cs;

namespace EternalFlow.Core;

public class PlayerController
{
    // Нові, набагато "м'якші" налаштування фізики:
    private readonly float acceleration = 1200f; // Було 2500. Тепер зривається з місця повільніше.
    private readonly float maxSpeed = 700f;      // Трохи збільшили максималку, щоб компенсувати повільний розгін.
    private readonly float drag = 3f;            // Було 8. Тепер ковзає значно довше (менше тертя).
    private readonly float bounceFactor = 0.6f;  // Пружність: зберігає 60% швидкості при відскоку від країв.

    public void Update(Player player, float deltaTime, int screenHeight)
    {
        float moveInput = 0f;

        // Об'єднуємо кнопки: або W, або Стрілка Вгору
        bool moveUp = Raylib.IsKeyDown(KeyboardKey.W) || Raylib.IsKeyDown(KeyboardKey.Up);
        bool moveDown = Raylib.IsKeyDown(KeyboardKey.S) || Raylib.IsKeyDown(KeyboardKey.Down);

        if (moveUp) moveInput -= 1f;
        if (moveDown) moveInput += 1f;

        // Застосовуємо фізику
        if (moveInput != 0f)
        {
            // Плавний розгін
            player.VelocityY += moveInput * acceleration * deltaTime;
            player.VelocityY = Math.Clamp(player.VelocityY, -maxSpeed, maxSpeed);
        }
        else
        {
            // Дуже тягуче гальмування
            player.VelocityY = Lerp(player.VelocityY, 0f, drag * deltaTime);
        }

        // Рухаємо гравця
        player.Position.Y += player.VelocityY * deltaTime;

        // ПРУЖНЕ ЗІТКНЕННЯ З КРАЯМИ ЕКРАНУ
        float padding = 30f; // Розмір мембрани
        if (player.Position.Y < padding)
        {
            player.Position.Y = padding; // Виштовхуємо, щоб не "прилипав"
            player.VelocityY = -player.VelocityY * bounceFactor; // Інвертуємо швидкість (відскок вниз)
        }
        else if (player.Position.Y > screenHeight - padding)
        {
            player.Position.Y = screenHeight - padding;
            player.VelocityY = -player.VelocityY * bounceFactor; // Відскок вгору
        }
    }

    private static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }
}