using TMPro;
using UnityEngine;

// Polls Boost's cooldown every frame - continuously changing, not event-driven.
public class BoostCooldownDisplay : MonoBehaviour
{
    [SerializeField] TMP_Text boostText;

    IShipMovementService movementService;

    void Start() => movementService = ServiceLocator.Get<IShipMovementService>();

    void Update()
    {
        float remaining = movementService.BoostCooldownRemaining;
        boostText.SetText(remaining > 0f ? "BOOST {0:0.0}s" : "BOOST READY", remaining);
    }
}
