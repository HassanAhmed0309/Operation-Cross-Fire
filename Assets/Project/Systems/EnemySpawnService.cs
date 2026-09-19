using UnityEngine;

// Plain C# service: counts down spawnInterval, pulls an Enemy from the pool and places it at the top edge.
public class EnemySpawnService : IEnemySpawnService
{
    readonly EnemyData enemyData;
    readonly EnemySpawnerData spawnerData;
    readonly IObjectPool<Enemy> pool;
    readonly IObjectPool<EnemyProjectile> projectilePool;

    float timeUntilNextSpawn;
    bool spawningEnabled = true;
    float speedMultiplier = 1f;
    float spawnIntervalMultiplier = 1f;
    float projectileSpeedMultiplier = 1f;

    public EnemySpawnService(EnemyData enemyData, EnemySpawnerData spawnerData, IObjectPool<Enemy> pool, IObjectPool<EnemyProjectile> projectilePool)
    {
        this.enemyData = enemyData;
        this.spawnerData = spawnerData;
        this.pool = pool;
        this.projectilePool = projectilePool;
        timeUntilNextSpawn = spawnerData.spawnInterval;

        EventBus.Subscribe<PhaseChangedSignal>(OnPhaseChanged);
        EventBus.Subscribe<RoundEndedSignal>(OnRoundEnded);
    }

    public void SetSpawningEnabled(bool isEnabled) => spawningEnabled = isEnabled;

    public void Tick(float deltaTime)
    {
        if (!spawningEnabled)
            return;

        timeUntilNextSpawn -= deltaTime;
        if (timeUntilNextSpawn > 0f)
            return;

        timeUntilNextSpawn = spawnerData.spawnInterval * spawnIntervalMultiplier;

        float x = Random.Range(-spawnerData.spawnHalfWidth, spawnerData.spawnHalfWidth);
        Vector3 position = new Vector3(x, spawnerData.spawnY, 0f);

        Enemy enemy = pool.Get();
        enemy.Launch(position, enemyData.moveSpeed * speedMultiplier, enemyData.health, enemyData.scoreValue, pool,
            enemyData.fireInterval, enemyData.projectileSpeed * projectileSpeedMultiplier, projectilePool);
    }

    void OnPhaseChanged(PhaseChangedSignal signal)
    {
        speedMultiplier = signal.EnemySpeedMultiplier;
        spawnIntervalMultiplier = signal.SpawnIntervalMultiplier;
        projectileSpeedMultiplier = signal.ProjectileSpeedMultiplier;
    }

    void OnRoundEnded(RoundEndedSignal signal) => SetSpawningEnabled(false);
}
