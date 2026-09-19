using UnityEngine;
using UnityEngine.InputSystem;

// Editor development controls: keyboard always drives the Pilot role, mouse always drives the Gunner role.
public class KeyboardMouseInputSource : MonoBehaviour, IInputSource
{
    CoopMovement controls;
    InputAction moveAction;
    InputAction boostAction;
    InputAction aimAction;
    InputAction fireAction;
    InputAction shieldAction;

    bool inputEnabled;
    bool suppressMove;  // set by CancelAll; cleared once the move keys are released
    bool suppressFire;  // set by CancelAll; cleared once the fire button is released

    int loggedMoveSign;  // debug only: last move direction written to the log
    bool loggedFire;     // debug only: last fire state written to the log

    void Awake()
    {
        controls = new CoopMovement();
        moveAction = controls.Pilot.Move;
        boostAction = controls.Pilot.Boost;
        aimAction = controls.Gunner.Aim;
        fireAction = controls.Gunner.Fire;
        shieldAction = controls.Gunner.Shield;
    }

    void OnEnable()
    {
        if (inputEnabled)
            controls.Enable();
    }

    void OnDisable() => controls.Disable();

    void OnDestroy() => controls.Dispose();

    public void SetEnabled(bool isEnabled)
    {
        inputEnabled = isEnabled;
        if (isEnabled)
            controls.Enable();
        else
            controls.Disable();

        if (InputDebugLog.Enabled)
            InputDebugLog.Write(isEnabled ? "KB/M enabled" : "KB/M disabled");
    }

    public void CancelAll()
    {
        suppressMove = true;
        suppressFire = true;

        if (InputDebugLog.Enabled)
            InputDebugLog.Write("KB/M cancelled (held keys ignored until released)");
    }

    public void Tick(ref PilotIntent pilot, ref GunnerIntent gunner)
    {
        if (!inputEnabled)
            return;

        float move = moveAction.ReadValue<float>();
        if (suppressMove)
        {
            if (move == 0f)
                suppressMove = false;
            move = 0f;
        }
        pilot.Move = Mathf.Clamp(pilot.Move + move, -1f, 1f);

        bool boost = boostAction.WasPressedThisFrame();
        pilot.BoostPressed |= boost;

        // Only snap the reticle when the mouse actually moved, so a resting mouse doesn't override touch aiming.
        if (aimAction.WasPerformedThisFrame())
        {
            gunner.HasAbsoluteAim = true;
            gunner.AimScreenPosition = aimAction.ReadValue<Vector2>();
        }

        bool fire = fireAction.IsPressed();
        if (suppressFire)
        {
            if (!fire)
                suppressFire = false;
            fire = false;
        }
        gunner.FireHeld |= fire;

        bool shield = shieldAction.WasPressedThisFrame();
        gunner.ShieldPressed |= shield;

        if (InputDebugLog.Enabled)
            LogChanges(move, boost, fire, shield);
    }

    // Debug only: writes a line when something changes, so held keys don't flood the log.
    void LogChanges(float move, bool boost, bool fire, bool shield)
    {
        int moveSign = move > 0f ? 1 : move < 0f ? -1 : 0;
        if (moveSign != loggedMoveSign)
        {
            loggedMoveSign = moveSign;
            InputDebugLog.Write(moveSign == 0 ? "KEY move stop" : moveSign > 0 ? "KEY move RIGHT" : "KEY move LEFT");
        }

        if (fire != loggedFire)
        {
            loggedFire = fire;
            InputDebugLog.Write(fire ? "MOUSE fire down" : "MOUSE fire up");
        }

        if (boost)
            InputDebugLog.Write("KEY boost (Pilot)");

        if (shield)
            InputDebugLog.Write("MOUSE shield (Gunner)");
    }
}
