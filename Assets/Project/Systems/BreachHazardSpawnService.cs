using UnityEngine;

// Plain C# service: same shape as EnemySpawnService/DebrisSpawnService, but starts disabled -
// breach hazards only begin appearing once Alert phase starts (GDD: "starts in Alert").
public class BreachHazardSpawnService : IBreachHazardSpawnService
{
    readonly BreachHazardData hazardData;
    readonly BreachHazardSpawnerData spawnerData;
    readonly IObjectPool<BreachHazard> pool;

    float timeUntilNextSpawn;
    bool spawningEnabled;
    float spawnIntervalMultiplier = 1f;

    public BreachHazardSpawnService(BreachHazardData hazardData, BreachHazardSpawnerData spawnerData, IObjectPool<BreachHazard> pool)
    {
        this.hazardData = hazardData;
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

        BreachHazard hazard = pool.Get();
        hazard.Launch(position, hazardData.moveSpeed, hazardData.health, hazardData.scoreValue, pool);
    }

    void OnPhaseChanged(PhaseChangedSignal signal)
    {
        spawnIntervalMultiplier = signal.SpawnIntervalMultiplier;

        if (signal.NewPhase != Phase.Patrol)
            spawningEnabled = true;
    }

    void OnRoundEnded(RoundEndedSignal signal) => SetSpawningEnabled(false);
}
