using Raylib_cs;

namespace EternalFlow.Core;

/// <summary>
/// Глобальна кольорова палітра для інтерфейсу та тексту.
/// Використовує "кремові" (пастельно-неонові) відтінки для приємного контрасту.
/// </summary>
public static class Palette
{
    // --- ОСНОВНІ КОЛЬОРИ ТЕКСТУ ---
    // Теплий, м'який білий
    public static readonly Color TextMain = new(245, 245, 240, 255);

    // Світлий сіро-бежевий для неактивних пунктів
    public static readonly Color TextSecondary = new(190, 190, 195, 255);

    // Напівпрозорий для підказок
    public static readonly Color TextHint = new(150, 150, 150, 120);

    // --- КОЛЬОРИ СТАНІВ ТА ВИДІЛЕННЯ ---
    // Кремовий м'ятний
    public static readonly Color Highlight = new(140, 240, 170, 255);

    // М'який лавандовий
    public static readonly Color Accent = new(190, 150, 255, 255);

    // Кремовий персиковий
    public static readonly Color Warning = new(255, 180, 130, 255);

    // М'який кораловий/кавуновий
    public static readonly Color Danger = new(250, 110, 120, 255);

    // --- КОЛЬОРИ РЕКОРДІВ ТА ДОСЯГНЕНЬ ---
    // М'яке, тепле золото
    public static readonly Color RecordScore = new(250, 210, 110, 255);

    // Кремовий аква/ціан
    public static readonly Color RecordFlow = new(120, 230, 250, 255);

    // --- ТІНІ ТА ЗАТЕМНЕННЯ ---
    public static readonly Color ShadowLight = new(0, 0, 0, 70);         // Легка тінь тексту
    public static readonly Color ShadowDark = new(0, 0, 0, 150);         // Глибока тінь
    public static readonly Color OverlayDark = new(0, 0, 0, 120);        // Напівпрозорий фон для паузи
    public static readonly Color BackgroundDark = new(15, 15, 18, 255);  // Глибокий, ледь синюватий чорний фон (EndScene)

    /// <summary>
    /// Допоміжний метод: повертає колір із заданою прозорістю (Alpha)
    /// </summary>
    public static Color WithAlpha(this Color color, byte alpha)
    {
        return new Color(color.R, color.G, color.B, alpha);
    }
}