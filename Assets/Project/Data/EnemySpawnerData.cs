using UnityEngine;

// Enemy spawn tuning. spawnInterval isn't in the GDD - set it after creating the asset.
[CreateAssetMenu(menuName = "Game Data/Enemy Spawner", fileName = "EnemySpawnerData")]
public class EnemySpawnerData : ScriptableObject
{
    public float spawnInterval;
    public float spawnHalfWidth = 7.5f; // matches ShipMovementData's suggested playfieldHalfWidth unless you want a wider spawn band
    public float spawnY = 6f;
    public int poolPrewarmCount = 10;
}
