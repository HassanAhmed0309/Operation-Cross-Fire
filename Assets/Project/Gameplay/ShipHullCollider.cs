using UnityEngine;

// Detects enemy contact with the ship. Checks Shield itself before applying hull damage - same
// coordination pattern as DebugDamageTrigger; Hull and Shield stay independent Systems services.
[RequireComponent(typeof(Collider2D))]
public class ShipHullCollider : MonoBehaviour
{
    IHullService hullService;
    IShieldService shieldService;

    void Start()
    {
        hullService = ServiceLocator.Get<IHullService>();
        shieldService = ServiceLocator.Get<IShieldService>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent(out IShipContactHazard hazard))
            return;

        if (!shieldService.IsActive)
            hullService.ApplyDamage(1);

        hazard.OnHitShip();
    }
}
