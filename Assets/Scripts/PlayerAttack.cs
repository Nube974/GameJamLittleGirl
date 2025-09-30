using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Refs")]
    public Transform muzzle;                 // point de sortie (optionnel)
    public HeartAmmo projectilePrefab;

    [Header("Tir")]
    public float fireCooldown = 0.12f;

    [Header("Auto-aim (ennemi le plus proche)")]
    public bool aimNearestEnemy = true;
    public float aimRadius = 20f;            // distance max pour chercher
    public LayerMask enemyLayers;            // coche tes layers d’ennemis
    public bool requireLineOfSight = false;  // true = vérifie qu'il n'y a pas d'obstacle
    public LayerMask obstacleLayers;         // couches à considérer comme obstacles (murs, décor)

    float nextFireTime;
    SpriteRenderer sprite;
    Collider2D[] myCols;

    void Awake()
    {
        sprite = GetComponentInChildren<SpriteRenderer>();
        myCols = GetComponentsInChildren<Collider2D>();
    }

    void Update()
    {
        if (Time.time < nextFireTime) return;

        if (Input.GetButtonDown("Shoot"))
        {
            Shoot();
            nextFireTime = Time.time + fireCooldown;
        }
    }

    void Shoot()
    {
        Vector3 spawnPos = muzzle ? muzzle.position : transform.position;

        // 1) Direction = vers l’ennemi le plus proche si possible
        Vector2 dir = aimNearestEnemy ? GetDirectionToNearestEnemy(spawnPos) : Vector2.zero;
        if (dir.sqrMagnitude < 0.0001f) // fallback: direction où le perso regarde
            dir = GetFacingDir();

        // 2) Instanciation
        var proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        // 3) Ignore collisions avec TOUS les colliders du joueur
        var projCol = proj.GetComponent<Collider2D>();
        if (projCol != null && myCols != null)
            foreach (var c in myCols) if (c) Physics2D.IgnoreCollision(projCol, c, true);

        // 4) Lance
        proj.Launch(dir.normalized, myCols);
    }

    // ----- Helpers -----

    Vector2 GetDirectionToNearestEnemy(Vector3 origin)
    {
        // Cherche tous les colliders d’ennemis dans un rayon (performant et simple)
        var hits = Physics2D.OverlapCircleAll(origin, aimRadius, enemyLayers);
        if (hits == null || hits.Length == 0) return Vector2.zero;

        Collider2D best = null;
        float bestSqr = float.PositiveInfinity;

        for (int i = 0; i < hits.Length; i++)
        {
            var h = hits[i];
            if (!h || !h.gameObject.activeInHierarchy) continue;

            Vector2 center = h.bounds.center;
            float d2 = (center - (Vector2)origin).sqrMagnitude;
            if (d2 >= bestSqr) continue;

            // Option: ligne de vue (raycast jusqu'à l'ennemi)
            if (requireLineOfSight)
            {
                Vector2 dir = (center - (Vector2)origin).normalized;
                float dist = Mathf.Sqrt(d2);
                var hit = Physics2D.Raycast(origin, dir, dist, obstacleLayers);
                if (hit.collider != null) continue; // obstacle entre les deux
            }

            best = h; bestSqr = d2;
        }

        if (!best) return Vector2.zero;
        return ((Vector2)best.bounds.center - (Vector2)origin);
    }

    Vector2 GetFacingDir()
    {
        if (muzzle) return muzzle.right;                     // le plus fiable si tu orientes le muzzle (0°=droite, 180°=gauche)
        if (sprite) return sprite.flipX ? Vector2.left : Vector2.right; // sinon via flipX
        float s = Mathf.Sign(transform.lossyScale.x);        // fallback via scale
        return s < 0f ? Vector2.left : Vector2.right;
    }

    // Debug visuel dans la Scene
    void OnDrawGizmosSelected()
    {
        if (aimNearestEnemy)
        {
            Gizmos.color = new Color(0f, 0.6f, 1f, 0.25f);
            Vector3 o = muzzle ? muzzle.position : transform.position;
            Gizmos.DrawWireSphere(o, aimRadius);
        }
    }
}
