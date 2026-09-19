// Published when the round advances to a new phase. Carries the multipliers directly so
// subscribers (EnemySpawnService, DebrisSpawnService, BreachHazardSpawnService) don't need RoundData.
public readonly struct PhaseChangedSignal
{
    public readonly Phase NewPhase;
    public readonly float EnemySpeedMultiplier;
    public readonly float SpawnIntervalMultiplier;
    public readonly float ProjectileSpeedMultiplier;

    public PhaseChangedSignal(Phase newPhase, float enemySpeedMultiplier, float spawnIntervalMultiplier, float projectileSpeedMultiplier)
    {
        NewPhase = newPhase;
        EnemySpeedMultiplier = enemySpeedMultiplier;
        SpawnIntervalMultiplier = spawnIntervalMultiplier;
        ProjectileSpeedMultiplier = projectileSpeedMultiplier;
    }
}
