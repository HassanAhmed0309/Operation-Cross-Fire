using UnityEngine;

// Verification aid until real collision sources exist (enemies/debris/hazards). Attach anywhere,
// right-click the component in Play mode > Apply Test Damage. Delete once real collisions land.
public class DebugDamageTrigger : MonoBehaviour
{
    IHullService hullService;
    IShieldService shieldService;

    void Start()
    {
        hullService = ServiceLocator.Get<IHullService>();
        shieldService = ServiceLocator.Get<IShieldService>();
    }

    [ContextMenu("Apply Test Damage")]
    void ApplyTestDamage()
    {
        if (hullService.IsDestroyed)
        {
#if UNITY_EDITOR
            Debug.Log("Ship already destroyed (hull 0) - no round/lose system yet, this is expected until that step.");
#endif
            return;
        }

        if (shieldService.IsActive)
        {
#if UNITY_EDITOR
            Debug.Log("Shield blocked the hit.");
#endif
            return;
        }

        bool applied = hullService.ApplyDamage(1);

#if UNITY_EDITOR
        if (!applied)
            Debug.Log("Invulnerability blocked the hit.");
        else
            Debug.Log("Hull now " + hullService.CurrentHull + "/" + hullService.MaxHull
                + (hullService.IsDestroyed ? " - DESTROYED (ShipDestroyedSignal published, nothing listens yet)" : ""));
#endif
    }

    // Testing aid: activates Shield without needing a working input binding.
    [ContextMenu("Force Activate Shield")]
    void ForceActivateShield()
    {
        shieldService.TryActivate();

#if UNITY_EDITOR
        Debug.Log(shieldService.IsActive
            ? "Shield activated (test) - " + shieldService.CooldownRemaining + "s cooldown queued."
            : "Shield did not activate - still on cooldown (" + shieldService.CooldownRemaining + "s remaining).");
#endif
    }
}
