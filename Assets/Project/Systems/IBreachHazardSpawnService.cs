// What BreachHazardSpawnDriver calls each frame to advance the spawn timer.
public interface IBreachHazardSpawnService
{
    void Tick(float deltaTime);
    void SetSpawningEnabled(bool isEnabled);
}
