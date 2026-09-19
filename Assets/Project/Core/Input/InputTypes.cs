using UnityEngine;

public enum PlayerId { P1 = 0, P2 = 1 }

public enum Role { Pilot, Gunner }

public enum TouchControlId { None, Left, Right, Boost, AimArea, Fire, Shield }

// Everything the current Pilot wants this frame, independent of device or player.
public struct PilotIntent
{
    public float Move;          // -1..1; left + right held together cancels to 0
    public bool BoostPressed;   // true only on the frame the press began
}

// Everything the current Gunner wants this frame, independent of device or player.
public struct GunnerIntent
{
    public bool HasAbsoluteAim;         // mouse: reticle snaps to AimScreenPosition
    public Vector2 AimScreenPosition;
    public Vector2 AimDelta;            // touch drag: reticle moves by this many pixels
    public bool FireHeld;               // the weapon applies its own cooldown
    public bool ShieldPressed;          // true only on the frame the press began
}
