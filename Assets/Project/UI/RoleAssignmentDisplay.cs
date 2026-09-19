using TMPro;
using UnityEngine;

// Displays which player currently holds which role. Same OnEnable/Refresh shape as TouchPanel.
public class RoleAssignmentDisplay : MonoBehaviour
{
    [SerializeField] TMP_Text player1Text;
    [SerializeField] TMP_Text player2Text;

    IRoleAssignment roleAssignment;

    void OnEnable()
    {
        roleAssignment = ServiceLocator.Get<IRoleAssignment>();
        roleAssignment.RolesChanged += Refresh;
        Refresh();
    }

    void OnDisable() => roleAssignment.RolesChanged -= Refresh;

    void Refresh()
    {
        player1Text.text = "PLAYER 1 - " + roleAssignment.GetRole(PlayerId.P1).ToString().ToUpperInvariant();
        player2Text.text = "PLAYER 2 - " + roleAssignment.GetRole(PlayerId.P2).ToString().ToUpperInvariant();
    }
}
