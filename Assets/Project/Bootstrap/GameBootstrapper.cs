using UnityEngine;

/// <summary>
/// Single entry point for service registration. Runs before other scripts' Awake so consumers
/// can safely resolve services in their own Start. Never `new` a service anywhere but here.
/// </summary>
[DefaultExecutionOrder(-1000)]
public class GameBootstrapper : MonoBehaviour
{
    private void Awake()
    {
        // Register Systems-layer services here as they're built, e.g.:
        // ServiceLocator.Register<IShipMovementService>(new ShipMovementService(shipData));
    }
}
