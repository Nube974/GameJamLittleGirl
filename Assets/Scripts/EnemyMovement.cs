using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyMovement : MonoBehaviour
{
    [Header("Mouvement")]
    [SerializeField] float movementSpeed = 2f;
    [SerializeField] float chaseDistance = 6f;          // distance d'aggro
    [SerializeField] bool patrolOrChase = true;         // si false: s'arr�te quand pas de cible

    [Header("Patrouille (optionnel)")]
    [SerializeField] Transform[] waypoints;             // 0..1 suffisent
    [SerializeField] float waypointTolerance = 0.05f;

    [Header("Cible")]
    [SerializeField] Transform player;                  // si null: cherche le tag "Player"
    [SerializeField] bool requireLineOfSight = false;   // raycast (facultatif)
    [SerializeField] LayerMask obstacleMask = 0;

    Rigidbody2D rb;
    int wpIndex = 0;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }
    }

    void FixedUpdate()
    {
        bool canChase = DetectRangePlayer();

        if (canChase)
        {
            ChasePlayer();
        }
        else if (patrolOrChase)
        {
            Patrol();
        }
        else
        {
            // immobile (utile pour la tourelle)
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }

        // Flip visuel selon la vitesse en X
        float vx = rb.linearVelocity.x;
        if (Mathf.Abs(vx) > 0.01f)
        {
            Vector3 s = transform.localScale;
            s.x = Mathf.Sign(vx) * Mathf.Abs(s.x);
            transform.localScale = s;
        }
    }

    // -----------------------
    //   FONCTIONS DEMAND�ES
    // -----------------------

    /// <summary>D�placement entre waypoints (ou demi-tour simple s'il n'y en a pas).</summary>
    public void Patrol()
    {
        if (waypoints != null && waypoints.Length > 0)
        {
            Vector2 target = waypoints[wpIndex].position;
            Vector2 dir = (target - (Vector2)transform.position).normalized;

            rb.linearVelocity = new Vector2(dir.x * movementSpeed, rb.linearVelocity.y);

            if (Vector2.Distance(transform.position, target) <= waypointTolerance)
                wpIndex = (wpIndex + 1) % waypoints.Length;
        }
        else
        {
            // Patrouille �pauvre� : avance tout droit.
            // (Tu peux ajouter un script de demi-tour sur trigger/edge selon ton level design.)
            rb.linearVelocity = new Vector2(transform.localScale.x * movementSpeed, rb.linearVelocity.y);
        }
    }

    /// <summary>Se rapproche du joueur (sur l�axe X pour un platformer 2D).</summary>
    public void ChasePlayer()
    {
        if (!player) return;

        float dirX = Mathf.Sign(player.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(dirX * movementSpeed, rb.linearVelocity.y);
    }

    /// <summary>Retourne true si le joueur est dans la port�e (et optionnellement en vue).</summary>
    public bool DetectRangePlayer()
    {
        if (!player) return false;

        if (Vector2.Distance(transform.position, player.position) > chaseDistance)
            return false;

        if (!requireLineOfSight) return true;

        // Ligne de vue (facultatif)
        Vector2 origin = transform.position;
        Vector2 dir = (player.position - transform.position).normalized;
        float dist = Vector2.Distance(transform.position, player.position);

        return !Physics2D.Raycast(origin, dir, dist, obstacleMask);
    }

    // --- Helpers publics utiles pour tes autres scripts (IA de tir/attaque) ---
    public bool IsChasing => DetectRangePlayer();
    public float Speed => movementSpeed;
}
