using UnityEngine;

// Reads the current Gunner intent each frame: moves the reticle, fires through Systems.
// The reticle is tracked as an offset from the ship, not an absolute world point - otherwise
// Pilot movement would silently change where the Gunner is currently aiming relative to the ship.
public class GunnerController : MonoBehaviour
{
    [SerializeField] float touchAimSensitivity = 0.02f; // world units per pixel of drag - not in GDD, tune by feel

    Camera cachedCamera;
    IShipInputRouter inputRouter;
    IWeaponService weaponService;
    IShieldService shieldService;
    Vector2 aimOffset;

    void Awake()
    {
        cachedCamera = Camera.main;
    }

    void Start()
    {
        inputRouter = ServiceLocator.Get<IShipInputRouter>();
        weaponService = ServiceLocator.Get<IWeaponService>();
        shieldService = ServiceLocator.Get<IShieldService>();
        aimOffset = Vector2.up * 2f;
    }

    void Update()
    {
        if (!inputRouter.GameplayEnabled)
            return;

        GunnerIntent gunner = inputRouter.Gunner;

        if (gunner.HasAbsoluteAim)
        {
            Vector3 screenPoint = gunner.AimScreenPosition;
            screenPoint.z = -cachedCamera.transform.position.z; // distance to the z=0 gameplay plane
            Vector3 worldPoint = cachedCamera.ScreenToWorldPoint(screenPoint);
            aimOffset = worldPoint - transform.position;
        }
        else if (gunner.AimDelta != Vector2.zero)
        {
            aimOffset += gunner.AimDelta * touchAimSensitivity;
        }

        weaponService.SetAimWorldPosition(transform.position + (Vector3)aimOffset);

        if (gunner.FireHeld)
            weaponService.TryFire();

        if (gunner.ShieldPressed)
            shieldService.TryActivate();

        weaponService.Tick(Time.deltaTime);
        shieldService.Tick(Time.deltaTime);
    }
}
