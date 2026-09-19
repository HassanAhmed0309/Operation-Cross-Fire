using TMPro;
using UnityEngine;

// One side of the screen. Shows the control group for its player's current role.
public class TouchPanel : MonoBehaviour
{
    [SerializeField] PlayerId player;
    [SerializeField] RoleAssignment roles;
    [SerializeField] GameObject pilotGroup;
    [SerializeField] GameObject gunnerGroup;
    [SerializeField] TouchControl[] pilotControls;   // Left, Right, Boost
    [SerializeField] TouchControl[] gunnerControls;  // AimArea, Fire, Shield
    [SerializeField] TMP_Text roleLabel;

    TouchControl[] activeControls;
    string pilotLabel;
    string gunnerLabel;

    public PlayerId Player => player;

    void Awake()
    {
        int playerNumber = (int)player + 1;
        pilotLabel = "PLAYER " + playerNumber + " — PILOT";
        gunnerLabel = "PLAYER " + playerNumber + " — GUNNER";
    }

    void OnEnable()
    {
        roles.RolesChanged += Refresh;
        Refresh();
    }

    void OnDisable() => roles.RolesChanged -= Refresh;

    public void Refresh()
    {
        bool isPilot = roles.GetRole(player) == Role.Pilot;
        pilotGroup.SetActive(isPilot);
        gunnerGroup.SetActive(!isPilot);
        activeControls = isPilot ? pilotControls : gunnerControls;

        if (roleLabel != null)
            roleLabel.text = isPilot ? pilotLabel : gunnerLabel;

        if (InputDebugLog.Enabled)
            InputDebugLog.Write("PANEL " + player + " now " + (isPilot ? "PILOT" : "GUNNER"));
    }

    public TouchControl HitTest(Vector2 screenPosition)
    {
        if (!isActiveAndEnabled)
            return null;

        for (int i = 0; i < activeControls.Length; i++)
        {
            if (activeControls[i].Contains(screenPosition))
                return activeControls[i];
        }
        return null;
    }
}
