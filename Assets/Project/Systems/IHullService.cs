// What a hit-detector (debug trigger now, real collisions later) calls when something hits the ship.
public interface IHullService
{
    int CurrentHull { get; }
    int MaxHull { get; }
    bool IsInvulnerable { get; }
    bool IsDestroyed { get; }

    // Returns true if the hit actually removed a hull point; false if blocked by invulnerability or already destroyed.
    // Callers check IShieldService.IsActive themselves before calling this - Hull doesn't know about Shield.
    bool ApplyDamage(int amount = 1);

    void Tick(float deltaTime);
}
