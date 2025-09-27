using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public Transform muzzle;                       // point de sortie
    public HeartAmmo projectilePrefab;     // ton prefab "coeur"
    public float fireCooldown = 0.12f;

    float nextFireTime;

    void Update()
    {
        if (Time.time < nextFireTime) return;

        // Fire1 = clic gauche / RT / etc. (Input Manager)
        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
            nextFireTime = Time.time + fireCooldown;
        }
    }

    void Shoot()
    {
        var proj = Instantiate(projectilePrefab, muzzle.position, Quaternion.identity);
        var p = proj.transform.position;
        p.z = 0f;
        proj.transform.position = p;
        var projCol = proj.GetComponent<Collider2D>();
        var playerCol = GetComponent<Collider2D>();
        if (projCol && playerCol) Physics2D.IgnoreCollision(projCol, playerCol, true);

        float facing = transform.localScale.x >= 0 ? 1f : -1f;
        proj.Launch(new Vector2(facing, 0f));
    }
}
