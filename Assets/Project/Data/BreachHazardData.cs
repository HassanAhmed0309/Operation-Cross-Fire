using UnityEngine;

// Breach hazard tuning. moveSpeed isn't in the GDD - set it after creating the asset. health/scoreValue are GDD-given.
[CreateAssetMenu(menuName = "Game Data/Breach Hazard", fileName = "BreachHazardData")]
public class BreachHazardData : ScriptableObject
{
    public BreachHazard prefab;
    public float moveSpeed;
    public int health = 3;
    public int scoreValue = 25;
}
