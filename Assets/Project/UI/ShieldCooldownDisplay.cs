using TMPro;
using UnityEngine;

// Polls Shield's active/cooldown state every frame - continuously changing, not event-driven.
public class ShieldCooldownDisplay : MonoBehaviour
{
    [SerializeField] TMP_Text shieldText;

    IShieldService shieldService;

    void Start() => shieldService = ServiceLocator.Get<IShieldService>();

    void Update()
    {
        if (shieldService.IsActive)
        {
            shieldText.text = "SHIELD ACTIVE";
            return;
        }

        float remaining = shieldService.CooldownRemaining;
        shieldText.SetText(remaining > 0f ? "SHIELD {0:0.0}s" : "SHIELD READY", remaining);
    }
}
