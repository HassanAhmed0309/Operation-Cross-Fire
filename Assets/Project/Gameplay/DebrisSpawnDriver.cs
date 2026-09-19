using UnityEngine;

// Drives the debris spawn timer each frame. No decision logic - just calls Tick.
public class DebrisSpawnDriver : MonoBehaviour
{
    IDebrisSpawnService spawnService;

    void Start() => spawnService = ServiceLocator.Get<IDebrisSpawnService>();

    void Update() => spawnService.Tick(Time.deltaTime);
}
