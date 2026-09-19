using UnityEngine;

// Ship horizontal movement + Boost tuning. moveSpeed/playfieldHalfWidth aren't in the GDD - set them after creating the asset.
[CreateAssetMenu(menuName = "Game Data/Ship/Movement", fileName = "ShipMovementData")]
public class ShipMovementData : ScriptableObject
{
    [Header("Movement")]
    public float moveSpeed;
    public float playfieldHalfWidth;

    [Header("Boost")]
    public float boostDuration = 1f;
    public float boostSpeedMultiplier = 1.75f;
    public float boostCooldown = 4f;
}
