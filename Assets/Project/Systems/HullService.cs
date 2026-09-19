using UnityEngine;

// Plain C# service: owns hull points + the post-hit invulnerability window. No knowledge of Shield.
public class HullService : IHullService
{
    readonly HullData data;

    int currentHull;
    float invulnerabilityRemaining;

    public HullService(HullData data)
    {
        this.data = data;
        currentHull = data.maxHull;
    }

    public int CurrentHull => currentHull;
    public int MaxHull => data.maxHull;
    public bool IsInvulnerable => invulnerabilityRemaining > 0f;
    public bool IsDestroyed => currentHull <= 0;

    public bool ApplyDamage(int amount = 1)
    {
        if (IsInvulnerable || IsDestroyed)
            return false;

        currentHull = Mathf.Max(0, currentHull - amount);
        invulnerabilityRemaining = data.invulnerabilityDuration;

        EventBus.Publish(new HullChangedSignal(currentHull, data.maxHull));

        if (currentHull <= 0)
            EventBus.Publish(new ShipDestroyedSignal());

        return true;
    }

    public void Tick(float deltaTime)
    {
        if (invulnerabilityRemaining > 0f)
            invulnerabilityRemaining = Mathf.Max(0f, invulnerabilityRemaining - deltaTime);
    }
}
