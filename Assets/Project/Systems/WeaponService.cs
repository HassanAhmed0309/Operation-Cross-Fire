using UnityEngine;

// Plain C# service: cooldown-gates fire, pulls a laser from the pool and launches it toward the current aim point.
public class WeaponService : IWeaponService
{
    readonly Transform muzzle;
    readonly WeaponData data;
    readonly IObjectPool<PlayerLaser> pool;

    Vector3 aimWorldPosition;
    float cooldownRemaining;

    public WeaponService(Transform muzzle, WeaponData data, IObjectPool<PlayerLaser> pool)
    {
        this.muzzle = muzzle;
        this.data = data;
        this.pool = pool;
        aimWorldPosition = muzzle.position + Vector3.up;
    }

    public void SetAimWorldPosition(Vector3 worldPosition) => aimWorldPosition = worldPosition;

    public void TryFire()
    {
        if (cooldownRemaining > 0f)
            return;

        Vector2 direction = aimWorldPosition - muzzle.position;
        PlayerLaser laser = pool.Get();
        laser.Launch(muzzle.position, direction, data.laserSpeed, pool);

        cooldownRemaining = data.fireCooldown;
    }

    public void Tick(float deltaTime)
    {
        if (cooldownRemaining > 0f)
            cooldownRemaining = Mathf.Max(0f, cooldownRemaining - deltaTime);
    }
}
