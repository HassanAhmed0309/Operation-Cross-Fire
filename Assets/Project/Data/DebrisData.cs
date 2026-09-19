using UnityEngine;

// Debris tuning. moveSpeed isn't in the GDD - set it after creating the asset. health/scoreValue are GDD-given.
[CreateAssetMenu(menuName = "Game Data/Debris", fileName = "DebrisData")]
public class DebrisData : ScriptableObject
{
    public Debris prefab;
    public float moveSpeed;
    public int health = 2;
    public int scoreValue = 15;
}
