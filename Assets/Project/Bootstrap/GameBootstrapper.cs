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

        // Register further Systems-layer services here as they're built.
    }
}
