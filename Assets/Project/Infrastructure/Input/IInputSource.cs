public interface IInputSource
{
    // Adds this source's contribution to the intents the router rebuilds each frame.
    void Tick(ref PilotIntent pilot, ref GunnerIntent gunner);

    // Drops everything currently held; held controls stay ignored until released.
    void CancelAll();

    void SetEnabled(bool isEnabled);
}
