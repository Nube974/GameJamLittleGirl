using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class JumpJetPackSystem : MonoBehaviour
{
    [Header("Jump")]
    [SerializeField] private float jumpForce = 12f;   // force du saut
    [SerializeField] public bool jump = true;         // true = Saut, false = Jetpack

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.8f, 0.18f);
    [SerializeField] private LayerMask groundLayer;   // UNIQUEMENT Ground

    [Header("Jetpack")]
    [SerializeField] private float jetpackAccel = 30f;    // accélération vers le haut (m/s²)
    [SerializeField] private float maxJetpackSpeed = 8f;  // vitesse verticale max quand on pousse
    [SerializeField] private float jetpackDuration = 2f;  // réservoir total (secondes de poussée)
    [SerializeField] private float jetpackRechargePerSecond = 1.2f;
    [SerializeField] private bool jetpackResetOnLand = false;

    [Header("Confort")]
    [SerializeField] private float jumpBufferTime = 0.08f;
    [SerializeField] private float lowJumpMultiplier = 0.5f;

    [Header("Switch par Trigger (facultatif)")]
    [SerializeField] private string enterJetpackTag = "JetpackZone";
    [SerializeField] private string exitJetpackTag = "JumpZone";
    [SerializeField] private bool enterOnce = true;



    Rigidbody2D rb;
    float bufferJump;
    float fuel;
    bool jetpackEnteredOnce;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        fuel = jetpackDuration;

        if (!groundCheck)
        {
            var t = transform.Find("groundCheck");
            if (t) groundCheck = t;
        }
    }

    void Update()
    {
        if (Input.GetButtonDown("Jump"))
            bufferJump = jumpBufferTime;

        if (jump && Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0f)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * lowJumpMultiplier);

        if (bufferJump > 0f) bufferJump -= Time.unscaledDeltaTime;
    }

    void FixedUpdate()
    {
        bool grounded = CheckGround();

        if (jump)
        {
            // --- SAUT ---
            if (grounded && bufferJump > 0f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                AudioManager.Instance?.PlayPlayerJump();
                bufferJump = 0f;
            }
        }
        else
        {
            // --- JETPACK ---
            if (grounded)
            {
                if (jetpackResetOnLand) fuel = jetpackDuration;
                else fuel = Mathf.Min(jetpackDuration, fuel + jetpackRechargePerSecond * Time.fixedDeltaTime);
            }

            if (Input.GetButton("Jump") && fuel > 0f)
            {
                // accélère vers maxJetpackSpeed indépendamment de la masse
                float newVy = Mathf.MoveTowards(rb.linearVelocity.y, maxJetpackSpeed, jetpackAccel * Time.fixedDeltaTime);
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, newVy);
                fuel -= Time.fixedDeltaTime;
                AudioManager.Instance?.JetpackStart();

            }
        }
    }

    // --- API ---
    public void SwitchAction(bool useJetpack)
    {
        jump = !useJetpack;
        if (useJetpack && fuel <= 0f)
            fuel = Mathf.Min(jetpackDuration, 0.35f); // petit coup de pouce si le réservoir est à 0
    }

    public bool CheckGround()
    {
        if (!groundCheck) return false;
        return Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(enterJetpackTag))
        {
            if (enterOnce && jetpackEnteredOnce) return;
            jetpackEnteredOnce = true;
            SwitchAction(useJetpack: true);
            // au début de l’utilisation

        }
        else if (other.CompareTag(exitJetpackTag))
        {
            SwitchAction(useJetpack: false);
            // quand on relâche/arrête
        }
    }

    // Augmente la capacité (durée totale) du jetpack.
    // alsoRefill = true : on ajoute aussi la même quantité au réservoir courant.
    public void AddJetpackDuration(float extraSeconds, bool alsoRefill = true)
    {
        if (extraSeconds <= 0f) return;
        jetpackDuration = Mathf.Max(0f, jetpackDuration + extraSeconds);
        if (alsoRefill) fuel = Mathf.Min(jetpackDuration, fuel + extraSeconds);
    }

    // Remplit le réservoir (sans toucher à la capacité).
    public void AddFuel(float seconds)
    {
        if (seconds <= 0f) return;
        fuel = Mathf.Min(jetpackDuration, fuel + seconds);
    }

        void OnDrawGizmosSelected()
    {
        if (!groundCheck) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
    }

    // Helpers
    public float Fuel01 => Mathf.Clamp01(fuel / Mathf.Max(0.0001f, jetpackDuration));
    public bool IsJetpacking => !jump && Input.GetButton("Jump") && fuel > 0f;
}
