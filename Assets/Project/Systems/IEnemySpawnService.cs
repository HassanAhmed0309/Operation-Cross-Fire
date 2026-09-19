// What EnemySpawnDriver calls each frame to advance the spawn timer.
public interface IEnemySpawnService
{
    void Tick(float deltaTime);
    void SetSpawningEnabled(bool isEnabled);
}
