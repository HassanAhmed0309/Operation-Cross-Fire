using UnityEngine;

// Plain C# service: counts down spawnInterval, pulls Debris from the pool and places it at the top edge.
// Debris speed itself doesn't scale with difficulty (the GDD only calls out enemy speed) - only the
// spawn interval reacts to phase changes, same as every other spawner.
public class DebrisSpawnService : IDebrisSpawnService
{
    readonly DebrisData debrisData;
    readonly DebrisSpawnerData spawnerData;
    readonly IObjectPool<Debris> pool;

    float timeUntilNextSpawn;
    bool spawningEnabled = true;
    float spawnIntervalMultiplier = 1f;

    public DebrisSpawnService(DebrisData debrisData, DebrisSpawnerData spawnerData, IObjectPool<Debris> pool)
    {
        this.debrisData = debrisData;
        this.spawnerData = spawnerData;
        this.pool = pool;
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

        Debris debris = pool.Get();
        debris.Launch(position, debrisData.moveSpeed, debrisData.health, debrisData.scoreValue, pool);
    }

    void OnPhaseChanged(PhaseChangedSignal signal) => spawnIntervalMultiplier = signal.SpawnIntervalMultiplier;

    void OnRoundEnded(RoundEndedSignal signal) => SetSpawningEnabled(false);
}
