using Raylib_cs;
using System.Numerics;

namespace EternalFlow.Core;

public class FloatingShapes
{
    // --- КОНСТАНТИ НАЛАШТУВАННЯ ---

    // Шанси кипіння (%) для різних стилів
    private const int CHANCE_TO_BOIL_STANDARD = 20;
    private const int CHANCE_TO_BOIL_ASYMMETRIC = 20;
    private const int CHANCE_TO_BOIL_SPIKY = 40;

    // Сила кипіння (амплітуда вібрації). 
    // 0.05f - легке тремтіння, 0.12f - стандарт, 0.25f - агресивний хаос.
    private const float BOIL_FORCE = 0.05f;

    private const int TOTAL_SHAPE_COUNT = 22;
    // ------------------------------------------

    private enum ShapeStyle
    {
        Standard,
        Asymmetric,
        Spiky
    }

    private class FloatingShape
    {
        public Vector2 Position;
        public float Radius;
        public float Speed;
        public float HueOffset;
        public int TargetSides;
        public float Rotation;
        public float RotSpeed;

        public ShapeStyle Style;
        public float AsymmetryFactor;
        public float BoilIntensity;
        public bool WillBoil;
    }

    private readonly List<FloatingShape> shapes = [];
    private float internalTime = 0f;

    public FloatingShapes(int screenWidth, int screenHeight, int shapeCount = TOTAL_SHAPE_COUNT)
    {
        for (int i = 0; i < shapeCount; i++)
        {
            var shape = new FloatingShape
            {
                // При створенні генеруємо випадкову позицію по всьому екрану
                Position = new Vector2(Random.Shared.Next(0, screenWidth), Random.Shared.Next(0, screenHeight))
            };

            ResetShapeParameters(shape);
            shapes.Add(shape);
        }
    }

    public void Update(float deltaTime)
    {
        internalTime += deltaTime;
        int screenWidth = Raylib.GetScreenWidth();
        int screenHeight = Raylib.GetScreenHeight();

        foreach (var shape in shapes)
        {
            shape.Position.X -= shape.Speed * 60f * deltaTime;
            shape.Rotation += shape.RotSpeed * deltaTime;

            // Якщо фігура вийшла за лівий край екрана
            if (shape.Position.X + shape.Radius * 2 < 0)
            {
                // Перероджуємо її властивості (нова форма, швидкість, колір тощо)
                ResetShapeParameters(shape);

                // Ставимо її за правий край екрана
                shape.Position.X = screenWidth + shape.Radius * 2;
                shape.Position.Y = Random.Shared.Next(0, screenHeight);
            }
        }
    }

    // Новий метод, який генерує "ДНК" фігури
    private static void ResetShapeParameters(FloatingShape shape)
    {
        int sides = Random.Shared.Next(3, 7);
        ShapeStyle style = (ShapeStyle)Random.Shared.Next(0, 3);

        int roll = Random.Shared.Next(100);
        bool shouldBoil = style switch
        {
            ShapeStyle.Standard => roll < CHANCE_TO_BOIL_STANDARD,
            ShapeStyle.Asymmetric => roll < CHANCE_TO_BOIL_ASYMMETRIC,
            ShapeStyle.Spiky => roll < CHANCE_TO_BOIL_SPIKY,
            _ => false
        };

        shape.Radius = Random.Shared.Next(35, 85);
        shape.Speed = Random.Shared.NextSingle() * 1.0f + 0.5f;
        shape.HueOffset = Random.Shared.Next(-20, 20);
        shape.TargetSides = sides;
        shape.Rotation = Random.Shared.NextSingle() * MathF.PI * 2f;
        shape.RotSpeed = (Random.Shared.NextSingle() - 0.5f) * 1.2f;
        shape.Style = style;
        shape.AsymmetryFactor = Random.Shared.NextSingle() * 0.4f + 0.4f;
        shape.BoilIntensity = Random.Shared.NextSingle() * 5f + 10f;
        shape.WillBoil = shouldBoil;
    }

    public void Draw(float baseHue, float baseLightness, float stress)
    {
        foreach (var shape in shapes)
        {
            float toxicShift = Math.Sign(shape.HueOffset) * 150f * stress;
            float finalHue = (baseHue + shape.HueOffset + toxicShift) % 360f;
            if (finalHue < 0) finalHue += 360f;

            float chroma = 0.08f + (stress * 0.15f);
            float lightness = baseLightness - 0.06f - (stress * 0.15f);

            Color shapeColor = ColorConverter.OklchToColor(lightness, chroma, finalHue);
            shapeColor.A = (byte)(70 + stress * 130);

            DrawMorphingShape(shape, shapeColor, stress, internalTime);
        }
    }

    private static void DrawMorphingShape(FloatingShape shape, Color color, float stress, float time)
    {
        int points = 120;
        float angleStep = MathF.PI * 2f / points;

        for (int i = 0; i < points; i++)
        {
            float a1 = i * angleStep;
            float a2 = (i + 1) * angleStep;

            float r1 = GetMorphedRadius(a1, shape, stress, time);
            float r2 = GetMorphedRadius(a2, shape, stress, time);

            Vector2 p1 = new(
                shape.Position.X + MathF.Cos(a1 + shape.Rotation) * r1,
                shape.Position.Y + MathF.Sin(a1 + shape.Rotation) * r1
            );
            Vector2 p2 = new(
                shape.Position.X + MathF.Cos(a2 + shape.Rotation) * r2,
                shape.Position.Y + MathF.Sin(a2 + shape.Rotation) * r2
            );

            Raylib.DrawTriangle(shape.Position, p2, p1, color);
        }
    }

    private static float GetMorphedRadius(float angle, FloatingShape shape, float stress, float time)
    {
        float softBreathing = MathF.Sin(angle * 2f + time * 1.5f) * (shape.Radius * 0.05f);
        float baseR = shape.Radius + softBreathing;

        float targetR = baseR;

        switch (shape.Style)
        {
            case ShapeStyle.Standard:
                targetR = GetPolyRadius(angle, shape.TargetSides, shape.Radius);
                break;

            case ShapeStyle.Asymmetric:
                if (shape.TargetSides == 3)
                {
                    float triShift = MathF.Sin(angle) * shape.AsymmetryFactor * stress;
                    targetR = GetPolyRadius(angle + triShift, 3, shape.Radius);
                }
                else if (shape.TargetSides == 4)
                {
                    targetR = GetPolyRadius(angle, 4, shape.Radius);
                    targetR *= 1f + MathF.Abs(MathF.Cos(angle)) * (shape.AsymmetryFactor * 1.5f * stress);
                }
                else
                {
                    targetR = GetPolyRadius(angle, shape.TargetSides, shape.Radius);
                    targetR += MathF.Sin(angle * 2f) * (shape.Radius * 0.2f * stress);
                }
                break;

            case ShapeStyle.Spiky:
                float spikes = MathF.Cos(angle * (shape.TargetSides * 2)) * (shape.Radius * 0.5f);
                targetR = shape.Radius + spikes;
                break;
        }

        float transition = stress * stress * (3f - 2f * stress);
        float resultR = baseR + (targetR - baseR) * transition;

        if (shape.WillBoil)
        {
            float boilNoise = MathF.Sin(angle * shape.BoilIntensity + time * 20f) * (shape.Radius * BOIL_FORCE * stress);
            return resultR + boilNoise;
        }

        return resultR;
    }

    private static float GetPolyRadius(float angle, int sides, float radius)
    {
        float segment = MathF.PI * 2f / sides;
        return radius * MathF.Cos(MathF.PI / sides) / MathF.Cos((angle % segment) - (segment / 2f));
    }
}