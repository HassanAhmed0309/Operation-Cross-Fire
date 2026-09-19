using UnityEngine;

// Reads the current Gunner intent each frame: moves the reticle, fires through Systems.
public class GunnerController : MonoBehaviour
{
    [SerializeField] float touchAimSensitivity = 0.02f; // world units per pixel of drag - not in GDD, tune by feel

    Camera cachedCamera;
    IShipInputRouter inputRouter;
    IWeaponService weaponService;
    Vector3 reticleWorldPosition;

    void Awake()
    {
        cachedCamera = Camera.main;
    }

    void Start()
    {
        inputRouter = ServiceLocator.Get<IShipInputRouter>();
        weaponService = ServiceLocator.Get<IWeaponService>();
        reticleWorldPosition = transform.position + Vector3.up * 2f;
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
            reticleWorldPosition = cachedCamera.ScreenToWorldPoint(screenPoint);
        }
        else if (gunner.AimDelta != Vector2.zero)
        {
            reticleWorldPosition += (Vector3)(gunner.AimDelta * touchAimSensitivity);
        }

        weaponService.SetAimWorldPosition(reticleWorldPosition);

        if (gunner.FireHeld)
            weaponService.TryFire();

        weaponService.Tick(Time.deltaTime);
    }
}
