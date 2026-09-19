using UnityEngine;

// Enemy tuning. moveSpeed isn't in the GDD - set it after creating the asset. health/scoreValue are GDD-given.
[CreateAssetMenu(menuName = "Game Data/Enemy", fileName = "EnemyData")]
public class EnemyData : ScriptableObject
{
    public Enemy prefab;
    public float moveSpeed;
    public int health = 1;
    public int scoreValue = 10;
}
