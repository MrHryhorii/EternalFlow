using Raylib_cs;
using System.Numerics;

namespace EternalFlow.Core;

public class FloatingShapes
{
    // Перелік можливих перетворень
    public enum MutationType
    {
        CrispGeometric, // Ідеально рівні фігури (трикутник, квадрат і т.д.)
        NervousPolygon, // Багатокутник, який вібрує і ламається
        SpikyStar       // Колюча зірка з гострими променями
    }

    private class FloatingShape
    {
        public Vector2 Position;
        public float Radius;
        public float Speed;
        public float HueOffset;

        public int TargetSides;       // 3 - трикутник, 4 - квадрат/ромб і т.д.
        public float Rotation;
        public float RotSpeed;
        public MutationType Mutation; // Тип мутації цієї конкретної фігури
    }

    private List<FloatingShape> shapes = new();

    public FloatingShapes(int screenWidth, int screenHeight, int shapeCount = 15)
    {
        for (int i = 0; i < shapeCount; i++)
        {
            shapes.Add(new FloatingShape
            {
                Position = new Vector2(Random.Shared.Next(0, screenWidth), Random.Shared.Next(0, screenHeight)),
                Radius = Random.Shared.Next(30, 120),
                Speed = Random.Shared.NextSingle() * 1.5f + 0.5f,
                HueOffset = Random.Shared.Next(-20, 20),
                TargetSides = Random.Shared.Next(3, 7), // Від трикутника до гексагона
                Rotation = Random.Shared.NextSingle() * MathF.PI * 2f,
                RotSpeed = (Random.Shared.NextSingle() - 0.5f) * 1.5f,

                // Випадковим чином призначаємо "ген" мутації
                Mutation = (MutationType)Random.Shared.Next(0, 3)
            });
        }
    }

    public void Update()
    {
        float deltaTime = Raylib.GetFrameTime();
        int screenWidth = Raylib.GetScreenWidth();
        int screenHeight = Raylib.GetScreenHeight();

        foreach (var shape in shapes)
        {
            shape.Position.X -= shape.Speed * 60f * deltaTime;
            shape.Rotation += shape.RotSpeed * deltaTime;

            if (shape.Position.X + shape.Radius * 2 < 0)
            {
                shape.Position.X = screenWidth + shape.Radius * 2;
                shape.Position.Y = Random.Shared.Next(0, screenHeight);
            }
        }
    }

    public void Draw(float baseHue, float baseLightness, float stress, float time)
    {
        foreach (var shape in shapes)
        {
            float toxicShift = Math.Sign(shape.HueOffset) * 150f * stress;
            float finalHue = (baseHue + shape.HueOffset + toxicShift) % 360f;
            if (finalHue < 0) finalHue += 360f;

            float chroma = 0.08f + (stress * 0.15f);
            float lightness = baseLightness - 0.06f - (stress * 0.15f);

            Color shapeColor = ColorConverter.OklchToColor(lightness, chroma, finalHue);
            shapeColor.A = (byte)(80 + stress * 120);

            DrawMorphingShape(shape, shapeColor, stress, time);
        }
    }

    private static void DrawMorphingShape(FloatingShape shape, Color color, float stress, float time)
    {
        int points = 60; // Достатньо для ідеальних прямих ліній та плавних кривих
        float angleStep = MathF.PI * 2f / points;

        for (int i = 0; i < points; i++)
        {
            float angle1 = i * angleStep;
            float angle2 = (i + 1) * angleStep;

            float r1 = GetMorphedRadius(angle1, shape, stress, time);
            float r2 = GetMorphedRadius(angle2, shape, stress, time);

            Vector2 p1 = new(
                shape.Position.X + MathF.Cos(angle1 + shape.Rotation) * r1,
                shape.Position.Y + MathF.Sin(angle1 + shape.Rotation) * r1
            );
            Vector2 p2 = new(
                shape.Position.X + MathF.Cos(angle2 + shape.Rotation) * r2,
                shape.Position.Y + MathF.Sin(angle2 + shape.Rotation) * r2
            );

            Raylib.DrawTriangle(shape.Position, p2, p1, color);
        }
    }

    private static float GetMorphedRadius(float angle, FloatingShape shape, float stress, float time)
    {
        // 1. БАЗА (СПОКІЙ): М'яка, дихаюча амеба
        float softNoise = MathF.Sin(angle * 3f + time * 1.5f + shape.HueOffset) * (shape.Radius * 0.08f);
        float blobRadius = shape.Radius + softNoise;

        // Математика для створення рівних граней багатокутника
        float segmentAngle = MathF.PI * 2f / shape.TargetSides;
        float halfSegment = segmentAngle / 2f;
        float localAngle = angle % segmentAngle;
        float polyRadius = shape.Radius * MathF.Cos(halfSegment) / MathF.Cos(localAngle - halfSegment);

        float stressRadius = blobRadius;

        // 2. МУТАЦІЯ (СТРЕС): Вибираємо форму залежно від типу фігури
        switch (shape.Mutation)
        {
            case MutationType.CrispGeometric:
                // Ідеально гострі трикутники, квадрати, ромби (ніякої вібрації)
                stressRadius = polyRadius;
                break;

            case MutationType.NervousPolygon:
                // Геометрія, яка зловісно вібрує по краях
                float harshFreq = shape.TargetSides * 2f;
                float harshNoise = MathF.Sin(angle * harshFreq - time * 10f) * (shape.Radius * 0.15f);
                stressRadius = polyRadius + harshNoise;
                break;

            case MutationType.SpikyStar:
                // Агресивна колючка з гострими променями
                float starNoise = MathF.Cos(angle * shape.TargetSides) * (shape.Radius * 0.5f);
                stressRadius = shape.Radius + starNoise;
                break;
        }

        // 3. ПЛАВНИЙ ПЕРЕХІД (Lerp) між спокоєм і стресом
        float t = stress * stress * (3f - 2f * stress); // SmoothStep
        return blobRadius + (stressRadius - blobRadius) * t;
    }
}