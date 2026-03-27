using System;
using Raylib_cs;

namespace EternalFlow.Core;

public class StressManager
{
    // Це наш фінальний "реальний" стрес, розрахований математикою
    public float CurrentStress { get; private set; } = 0f;

    public void Update(Player player, PathGenerator path, int screenHeight, float deltaTime)
    {
        // 1. ОТРИМУЄМО КООРДИНАТИ
        float pathY = path.GetPathY(player.Position.X, screenHeight); // Y маршруту прямо під гравцем
        float deltaY = pathY - player.Position.Y; // Відстань (позитивна, якщо лінія нижче гравця)
        float absDistance = Math.Abs(deltaY);

        // Нормалізуємо відстань (вважаємо, що половина екрана - це максимальне відхилення = 1.0)
        float maxDistance = screenHeight / 2f;
        float normalizedDist = Math.Clamp(absDistance / maxDistance, 0f, 1f);

        // 2. КУБІЧНА МАТЕМАТИКА (М'яка зона комфорту)
        // normalizedDist = 0.1 (близько) -> 0.1^3 = 0.001 (стрес майже нуль)
        // normalizedDist = 0.5 (середньо) -> 0.5^3 = 0.125 (починає тиснути)
        // normalizedDist = 0.9 (далеко) -> 0.9^3 = 0.729 (жорстке покарання)
        float targetStress = normalizedDist * normalizedDist * normalizedDist;

        // 3. БОНУС НАПРЯМКУ (Чи намагається гравець повернутися?)
        // Якщо напрямок швидкості збігається з напрямком до лінії, і ми рухаємося відчутно швидко
        bool isMovingTowardsPath = (Math.Sign(player.VelocityY) == Math.Sign(deltaY)) && Math.Abs(player.VelocityY) > 50f;

        // 4. ЕКСПОНЕНЦІАЛЬНИЙ ЧАС (В'язкість стресу)
        if (targetStress > CurrentStress)
        {
            // СТРЕС ЗРОСТАЄ
            // Зростає швидше, якщо ти дуже далеко від лінії
            float buildUpSpeed = 1.0f + (targetStress * 2.0f);
            CurrentStress += (targetStress - CurrentStress) * deltaTime * buildUpSpeed;
        }
        else
        {
            // СТРЕС ПАДАЄ (Гравець заспокоюється)
            float recoverySpeed = 0.4f; // Базове повільне заспокоєння (треба довго бути на лінії)

            // Бонус: якщо стрес уже високий (> 0.3), але гравець АКТИВНО летить до лінії
            if (CurrentStress > 0.3f && isMovingTowardsPath)
            {
                recoverySpeed = 2.0f; // Даємо різке полегшення (бонус за правильний ритм)
            }

            CurrentStress += (targetStress - CurrentStress) * deltaTime * recoverySpeed;
        }

        // Запобіжник
        CurrentStress = Math.Clamp(CurrentStress, 0f, 1f);
    }
}