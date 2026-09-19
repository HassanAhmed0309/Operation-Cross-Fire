// What Gameplay resolves from ServiceLocator to read this frame's merged Pilot/Gunner input.
public interface IShipInputRouter
{
    PilotIntent Pilot { get; }
    GunnerIntent Gunner { get; }
    bool GameplayEnabled { get; }

    // Quantum Flux: drop all held input across every source.
    void CancelAll();

    // Round start/end: turn gameplay input reading on/off.
    void SetGameplayEnabled(bool isEnabled);
}
