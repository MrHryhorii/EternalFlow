using Raylib_cs;
using EternalFlow.Scenes;

namespace EternalFlow;

public class Game
{
    private Scene? currentScene;
    private Scene? nextScene; // Сцена, на яку ми переходимо
    private Font globalFont;

    // --- Змінні для плавного переходу ---
    private bool isTransitioning = false;
    private bool isFadingOut = false; // true - темнішає (сцена зникає), false - світлішає (нова з'являється)
    private float transitionAlpha = 0f; // Від 0.0 (прозоро) до 1.0 (повністю чорний)
    private const float TRANSITION_SPEED = 2.5f; // Чим більше число, тим швидший перехід

    public Game(Font font)
    {
        globalFont = font;

        // При старті гри ми не робимо перехід, просто показуємо меню
        currentScene = new MenuScene(this, globalFont);
    }

    public void Update()
    {
        // ЛОГІКА ПЕРЕХОДУ
        if (isTransitioning)
        {
            float deltaTime = Raylib.GetFrameTime();

            if (isFadingOut)
            {
                // Екран темнішає
                transitionAlpha += TRANSITION_SPEED * deltaTime;
                if (transitionAlpha >= 1f)
                {
                    transitionAlpha = 1f;

                    // Як тільки екран став ПОВНІСТЮ ЧОРНИМ - міняємо сцену під ним
                    currentScene?.Unload();
                    currentScene = nextScene;
                    nextScene = null;

                    isFadingOut = false; // Починаємо світлішати, щоб показати нову сцену
                }
            }
            else
            {
                // Екран світлішає (показується нова сцена)
                transitionAlpha -= TRANSITION_SPEED * deltaTime;
                if (transitionAlpha <= 0f)
                {
                    transitionAlpha = 0f;
                    isTransitioning = false; // Перехід завершено повністю!
                }
            }
        }

        // ОНОВЛЕННЯ СЦЕНИ
        // Оновлюємо поточну сцену ТІЛЬКИ якщо екран не повністю чорний (alpha < 1.0)
        // Це гарантує, що гра не почнеться, поки гравець нічого не бачить
        if (transitionAlpha < 1f)
        {
            currentScene?.Update();
        }
    }

    public void Draw()
    {
        // Сцена сама викликає BeginDrawing та EndDrawing, і малює свої об'єкти
        currentScene?.Draw();
    }

    // НОВИЙ МЕТОД, який викликається з інших сцен для малювання затемнення
    public void DrawTransitionOverlay()
    {
        if (transitionAlpha > 0f)
        {
            int screenWidth = Raylib.GetScreenWidth();
            int screenHeight = Raylib.GetScreenHeight();

            // Переводимо float (0.0 - 1.0) у byte (0 - 255)
            int alpha = (int)Math.Clamp(transitionAlpha * 255f, 0, 255);

            // Малюємо чорний прямокутник на весь екран з прозорістю alpha
            Raylib.DrawRectangle(0, 0, screenWidth, screenHeight, new Color(0, 0, 0, alpha));
        }
    }

    public void ChangeScene(Scene newScene)
    {
        // Якщо вже йде перехід (наприклад, гравець швидко клацає Enter) - ігноруємо
        if (isTransitioning) return;

        // Запускаємо процес затемнення
        nextScene = newScene;
        isTransitioning = true;
        isFadingOut = true;
    }

    public Font GetFont() => globalFont;
}