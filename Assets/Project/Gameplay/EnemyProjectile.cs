using UnityEngine;

// Pooled enemy projectile: travels straight down, releases at the cleanup boundary, damages the
// ship on contact (ShipHullCollider already checks for IShipContactHazard - no changes needed there).
// Not IDamageable - the GDD's laser-collision list is enemies/debris/breach hazards only.
[RequireComponent(typeof(Collider2D))]
public class EnemyProjectile : MonoBehaviour, IShipContactHazard
{
    [SerializeField] float cleanupBoundaryY = -6.5f; // below the bottom of the playfield

    Vector2 direction;
    float speed;
    IObjectPool<EnemyProjectile> pool;

    public void Launch(Vector3 position, Vector2 direction, float speed, IObjectPool<EnemyProjectile> pool)
    {
        transform.position = position;
        transform.rotation = Quaternion.identity;
        this.direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.down;
        this.speed = speed;
        this.pool = pool;
    }

    void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        if (transform.position.y < cleanupBoundaryY)
            pool.Release(this);
    }

    // Called by ShipHullCollider when this projectile hits the ship.
    public void OnHitShip()
    {
        pool.Release(this);
    }
}
