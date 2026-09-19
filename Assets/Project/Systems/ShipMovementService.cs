using UnityEngine;

// Plain C# service: owns the ship's horizontal movement + Boost timers. Constructed once in GameBootstrapper.
public class ShipMovementService : IShipMovementService
{
    readonly Transform shipTransform;
    readonly ShipMovementData data;

    float moveAxis;
    float boostTimeRemaining;
    float boostCooldownRemaining;

    public ShipMovementService(Transform shipTransform, ShipMovementData data)
    {
        this.shipTransform = shipTransform;
        this.data = data;
    }

    public float BoostCooldownRemaining => boostCooldownRemaining;

    public void SetMoveAxis(float axis) => moveAxis = Mathf.Clamp(axis, -1f, 1f);

    public void TryBoost()
    {
        if (boostCooldownRemaining > 0f || boostTimeRemaining > 0f)
            return;

        boostTimeRemaining = data.boostDuration;
        boostCooldownRemaining = data.boostCooldown;
    }

    public void Tick(float deltaTime)
    {
        if (boostTimeRemaining > 0f)
            boostTimeRemaining = Mathf.Max(0f, boostTimeRemaining - deltaTime);
        if (boostCooldownRemaining > 0f)
            boostCooldownRemaining = Mathf.Max(0f, boostCooldownRemaining - deltaTime);

        float speed = data.moveSpeed * (boostTimeRemaining > 0f ? data.boostSpeedMultiplier : 1f);

        Vector3 position = shipTransform.position;
        position.x = Mathf.Clamp(position.x + moveAxis * speed * deltaTime, -data.playfieldHalfWidth, data.playfieldHalfWidth);
        shipTransform.position = position;
    }
}
