using UnityEngine;

// Round timing + difficulty tuning - every value here is GDD-given, nothing to guess.
[CreateAssetMenu(menuName = "Game Data/Round", fileName = "RoundData")]
public class RoundData : ScriptableObject
{
    public float roundDuration = 60f;
    public float patrolEndTime = 20f;
    public float alertEndTime = 40f;
    public float warningLeadTime = 3f;
    public float alertEnemySpeedMultiplier = 1.25f;
    public float criticalSpawnIntervalMultiplier = 0.70f;
    public float criticalProjectileSpeedMultiplier = 1.50f;
}
