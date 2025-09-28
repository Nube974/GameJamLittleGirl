using UnityEngine;

public class EnemyAmmo : MonoBehaviour
{
    public float speed = 20f;
    public float maxDistance = 20f;
    public float lifeTime = 3f;
    public LayerMask hitMask = ~0;        // ce qui peut détruire le projectile
    public int damage = 10;

    Vector2 dir, startPos;
    float t;
    Collider2D[] ignored;                 // colliders du tireur à ignorer
    const float spawnSafeTime = 0.08f;    // grâce anti-collision instantanée

    public void Launch(Vector2 direction, Collider2D[] ownerToIgnore = null)
    {
        dir = direction.sqrMagnitude > 0 ? direction.normalized : Vector2.right;
        startPos = transform.position;
        t = 0f;
        ignored = ownerToIgnore;

        // Ignore les collisions avec le tireur
        var myCol = GetComponent<Collider2D>();
        if (myCol && ignored != null)
            foreach (var c in ignored) if (c) Physics2D.IgnoreCollision(myCol, c, true);

        // Assure Z=0 en 2D
        var p = transform.position; p.z = 0; transform.position = p;
    }

    void Update()
    {
        transform.position += (Vector3)(dir * speed * Time.deltaTime);
        Debug.DrawLine(startPos, transform.position, Color.magenta, 0f, false); // trajectoire

        t += Time.deltaTime;
        if (t >= lifeTime || Vector2.Distance(startPos, transform.position) >= maxDistance)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Temps de grâce après le spawn
        if (t < spawnSafeTime) return;

        Debug.Log($"[Projectile] hit {other.name} | layer={LayerMask.LayerToName(other.gameObject.layer)}");

        // Ignore le tireur si tu stockes ses colliders
        if (ignored != null) foreach (var c in ignored) if (other == c) return;

        // ignore les autres projectiles (même layer)
        if (other.gameObject.layer == gameObject.layer) return;

        // Trouve la Health même si le collider est sur un enfant
        var hp = other.GetComponent<Health>() ?? other.GetComponentInParent<Health>();
        if (hp != null)
        {
            Debug.Log("[Projectile] found Health -> TakeDamage");
            hp.TakeDamage(damage);
        }

        Destroy(gameObject);
    }

}
