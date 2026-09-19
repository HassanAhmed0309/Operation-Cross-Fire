using UnityEngine;

// What GunnerController calls each frame to aim and fire.
public interface IWeaponService
{
    void SetAimWorldPosition(Vector3 worldPosition);
    void TryFire();
    void Tick(float deltaTime);
}
