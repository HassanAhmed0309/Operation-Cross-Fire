// What PilotController calls each frame to move the ship and trigger Boost.
public interface IShipMovementService
{
    void SetMoveAxis(float axis);      // -1..1, read fresh every frame
    void TryBoost();                   // no-ops if still on cooldown or already boosting
    float BoostCooldownRemaining { get; }
    void Tick(float deltaTime);
}
