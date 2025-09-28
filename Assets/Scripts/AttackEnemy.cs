using UnityEngine;

public class AttackEnemy : MonoBehaviour
{
    public enum AttackType { Melee, Range, MeleeAndRange }

    [Header("Diagramme")]
    [SerializeField] float attackDistance = 6f;   // portée d’attaque (tir)
    [SerializeField] int damage = 10;
    [SerializeField] AttackType attackType = AttackType.Melee;

    [Header("Cible")]
    [SerializeField] Transform player;            // auto-find si null
    [SerializeField] LayerMask playerMask = ~0;   // couche du joueur (pour le melee)

    [Header("Melee")]
    [SerializeField] Transform meleePoint;        // point de frappe
    [SerializeField] float meleeRadius = 0.6f;
    [SerializeField] float meleeCooldown = 0.6f;

    [Header("Range")]
    [SerializeField] Transform firePoint;         // sortie du projectile
    [SerializeField] EnemyAmmo projectilePrefab;
    [SerializeField] float fireCooldown = 0.35f;
    [SerializeField] float projectileSpeed = 18f;

    float nextMeleeTime;
    float nextFireTime;

    void Awake()
    {
        if (!player)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }
    }

    void Update()
    {
        if (!player) return;

        bool canShoot = PlayerInRange(attackDistance);
        bool canMelee = PlayerInRange(meleeRadius * 1.05f); // très proche

        switch (attackType)
        {
            case AttackType.Melee:
                if (canMelee) EnemyMelee();
                break;

            case AttackType.Range:
                if (canShoot) Fire();
                break;

            case AttackType.MeleeAndRange:
                if (canMelee) EnemyMelee();           // priorité au melee
                else if (canShoot) Fire();
                break;
        }
    }

    // --- Fonctions du diagramme ---

    /// <summary>Vrai si le joueur est dans 'distance' autour de l’ennemi.</summary>
    public bool PlayerInRange(float distance)
    {
        if (!player) return false;
        return Vector2.Distance(transform.position, player.position) <= distance;
    }

    /// <summary>Tir un projectile vers le joueur (attaque distance).</summary>
    public void Fire()
    {
        if (Time.time < nextFireTime || !projectilePrefab || !firePoint) return;
        nextFireTime = Time.time + fireCooldown;

        // direction vers le joueur
        Vector2 dir = ((Vector2)(player.position - firePoint.position)).normalized;

        // décale légèrement le spawn devant le canon
        Vector3 spawnPos = firePoint.position + (Vector3)(dir * 0.25f);

        var proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        // ignorer la collision avec l'ennemi tireur
        var projCol = proj.GetComponent<Collider2D>();
        var selfCol = GetComponent<Collider2D>(); // mets le collider racine de l’ennemi
        if (projCol && selfCol) Physics2D.IgnoreCollision(projCol, selfCol, true);

        proj.speed = projectileSpeed;
        proj.Launch(dir);
    }

    /// <summary>Inflige des dégâts en zone autour de 'meleePoint'.</summary>
    public void EnemyMelee()
    {
        if (Time.time < nextMeleeTime || !meleePoint) return;

        nextMeleeTime = Time.time + meleeCooldown;

        var hit = Physics2D.OverlapCircle((Vector2)meleePoint.position, meleeRadius, playerMask);
        //if (hit)
        {
            // Si tu as un système de vie :
            //hit.GetComponent<IHealth>()?.TakeDamage(damage);
            // Sinon provisoire :
            // Debug.Log("Melee hit " + hit.name);
        }
    }

    public bool IsInShootRange()
    {
        if (attackType == AttackType.Melee) return false;
        return PlayerInRange(attackDistance);
    }

    // Gizmos pour régler facilement les portées
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackDistance);

        if (meleePoint)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(meleePoint.position, meleeRadius);
        }
        if (!firePoint) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(firePoint.position, 0.05f);
        Gizmos.DrawLine(firePoint.position, firePoint.position + firePoint.right * 0.5f);
    }
}
