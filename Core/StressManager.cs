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

        // ЗМІНА 1: Звужуємо зону "смерті".
        // Тепер 1/3 висоти екрана (напр. 240 пікселів для 720p) - це вже 100% стресу.
        // AFK-гравець на великій хвилі гарантовано проб'є цю межу.
        float maxDistance = screenHeight / 3f;
        float normalizedDist = Math.Clamp(absDistance / maxDistance, 0f, 1f);

        // ЗМІНА 2: КВАДРАТИЧНА МАТЕМАТИКА замість кубічної.
        // normalizedDist = 0.1 (дуже близько) -> 0.1^2 = 0.01 (ідеально, росте множник)
        // normalizedDist = 0.5 (середньо)     -> 0.5^2 = 0.25 (стрес відчутно росте)
        // normalizedDist = 0.9 (далеко)       -> 0.9^2 = 0.81 (уже горять очки!)
        float targetStress = normalizedDist * normalizedDist;

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