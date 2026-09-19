using UnityEngine;

// Pooled debris: falls straight down, destroyed by the player laser, damages the ship on contact.
[RequireComponent(typeof(Collider2D))]
public class Debris : MonoBehaviour, IDamageable, IShipContactHazard
{
    [SerializeField] float cleanupBoundaryY = -6.5f; // below the bottom of the playfield

    int health;
    float moveSpeed;
    int scoreValue;
    IObjectPool<Debris> pool;

    public void Launch(Vector3 position, float moveSpeed, int health, int scoreValue, IObjectPool<Debris> pool)
    {
        transform.position = position;
        transform.rotation = Quaternion.identity;
        this.moveSpeed = moveSpeed;
        this.health = health;
        this.scoreValue = scoreValue;
        this.pool = pool;
    }

    void Update()
    {
        transform.position += Vector3.down * moveSpeed * Time.deltaTime;

        if (transform.position.y < cleanupBoundaryY)
            pool.Release(this);
    }

    // Called by PlayerLaser when it hits this debris.
    public void Hit()
    {
        health--;
        if (health <= 0)
        {
            EventBus.Publish(new ScoreAwardedSignal(scoreValue));
            pool.Release(this);
        }
    }

    // Called by ShipHullCollider when this debris touches the ship.
    public void OnHitShip()
    {
        pool.Release(this);
    }
}
