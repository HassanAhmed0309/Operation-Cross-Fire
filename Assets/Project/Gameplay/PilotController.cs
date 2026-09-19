using UnityEngine;

// Reads the current Pilot intent each frame and drives ship movement/Boost through Systems.
public class PilotController : MonoBehaviour
{
    IShipInputRouter inputRouter;
    IShipMovementService movementService;

    void Start()
    {
        inputRouter = ServiceLocator.Get<IShipInputRouter>();
        movementService = ServiceLocator.Get<IShipMovementService>();
    }

    void Update()
    {
        if (!inputRouter.GameplayEnabled)
            return;

        PilotIntent pilot = inputRouter.Pilot;
        movementService.SetMoveAxis(pilot.Move);
        if (pilot.BoostPressed)
            movementService.TryBoost();

        movementService.Tick(Time.deltaTime);
    }
}
