using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EnemyAnimator : MonoBehaviour
{
    [Header("Refs (auto si vide)")]
    public Animator animator;
    public SpriteRenderer sprite;
    public Transform groundCheck;                 // optionnel
    public Vector2 groundSize = new(0.8f, 0.16f); // optionnel
    public Rigidbody2D rb;                        // conseillé (peut être sur le parent)
    public LayerMask groundLayer;                 // optionnel si Grounded utilisé

    [Header("Animator Params")]
    public string moveBool = "Move";     // Idle/Run
    public string groundedBool = "Grounded"; // si tu l'utilises
    public string speedFloat = "Speed";    // si tu l'utilises

    [Header("Flip / Cible")]
    public bool faceTarget = false;          // TRUE = regarde toujours target
    public Transform target;                 // joueur à suivre (si faceTarget)

    [Header("Seuils & Lissage")]
    public float enterRun = 0.12f;           // au-dessus => Run
    public float exitRun = 0.08f;           // en-dessous => Idle
    public float tinyKill = 0.02f;           // micro-vitesse clampée à 0
    public float smooth = 0.12f;           // lissage expo pour Speed

    [Header("Safeguard (si transition Run→Idle capricieuse)")]
    public bool useSafeguard = true;
    public string runStateName = "Run";     // nom EXACT de l’état Run
    public string idleStateName = "Idle";    // nom EXACT de l’état Idle
    public float crossFadeTime = 0.05f;

    Vector3 lastPos;
    float smoothedSpeed;

    void Reset()
    {
        animator = GetComponent<Animator>();
        sprite = GetComponentInChildren<SpriteRenderer>();
        rb = GetComponentInParent<Rigidbody2D>();
    }

    void Awake()
    {
        if (!animator) animator = GetComponent<Animator>();
        if (!sprite) sprite = GetComponentInChildren<SpriteRenderer>();
        if (!rb) rb = GetComponentInParent<Rigidbody2D>();
        lastPos = transform.position;
    }

    void Update()
    {
        // ---- vitesse horizontale (RB si dispo, sinon delta position)
        float vx = rb ? rb.linearVelocity.x
                      : (transform.position.x - lastPos.x) / Mathf.Max(Time.deltaTime, 0.0001f);

        // tue la micro-vitesse pour éviter le jitter autour de 0
        if (Mathf.Abs(vx) < tinyKill) vx = 0f;
        if (rb && Mathf.Abs(rb.linearVelocity.x) < tinyKill)
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        float ax = Mathf.Abs(vx);

        // ---- hysteresis Move (évite pompage)
        bool moving = animator.GetBool(moveBool);
        if (!moving && ax > enterRun) moving = true;
        else if (moving && ax < exitRun) moving = false;
        animator.SetBool(moveBool, moving);

        // ---- Speed lissée (si param utilisé / blend tree)
        if (!string.IsNullOrEmpty(speedFloat))
        {
            smoothedSpeed = Mathf.Lerp(smoothedSpeed, ax, 1f - Mathf.Exp(-smooth * Time.deltaTime));
            animator.SetFloat(speedFloat, smoothedSpeed);
        }

        // ---- Grounded (optionnel)
        if (groundCheck && !string.IsNullOrEmpty(groundedBool))
        {
            bool grounded = Physics2D.OverlapBox(groundCheck.position, groundSize, 0f, groundLayer);
            animator.SetBool(groundedBool, grounded);
        }

        // ---- SpriteDirectionChecker (flip)
        if (faceTarget && target)
        {
            float dir = target.position.x - transform.position.x;
            if (Mathf.Abs(dir) > 0.0001f) sprite.flipX = dir < 0f;
        }
        else
        {
            if (Mathf.Abs(vx) > 0.001f && sprite) sprite.flipX = vx < 0f;
        }

        // ---- Safeguard : si Move=false mais on reste bloqué en Run, on force Idle
        if (useSafeguard)
        {
            var st = animator.GetCurrentAnimatorStateInfo(0);
            bool isRun = (!string.IsNullOrEmpty(runStateName) && st.IsName(runStateName));
            bool wantIdle = !animator.GetBool(moveBool);
            if (wantIdle && isRun && !string.IsNullOrEmpty(idleStateName))
            {
                animator.CrossFade(idleStateName, crossFadeTime, 0, 0f);
            }
        }

        lastPos = transform.position;
    }

    // Gizmo pour le groundCheck
    void OnDrawGizmosSelected()
    {
        if (!groundCheck) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(groundCheck.position, groundSize);
    }
}
