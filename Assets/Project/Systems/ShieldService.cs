using UnityEngine;

// Plain C# service: owns Shield's active/cooldown timers. No knowledge of Hull.
public class ShieldService : IShieldService
{
    readonly ShieldData data;

    float activeTimeRemaining;
    float cooldownRemaining;

    public ShieldService(ShieldData data)
    {
        this.data = data;
    }

    public bool IsActive => activeTimeRemaining > 0f;
    public float CooldownRemaining => cooldownRemaining;

    public void TryActivate()
    {
        if (IsActive || cooldownRemaining > 0f)
            return;

        activeTimeRemaining = data.duration;
        cooldownRemaining = data.cooldown;
    }

    public void Tick(float deltaTime)
    {
        if (activeTimeRemaining > 0f)
            activeTimeRemaining = Mathf.Max(0f, activeTimeRemaining - deltaTime);
        if (cooldownRemaining > 0f)
            cooldownRemaining = Mathf.Max(0f, cooldownRemaining - deltaTime);
    }
}
