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

    void Awake()
    {
        if (shipInputRouter != null)
            ServiceLocator.Register<IShipInputRouter>(shipInputRouter);

        if (roleAssignment != null)
            ServiceLocator.Register<IRoleAssignment>(roleAssignment);

        if (shipTransform != null && shipMovementData != null)
            ServiceLocator.Register<IShipMovementService>(new ShipMovementService(shipTransform, shipMovementData));

        // Register further Systems-layer services here as they're built.
    }
}
