using UnityEngine;

// Breach hazard spawn tuning. spawnInterval isn't in the GDD - set it after creating the asset.
// Spawning only actually starts once Alert phase begins, regardless of this interval's value.
[CreateAssetMenu(menuName = "Game Data/Breach Hazard Spawner", fileName = "BreachHazardSpawnerData")]
public class BreachHazardSpawnerData : ScriptableObject
{
    public float spawnInterval;
    public float spawnHalfWidth = 7.5f;
    public float spawnY = 6f;
    public int poolPrewarmCount = 4;
}
