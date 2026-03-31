using Raylib_cs;

namespace EternalFlow.Core;

/// <summary>
/// Handles color space conversions. 
/// Specifically, it converts OKLCH (Lightness, Chroma, Hue) to standard sRGB used by Raylib.
/// OKLCH is perceptually uniform, meaning transitions between colors look natural to the human eye,
/// avoiding the harsh, muddy, or excessively bright bands often seen in standard HSL or HSV.
/// </summary>
public static class ColorConverter
{
    /// <summary>
    /// Converts an OKLCH color to a Raylib Color struct.
    /// L: Lightness (0.0 - 1.0, where 1.0 is pure white)
    /// C: Chroma / Saturation (0.0 - 0.4 is a safe range for standard monitors)
    /// H: Hue in degrees (0 - 360)
    /// </summary>
    public static Color OklchToColor(float L, float C, float H)
    {
        // Convert Hue from degrees to radians for trigonometric functions
        float hRad = H * MathF.PI / 180f;

        // Step 1: Convert from cylindrical OKLCH to Cartesian Oklab (L, a, b)
        float a = C * MathF.Cos(hRad);
        float b = C * MathF.Sin(hRad);

        // Step 2: Convert Oklab to linear LMS color space
        float l_ = L + 0.3963377774f * a + 0.2158037573f * b;
        float m_ = L - 0.1055613458f * a - 0.0638541728f * b;
        float s_ = L - 0.0894841775f * a - 1.2914855480f * b;

        // Cube the LMS values to revert the non-linear compression used in Oklab
        float l = l_ * l_ * l_;
        float m = m_ * m_ * m_;
        float s = s_ * s_ * s_;

        // Step 3: Convert linear LMS to linear RGB
        float rLin = +4.0767416621f * l - 3.3077115913f * m + 0.2309699292f * s;
        float gLin = -1.2684380046f * l + 2.6097574011f * m - 0.3413193965f * s;
        float bLin = -0.0041960863f * l - 0.7034186147f * m + 1.7076147010f * s;

        // Step 4: Apply Gamma correction to translate linear RGB to the standard sRGB color space
        float r = ApplySRGBGamma(rLin);
        float g = ApplySRGBGamma(gLin);
        float bColor = ApplySRGBGamma(bLin);

        // Clamp values to the 0-255 byte range required by Raylib
        return new Color(
            (int)Math.Clamp(r * 255f, 0, 255),
            (int)Math.Clamp(g * 255f, 0, 255),
            (int)Math.Clamp(bColor * 255f, 0, 255),
            255
        );
    }

    /// <summary>
    /// Applies the standard sRGB gamma curve. This ensures colors are displayed correctly
    /// on monitors, compensating for the non-linear way human eyes perceive brightness.
    /// </summary>
    private static float ApplySRGBGamma(float v)
    {
        if (v <= 0.0031308f) return 12.92f * v;
        return 1.055f * MathF.Pow(v, 1f / 2.4f) - 0.055f;
    }
}