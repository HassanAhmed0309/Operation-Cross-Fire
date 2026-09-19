using UnityEngine;

// Plain C# service: counts down spawnInterval, pulls an Enemy from the pool and places it at the top edge.
public class EnemySpawnService : IEnemySpawnService
{
    readonly EnemyData enemyData;
    readonly EnemySpawnerData spawnerData;
    readonly IObjectPool<Enemy> pool;

    float timeUntilNextSpawn;
    bool spawningEnabled = true;

    public EnemySpawnService(EnemyData enemyData, EnemySpawnerData spawnerData, IObjectPool<Enemy> pool)
    {
        this.enemyData = enemyData;
        this.spawnerData = spawnerData;
        this.pool = pool;
        timeUntilNextSpawn = spawnerData.spawnInterval;
    }

    public void SetSpawningEnabled(bool isEnabled) => spawningEnabled = isEnabled;

    public void Tick(float deltaTime)
    {
        if (!spawningEnabled)
            return;

        timeUntilNextSpawn -= deltaTime;
        if (timeUntilNextSpawn > 0f)
            return;

        timeUntilNextSpawn = spawnerData.spawnInterval;

        float x = Random.Range(-spawnerData.spawnHalfWidth, spawnerData.spawnHalfWidth);
        Vector3 position = new Vector3(x, spawnerData.spawnY, 0f);

        Enemy enemy = pool.Get();
        enemy.Launch(position, enemyData.moveSpeed, enemyData.health, enemyData.scoreValue, pool);
    }
}
