using UnityEngine;

// Drives the breach hazard spawn timer each frame. No decision logic - just calls Tick.
public class BreachHazardSpawnDriver : MonoBehaviour
{
    IBreachHazardSpawnService spawnService;

    void Start() => spawnService = ServiceLocator.Get<IBreachHazardSpawnService>();

    void Update() => spawnService.Tick(Time.deltaTime);
}
