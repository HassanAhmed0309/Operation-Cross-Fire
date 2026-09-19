using UnityEngine;

// Drives the round timer each frame. No decision logic - just calls Tick.
public class RoundDriver : MonoBehaviour
{
    IRoundService roundService;

    void Start() => roundService = ServiceLocator.Get<IRoundService>();

    void Update() => roundService.Tick(Time.deltaTime);
}
