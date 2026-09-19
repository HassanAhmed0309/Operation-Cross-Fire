using UnityEngine;

// Toggles a child renderer (circle/outline) while Shield is active - the GDD's minimal Shield visual.
public class ShieldVisual : MonoBehaviour
{
    [SerializeField] GameObject shieldOutline;

    IShieldService shieldService;

    void Start() => shieldService = ServiceLocator.Get<IShieldService>();

    void Update() => shieldOutline.SetActive(shieldService.IsActive);
}
