// What DebrisSpawnDriver calls each frame to advance the spawn timer.
public interface IDebrisSpawnService
{
    void Tick(float deltaTime);
    void SetSpawningEnabled(bool isEnabled);
}
