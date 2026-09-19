using UnityEngine;

// Pooled player laser: travels in a fixed direction, releases itself past the cleanup boundary.
// No collision/damage yet - nothing to hit exists until enemies/debris/hazards land.
public class PlayerLaser : MonoBehaviour
{
    [SerializeField] float cleanupBoundary = 12f; // world units from origin, either axis

    Vector2 direction;
    float speed;
    IObjectPool<PlayerLaser> pool;

    public void Launch(Vector3 position, Vector2 direction, float speed, IObjectPool<PlayerLaser> pool)
    {
        transform.position = position;
        transform.rotation = Quaternion.identity;
        this.direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.up;
        this.speed = speed;
        this.pool = pool;
    }

    void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        Vector3 position = transform.position;
        if (Mathf.Abs(position.x) > cleanupBoundary || Mathf.Abs(position.y) > cleanupBoundary)
            pool.Release(this);
    }
}
