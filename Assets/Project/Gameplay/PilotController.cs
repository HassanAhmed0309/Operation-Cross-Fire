using UnityEngine;

// Reads the current Pilot intent each frame and drives ship movement/Boost through Systems.
public class PilotController : MonoBehaviour
{
    IShipInputRouter inputRouter;
    IShipMovementService movementService;
    IHullService hullService;

    void Start()
    {
        inputRouter = ServiceLocator.Get<IShipInputRouter>();
        movementService = ServiceLocator.Get<IShipMovementService>();
        hullService = ServiceLocator.Get<IHullService>();
    }

    void Update()
    {
        hullService.Tick(Time.deltaTime); // ship-wide state; ticked here since Pilot always runs, regardless of role

        if (!inputRouter.GameplayEnabled)
            return;

        PilotIntent pilot = inputRouter.Pilot;
        movementService.SetMoveAxis(pilot.Move);
        if (pilot.BoostPressed)
            movementService.TryBoost();

        movementService.Tick(Time.deltaTime);
    }
}
