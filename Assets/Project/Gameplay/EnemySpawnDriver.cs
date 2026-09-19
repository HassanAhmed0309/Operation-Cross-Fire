using UnityEngine;

// Drives the enemy spawn timer each frame. No decision logic - just calls Tick.
public class EnemySpawnDriver : MonoBehaviour
{
    IEnemySpawnService spawnService;

    void Start() => spawnService = ServiceLocator.Get<IEnemySpawnService>();

    void Update() => spawnService.Tick(Time.deltaTime);
}
