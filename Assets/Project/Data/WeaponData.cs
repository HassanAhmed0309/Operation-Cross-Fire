using UnityEngine;

// Weapon tuning. fireCooldown is GDD-given; laserSpeed + poolPrewarmCount aren't - set after creating the asset.
[CreateAssetMenu(menuName = "Game Data/Weapon", fileName = "WeaponData")]
public class WeaponData : ScriptableObject
{
    public PlayerLaser laserPrefab;
    public float fireCooldown = 0.25f;
    public float laserSpeed;
    public int poolPrewarmCount = 8;
}
