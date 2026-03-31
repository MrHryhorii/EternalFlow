using Raylib_cs;

namespace EternalFlow.Core;

/// <summary>
/// A centralized color palette for all UI elements and text.
/// Uses soft, creamy neo-pastel colors to reduce eye strain and maintain a cohesive, calming art style.
/// </summary>
public static class Palette
{
    // --- MAIN TEXT COLORS ---
    // Warm, soft off-white for primary readable text to avoid the harsh glare of pure white
    public static readonly Color TextMain = new(245, 245, 240, 255);

    // Light grey-beige for inactive menu options and secondary information
    public static readonly Color TextSecondary = new(190, 190, 195, 255);

    // Semi-transparent color for non-intrusive hints and control instructions
    public static readonly Color TextHint = new(150, 150, 150, 120);

    // --- STATE AND HIGHLIGHT COLORS ---
    // Soft mint green used to indicate positive actions, active menu cursors, and low stress
    public static readonly Color Highlight = new(140, 240, 170, 255);

    // Soft lavender for primary titles and game branding
    public static readonly Color Accent = new(190, 150, 255, 255);

    // Creamy peach to warn the player of rising stress levels before things get critical
    public static readonly Color Warning = new(255, 180, 130, 255);

    // Soft coral/watermelon red for critical stress, errors, and the breaking of the flow
    public static readonly Color Danger = new(250, 110, 120, 255);

    // --- RECORD AND ACHIEVEMENT COLORS ---
    // Warm gold specifically reserved for High Score displays
    public static readonly Color RecordScore = new(250, 210, 110, 255);

    // Soft cyan/aqua specifically reserved for Perfect Flow Time displays
    public static readonly Color RecordFlow = new(120, 230, 250, 255);

    // --- SHADOWS AND OVERLAYS ---
    // Subtle drop shadow to detach floating text from the background
    public static readonly Color ShadowLight = new(0, 0, 0, 70);

    // Deep shadow for elements that need strong contrast to remain readable
    public static readonly Color ShadowDark = new(0, 0, 0, 150);

    // Dark transparent overlay used to dim the screen during the pause menu
    public static readonly Color OverlayDark = new(0, 0, 0, 120);

    // Deep, slightly bluish black for dead-state backgrounds (like the End Scene)
    public static readonly Color BackgroundDark = new(15, 15, 18, 255);

    /// <summary>
    /// Helper extension method to quickly create a copy of a palette color with a specific transparency (Alpha).
    /// </summary>
    public static Color WithAlpha(this Color color, byte alpha)
    {
        return new Color(color.R, color.G, color.B, alpha);
    }
}