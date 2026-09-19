using System;
using UnityEngine;

// Single source of truth for which player currently holds which role.
public class RoleAssignment : MonoBehaviour, IRoleAssignment
{
    readonly Role[] roles = { Role.Pilot, Role.Gunner };

    public event Action RolesChanged;

    public Role GetRole(PlayerId player) => roles[(int)player];

    public PlayerId GetPlayer(Role role) => roles[(int)PlayerId.P1] == role ? PlayerId.P1 : PlayerId.P2;

    public void Swap()
    {
        (roles[0], roles[1]) = (roles[1], roles[0]);
        RolesChanged?.Invoke();
    }

    public void ResetToDefault()
    {
        roles[(int)PlayerId.P1] = Role.Pilot;
        roles[(int)PlayerId.P2] = Role.Gunner;
        RolesChanged?.Invoke();
    }
}
