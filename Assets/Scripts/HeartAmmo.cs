using UnityEngine;

public class HeartAmmo : MonoBehaviour
{
    public float speed = 22f;
    public float maxDistance = 20f;
    public int damage = 10;

    [Tooltip("Couches qui doivent prendre des dégâts (ex: Enemy)")]
    public LayerMask hitMask;

    Vector2 dir, startPos;
    Collider2D[] ownerCols; // colliders du tireur à ignorer

    public void Launch(Vector2 direction, Collider2D[] ownerToIgnore = null)
    {
        dir = direction.sqrMagnitude > 0 ? direction.normalized : Vector2.right;
        startPos = transform.position;
        ownerCols = ownerToIgnore;

        // ignore le joueur (tous ses colliders)
        var myCol = GetComponent<Collider2D>();
        if (myCol != null && ownerCols != null)
            foreach (var c in ownerCols) if (c) Physics2D.IgnoreCollision(myCol, c, true);
    }

    void Update()
    {
        transform.position += (Vector3)(dir * speed * Time.deltaTime);
        if (Vector2.Distance(startPos, transform.position) >= maxDistance)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // ignore le tireur
        if (ownerCols != null) foreach (var c in ownerCols) if (other == c) return;

        // ne réagit qu'aux layers ciblées
        if ((hitMask.value & (1 << other.gameObject.layer)) == 0) return;

        // récupère Health même si le collider est sur un enfant
        var hp = other.GetComponent<Health>() ?? other.GetComponentInParent<Health>();
        if (hp != null) hp.TakeDamage(damage);

        Destroy(gameObject);
    }
}
