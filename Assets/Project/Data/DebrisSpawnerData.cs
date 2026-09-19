using UnityEngine;

// Debris spawn tuning. spawnInterval isn't in the GDD - set it after creating the asset.
[CreateAssetMenu(menuName = "Game Data/Debris Spawner", fileName = "DebrisSpawnerData")]
public class DebrisSpawnerData : ScriptableObject
{
    public float spawnInterval;
    public float spawnHalfWidth = 7.5f;
    public float spawnY = 6f;
    public int poolPrewarmCount = 6;
}
