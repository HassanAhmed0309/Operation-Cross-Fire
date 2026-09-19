using UnityEngine;

/// <summary>
/// Single entry point for service registration. Runs before other scripts' Awake so consumers
/// can safely resolve services in their own Start. Never `new` a service anywhere but here.
/// </summary>
[DefaultExecutionOrder(-1000)]
public class GameBootstrapper : MonoBehaviour
{
    [SerializeField] ShipInputRouter shipInputRouter;
    [SerializeField] RoleAssignment roleAssignment;

    [Header("Ship movement")]
    [SerializeField] Transform shipTransform;
    [SerializeField] ShipMovementData shipMovementData;

    [Header("Weapon")]
    [SerializeField] WeaponData weaponData;
    [SerializeField] Transform laserPoolParent;

    [Header("Hull + Shield")]
    [SerializeField] HullData hullData;
    [SerializeField] ShieldData shieldData;

    [Header("Enemies")]
    [SerializeField] EnemyData enemyData;
    [SerializeField] EnemySpawnerData enemySpawnerData;
    [SerializeField] Transform enemyPoolParent;

    [Header("Debris")]
    [SerializeField] DebrisData debrisData;
    [SerializeField] DebrisSpawnerData debrisSpawnerData;
    [SerializeField] Transform debrisPoolParent;

    [Header("Breach Hazard")]
    [SerializeField] BreachHazardData breachHazardData;
    [SerializeField] BreachHazardSpawnerData breachHazardSpawnerData;
    [SerializeField] Transform breachHazardPoolParent;

    [Header("Round")]
    [SerializeField] RoundData roundData;

    void Awake()
    {
        if (shipInputRouter != null)
            ServiceLocator.Register<IShipInputRouter>(shipInputRouter);

        if (roleAssignment != null)
            ServiceLocator.Register<IRoleAssignment>(roleAssignment);

        if (shipTransform != null && shipMovementData != null)
            ServiceLocator.Register<IShipMovementService>(new ShipMovementService(shipTransform, shipMovementData));

        if (shipTransform != null && weaponData != null && weaponData.laserPrefab != null)
        {
            var laserPool = new ObjectPool<PlayerLaser>(weaponData.laserPrefab, laserPoolParent, weaponData.poolPrewarmCount);
            ServiceLocator.Register<IWeaponService>(new WeaponService(shipTransform, weaponData, laserPool));
        }

        if (hullData != null)
            ServiceLocator.Register<IHullService>(new HullService(hullData));

        if (shieldData != null)
            ServiceLocator.Register<IShieldService>(new ShieldService(shieldData));

        if (enemyData != null && enemySpawnerData != null && enemyData.prefab != null)
        {
            var enemyPool = new ObjectPool<Enemy>(enemyData.prefab, enemyPoolParent, enemySpawnerData.poolPrewarmCount);

            // Only prewarm a projectile pool if this enemy type actually fires - fireInterval stays
            // 0-safe (never used) when projectilePrefab isn't set up yet.
            IObjectPool<EnemyProjectile> enemyProjectilePool = enemyData.projectilePrefab != null
                ? new ObjectPool<EnemyProjectile>(enemyData.projectilePrefab, enemyPoolParent, enemyData.projectilePoolPrewarmCount)
                : null;

            ServiceLocator.Register<IEnemySpawnService>(new EnemySpawnService(enemyData, enemySpawnerData, enemyPool, enemyProjectilePool));
        }

        if (debrisData != null && debrisSpawnerData != null && debrisData.prefab != null)
        {
            var debrisPool = new ObjectPool<Debris>(debrisData.prefab, debrisPoolParent, debrisSpawnerData.poolPrewarmCount);
            ServiceLocator.Register<IDebrisSpawnService>(new DebrisSpawnService(debrisData, debrisSpawnerData, debrisPool));
        }

        if (breachHazardData != null && breachHazardSpawnerData != null && breachHazardData.prefab != null)
        {
            var breachHazardPool = new ObjectPool<BreachHazard>(breachHazardData.prefab, breachHazardPoolParent, breachHazardSpawnerData.poolPrewarmCount);
            ServiceLocator.Register<IBreachHazardSpawnService>(new BreachHazardSpawnService(breachHazardData, breachHazardSpawnerData, breachHazardPool));
        }

        ServiceLocator.Register<IScoreService>(new ScoreService());

        if (roundData != null)
            ServiceLocator.Register<IRoundService>(new RoundService(roundData));

        // Register further Systems-layer services here as they're built.
    }
}
