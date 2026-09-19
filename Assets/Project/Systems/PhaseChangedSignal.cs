// Published when the round advances to a new phase. Carries the multipliers directly so
// subscribers (EnemySpawnService, later Debris/BreachHazard/enemy-projectile services) don't need RoundData.
public readonly struct PhaseChangedSignal
{
    public readonly Phase NewPhase;
    public readonly float EnemySpeedMultiplier;
    public readonly float SpawnIntervalMultiplier;

    public PhaseChangedSignal(Phase newPhase, float enemySpeedMultiplier, float spawnIntervalMultiplier)
    {
        NewPhase = newPhase;
        EnemySpeedMultiplier = enemySpeedMultiplier;
        SpawnIntervalMultiplier = spawnIntervalMultiplier;
    }
}
