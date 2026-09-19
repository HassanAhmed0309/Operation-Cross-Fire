using UnityEngine;

// Pooled breach hazard: falls straight down, destroyed by the player laser for score, but does NOT
// damage the ship on contact (not in the GDD's ship-collision list). Reaching the bottom boundary is
// an instant loss, independent of hull.
[RequireComponent(typeof(Collider2D))]
public class BreachHazard : MonoBehaviour, IDamageable
{
    [SerializeField] float bottomBoundaryY = -5f; // reaching this = instant loss

    int health;
    float moveSpeed;
    int scoreValue;
    IObjectPool<BreachHazard> pool;

    public void Launch(Vector3 position, float moveSpeed, int health, int scoreValue, IObjectPool<BreachHazard> pool)
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

        if (transform.position.y < bottomBoundaryY)
        {
            EventBus.Publish(new BreachHazardEscapedSignal());
            pool.Release(this);
        }
    }

    // Called by PlayerLaser when it hits this breach hazard.
    public void Hit()
    {
        health--;
        if (health <= 0)
        {
            EventBus.Publish(new ScoreAwardedSignal(scoreValue));
            pool.Release(this);
        }
    }
}
