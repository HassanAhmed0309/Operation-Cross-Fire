using System;

// What other layers resolve from ServiceLocator to read/change which player holds which role.
public interface IRoleAssignment
{
    event Action RolesChanged;

    Role GetRole(PlayerId player);
    PlayerId GetPlayer(Role role);
    void Swap();
    void ResetToDefault();
}
