using UnityEngine;

// Shield tuning - both values are GDD-given, nothing to guess.
[CreateAssetMenu(menuName = "Game Data/Ship/Shield", fileName = "ShieldData")]
public class ShieldData : ScriptableObject
{
    public float duration = 1.5f;
    public float cooldown = 5f;
}
