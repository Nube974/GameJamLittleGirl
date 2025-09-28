using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyMovement : MonoBehaviour
{
    [Header("Mouvement")]
    [SerializeField] float movementSpeed = 2f;
    [SerializeField] float chaseDistance = 6f;
    [SerializeField] bool patrolOrChase = true;

    [Header("Patrouille (waypoints enfants du prefab)")]
    [SerializeField] Transform waypointsRoot;   // Empty enfant "Waypoints"
    [SerializeField] bool pingPong = true;      // aller-retour W0<->W1
    [SerializeField] float waypointTolerance = 0.2f;
    [SerializeField] float waitAtPoint = 0f;
    [SerializeField] bool detachWaypointsAtRuntime = false; // option: détacher au Start

    [Header("Cible (chase)")]
    [SerializeField] Transform player;

    Rigidbody2D rb;

    // --- Données figées au runtime ---
    Vector2[] wpWorld;    // positions monde figées (NE BOUGENT PLUS)
    int wpIndex = 0, dirSign = 1;
    float waitTimer = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (!player)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }
    }

    void Start()
    {
        // 1) trouver le holder des waypoints
        if (!waypointsRoot) waypointsRoot = transform.Find("Waypoints");

        // 2) BAKER les positions monde des enfants (on ignore le root lui-même)
        if (waypointsRoot)
        {
            var kids = waypointsRoot.GetComponentsInChildren<Transform>(includeInactive: true);
            if (kids.Length > 1)
            {
                wpWorld = new Vector2[kids.Length - 1];
                for (int i = 1; i < kids.Length; i++)
                    wpWorld[i - 1] = kids[i].position; // POS MONDE FIGÉE
            }

            // 3) Option : on détache pour ne plus qu’ils suivent le parent (pur confort)
            if (detachWaypointsAtRuntime)
            {
                var holder = new GameObject($"{name}_WaypointsRuntime").transform;
                holder.position = Vector3.zero; // scène
                for (int i = 1; i < kids.Length; i++)
                    kids[i].SetParent(holder, true); // gardent leur pos monde
            }
        }
    }

    void FixedUpdate()
    {
        // Si tu as un AttackEnemy avec tir à distance, stoppe quand on est à portée
        var atk = GetComponent<AttackEnemy>();
        if (atk && atk.IsInShootRange())
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        bool canChase = DetectRangePlayer();

        if (canChase) ChasePlayer();
        else if (patrolOrChase) Patrol();
        else rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        // Flip visuel sur l'axe X selon la vitesse
        if (Mathf.Abs(rb.linearVelocity.x) > 0.01f)
        {
            var s = transform.localScale; s.x = Mathf.Sign(rb.linearVelocity.x) * Mathf.Abs(s.x);
            transform.localScale = s;
        }
    }

    // -------- Patrouille QUI N'UTILISE QUE wpWorld --------
    void Patrol()
    {
        if (wpWorld == null || wpWorld.Length == 0)
        {
            rb.linearVelocity = new Vector2(transform.localScale.x * movementSpeed, rb.linearVelocity.y);
            return;
        }

        if (waitTimer > 0f)
        {
            waitTimer -= Time.fixedDeltaTime;
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        Vector2 pos = rb.position;
        Vector2 target = wpWorld[wpIndex];

        float dx = target.x - pos.x;
        float step = movementSpeed * Time.fixedDeltaTime;

        // Arrivé (ou overshoot) -> clip au point, puis next
        if (Mathf.Abs(dx) <= waypointTolerance || Mathf.Abs(dx) <= step)
        {
            rb.position = new Vector2(target.x, pos.y);
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            NextWaypoint();
            waitTimer = waitAtPoint;
            return;
        }

        float dirX = Mathf.Sign(dx);
        rb.linearVelocity = new Vector2(dirX * movementSpeed, rb.linearVelocity.y);
    }

    void NextWaypoint()
    {
        if (!pingPong)
        {
            wpIndex = (wpIndex + 1) % wpWorld.Length;
        }
        else
        {
            if (wpIndex == 0) dirSign = 1;
            else if (wpIndex == wpWorld.Length - 1) dirSign = -1;
            wpIndex += dirSign;
        }
    }

    // -------- Chase / Détection simples --------
    void ChasePlayer()
    {
        if (!player) return;
        float dirX = Mathf.Sign(player.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(dirX * movementSpeed, rb.linearVelocity.y);
    }

    bool DetectRangePlayer()
    {
        if (!player) return false;
        return Vector2.Distance(transform.position, player.position) <= chaseDistance;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        // Affiche le chemin figé si possible
        if (wpWorld != null && wpWorld.Length > 0)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < wpWorld.Length; i++)
            {
                Gizmos.DrawSphere(wpWorld[i], 0.06f);
                if (i + 1 < wpWorld.Length) Gizmos.DrawLine(wpWorld[i], wpWorld[i + 1]);
            }
        }
        else if (waypointsRoot) // sinon affiche les enfants actuels (édition)
        {
            var kids = waypointsRoot.GetComponentsInChildren<Transform>();
            Gizmos.color = Color.yellow;
            for (int i = 1; i < kids.Length; i++)
            {
                Gizmos.DrawSphere(kids[i].position, 0.06f);
                if (i + 1 < kids.Length) Gizmos.DrawLine(kids[i].position, kids[i + 1].position);
            }
        }
    }
#endif
}
