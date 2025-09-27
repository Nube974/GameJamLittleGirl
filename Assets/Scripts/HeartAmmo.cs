using UnityEngine;

public class HeartAmmo : MonoBehaviour
{
    public float speed = 20f;
    public float maxDistance = 12f;
    public int damage = 10;

    Vector2 dir;
    Vector2 startPos;

    public void Launch(Vector2 direction)
    {
        dir = direction.normalized;
        startPos = transform.position;
    }

    void Update()
    {
        transform.position += (Vector3)(dir * speed * Time.deltaTime);
        if (Vector2.Distance(startPos, transform.position) >= maxDistance)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) return; // ignore le joueur

        // Option simple : touche un ennemi (tag "Enemy") -> dégâts
        if (other.CompareTag("Enemy"))
        {
            // si tu as une interface/health, appelle-la ici
            // other.GetComponent<IHealth>()?.TakeDamage(damage);
        }

        Destroy(gameObject); // s’arrête à l’impact
    }
}
