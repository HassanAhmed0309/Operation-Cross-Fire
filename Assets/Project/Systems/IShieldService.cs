// What GunnerController calls to activate Shield, and what a hit-detector checks before applying hull damage.
public interface IShieldService
{
    bool IsActive { get; }
    float CooldownRemaining { get; }

    void TryActivate();
    void Tick(float deltaTime);
}
