using UnityEngine;

// Enemy tuning. moveSpeed/firing fields aren't in the GDD - set them after creating the asset.
// health/scoreValue are GDD-given. fireInterval = 0 means this enemy type never fires.
[CreateAssetMenu(menuName = "Game Data/Enemy", fileName = "EnemyData")]
public class EnemyData : ScriptableObject
{
    public Enemy prefab;
    public float moveSpeed;
    public int health = 1;
    public int scoreValue = 10;

    [Header("Firing (0 fireInterval = never fires)")]
    public float fireInterval;
    public float projectileSpeed;
    public EnemyProjectile projectilePrefab;
    public int projectilePoolPrewarmCount = 6;
}
