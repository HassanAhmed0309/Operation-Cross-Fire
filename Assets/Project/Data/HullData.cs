using UnityEngine;

// Ship hull tuning. maxHull is GDD-given; invulnerabilityDuration isn't - set it after creating the asset.
[CreateAssetMenu(menuName = "Game Data/Ship/Hull", fileName = "HullData")]
public class HullData : ScriptableObject
{
    public int maxHull = 3;
    public float invulnerabilityDuration;
}
