using Raylib_cs;
using EternalFlow.Scenes;
using EternalFlow.Core;

namespace EternalFlow;

public class Game
{
    private Scene? currentScene;
    private Scene? nextScene;
    private Font globalFont;

    private bool isTransitioning = false;
    private bool isFadingOut = false;
    private float transitionAlpha = 0f;
    private const float TRANSITION_SPEED = 2.5f;

    public int HighScore { get; set; } = 0;
    public float BestPerfectFlowTime { get; set; } = 0f;

    // --- ГЛОБАЛЬНЕ АУДІО ТА СТРЕС ---
    public AudioManager Audio { get; private set; }
    public float GlobalStress { get; set; } = 0f;

    public Game(Font font)
    {
        globalFont = font;

        // Ініціалізуємо аудіо одразу при старті гри
        Audio = new AudioManager();

        currentScene = new MenuScene(this, globalFont);
    }

    public void Update()
    {
        float deltaTime = Raylib.GetFrameTime(); // Отримуємо час кадру один раз

        // --- Оновлюємо музику кожен кадр ---
        Audio.Update(GlobalStress, deltaTime);

        // ЛОГІКА ПЕРЕХОДУ
        if (isTransitioning)
        {
            if (isFadingOut)
            {
                transitionAlpha += TRANSITION_SPEED * deltaTime;
                if (transitionAlpha >= 1f)
                {
                    transitionAlpha = 1f;

                    currentScene?.Unload();
                    currentScene = nextScene;
                    nextScene = null;

                    isFadingOut = false;
                }
            }
            else
            {
                transitionAlpha -= TRANSITION_SPEED * deltaTime;
                if (transitionAlpha <= 0f)
                {
                    transitionAlpha = 0f;
                    isTransitioning = false;
                }
            }
        }

        // ОНОВЛЕННЯ СЦЕНИ
        if (transitionAlpha < 1f)
        {
            currentScene?.Update();
        }
    }

    public void Draw()
    {
        currentScene?.Draw();
    }

    public void DrawTransitionOverlay()
    {
        if (transitionAlpha > 0f)
        {
            int screenWidth = Raylib.GetScreenWidth();
            int screenHeight = Raylib.GetScreenHeight();
            int alpha = (int)Math.Clamp(transitionAlpha * 255f, 0, 255);
            Raylib.DrawRectangle(0, 0, screenWidth, screenHeight, new Color(0, 0, 0, alpha));
        }
    }

    public void ChangeScene(Scene newScene)
    {
        if (isTransitioning) return;

        nextScene = newScene;
        isTransitioning = true;
        isFadingOut = true;
    }

    public Font GetFont() => globalFont;

    // --- Звільняємо пам'ять при закритті ---
    public void Unload()
    {
        Audio.Unload();
    }
}