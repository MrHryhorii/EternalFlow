using Raylib_cs;
using System;

namespace EternalFlow.Core;

public static class ColorConverter
{
    // L: Світлота (0.0 - 1.0)
    // C: Хрома / Насиченість (0.0 - 0.4 для звичайних екранів)
    // H: Відтінок у градусах (0 - 360)
    public static Color OklchToColor(float L, float C, float H)
    {
        // Переводимо градуси в радіани
        float hRad = H * MathF.PI / 180f;

        // Переводимо Oklch в базовий Oklab (L, a, b)
        float a = C * MathF.Cos(hRad);
        float b = C * MathF.Sin(hRad);

        // Перетворення Oklab у лінійний LMS
        float l_ = L + 0.3963377774f * a + 0.2158037573f * b;
        float m_ = L - 0.1055613458f * a - 0.0638541728f * b;
        float s_ = L - 0.0894841775f * a - 1.2914855480f * b;

        // Піднесення до куба
        float l = l_ * l_ * l_;
        float m = m_ * m_ * m_;
        float s = s_ * s_ * s_;

        // Перетворення лінійного LMS у лінійний RGB
        float rLin = +4.0767416621f * l - 3.3077115913f * m + 0.2309699292f * s;
        float gLin = -1.2684380046f * l + 2.6097574011f * m - 0.3413193965f * s;
        float bLin = -0.0041960863f * l - 0.7034186147f * m + 1.7076147010f * s;

        // Гамма-корекція (перехід від лінійного RGB до стандартного sRGB)
        float r = ApplySRGBGamma(rLin);
        float g = ApplySRGBGamma(gLin);
        float bColor = ApplySRGBGamma(bLin); // Назвали bColor, щоб не конфліктувало з b з Oklab

        // Затискаємо в межі 0-255 і повертаємо колір Raylib (використовуємо int)
        return new Color(
            (int)Math.Clamp(r * 255f, 0, 255),
            (int)Math.Clamp(g * 255f, 0, 255),
            (int)Math.Clamp(bColor * 255f, 0, 255),
            255
        );
    }

    private static float ApplySRGBGamma(float v)
    {
        if (v <= 0.0031308f) return 12.92f * v;
        return 1.055f * MathF.Pow(v, 1f / 2.4f) - 0.055f;
    }
}