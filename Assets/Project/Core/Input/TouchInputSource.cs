using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

// Device touch controls. Each touch is bound to the control it began on and keeps that binding until it ends.
public class TouchInputSource : MonoBehaviour, IInputSource
{
    const int MaxTouches = 10;

    struct TouchBinding
    {
        public int TouchId;
        public TouchControlId Control;
        public bool Cancelled;  // held through a Quantum Flux; ignored until lifted
        public bool Seen;       // still present in this frame's active touches
    }

    [SerializeField] TouchPanel leftPanel;   // Player 1
    [SerializeField] TouchPanel rightPanel;  // Player 2

    readonly TouchBinding[] bindings = new TouchBinding[MaxTouches];
    int bindingCount;
    bool inputEnabled;

    void OnEnable() => EnhancedTouchSupport.Enable();

    void OnDisable()
    {
        EnhancedTouchSupport.Disable();
        bindingCount = 0;
    }

    public void SetEnabled(bool isEnabled)
    {
        inputEnabled = isEnabled;
        if (!isEnabled)
            bindingCount = 0;

        if (InputDebugLog.Enabled)
            InputDebugLog.Write(isEnabled ? "TOUCH enabled" : "TOUCH disabled");
    }

    public void CancelAll()
    {
        for (int i = 0; i < bindingCount; i++)
            bindings[i].Cancelled = true;

        if (InputDebugLog.Enabled)
            InputDebugLog.Write("TOUCH cancelled " + bindingCount + " touch(es)");
    }

    public void Tick(ref PilotIntent pilot, ref GunnerIntent gunner)
    {
        if (!inputEnabled)
            return;

        for (int i = 0; i < bindingCount; i++)
            bindings[i].Seen = false;

        bool left = false;
        bool right = false;

        var touches = Touch.activeTouches;
        for (int t = 0; t < touches.Count; t++)
        {
            Touch touch = touches[t];
            TouchPhase phase = touch.phase;

            int index = FindBinding(touch.touchId);
            if (index < 0)
            {
                // Ownership is decided only where a touch begins; unbound touches are ignored for their lifetime.
                if (phase != TouchPhase.Began)
                    continue;
                index = TryBind(touch);
                if (index < 0)
                {
                    if (InputDebugLog.Enabled && phase == TouchPhase.Began)
                        InputDebugLog.Write("TOUCH #" + touch.touchId + " began on no control (ignored)");
                    continue;
                }
            }

            // Ended touches stay unseen and are removed below.
            if (phase == TouchPhase.Ended || phase == TouchPhase.Canceled)
                continue;

            bindings[index].Seen = true;
            if (bindings[index].Cancelled)
                continue;

            switch (bindings[index].Control)
            {
                case TouchControlId.Left:
                    left = true;
                    break;
                case TouchControlId.Right:
                    right = true;
                    break;
                case TouchControlId.Boost:
                    if (phase == TouchPhase.Began)
                    {
                        pilot.BoostPressed = true;
                        if (InputDebugLog.Enabled)
                            InputDebugLog.Write("TOUCH boost (Pilot)");
                    }
                    break;
                case TouchControlId.AimArea:
                    gunner.AimDelta += touch.delta;
                    break;
                case TouchControlId.Fire:
                    gunner.FireHeld = true;
                    break;
                case TouchControlId.Shield:
                    if (phase == TouchPhase.Began)
                    {
                        gunner.ShieldPressed = true;
                        if (InputDebugLog.Enabled)
                            InputDebugLog.Write("TOUCH shield (Gunner)");
                    }
                    break;
            }
        }

        pilot.Move = Mathf.Clamp(pilot.Move + (right ? 1f : 0f) - (left ? 1f : 0f), -1f, 1f);

        RemoveUnseenBindings();
    }

    int FindBinding(int touchId)
    {
        for (int i = 0; i < bindingCount; i++)
        {
            if (bindings[i].TouchId == touchId)
                return i;
        }
        return -1;
    }

    int TryBind(Touch touch)
    {
        if (bindingCount == MaxTouches)
            return -1;

        Vector2 position = touch.screenPosition;
        TouchPanel panel = position.x < Screen.width * 0.5f ? leftPanel : rightPanel;
        if (panel == null)
            return -1;

        TouchControl control = panel.HitTest(position);
        if (control == null)
            return -1;

        bindings[bindingCount] = new TouchBinding { TouchId = touch.touchId, Control = control.Id };

        if (InputDebugLog.Enabled)
            InputDebugLog.Write("TOUCH #" + touch.touchId + " bound to " + control.Id + " (" + panel.Player + ")");

        return bindingCount++;
    }

    // Also clears bindings whose Ended phase was missed (e.g. the app lost focus mid-touch).
    void RemoveUnseenBindings()
    {
        for (int i = bindingCount - 1; i >= 0; i--)
        {
            if (bindings[i].Seen)
                continue;

            if (InputDebugLog.Enabled)
                InputDebugLog.Write("TOUCH #" + bindings[i].TouchId + " released from " + bindings[i].Control);

            bindingCount--;
            bindings[i] = bindings[bindingCount];
        }
    }
}
