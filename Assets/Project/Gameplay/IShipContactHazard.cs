// Anything that deals hull damage on contact with the ship and is consumed by it (Enemy, Debris).
// Breach hazard deliberately does NOT implement this - the GDD only lists it colliding with the
// bottom boundary, not the ship.
public interface IShipContactHazard
{
    void OnHitShip();
}
