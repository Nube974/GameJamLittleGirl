using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class JumpJetPackSystem : MonoBehaviour
{
    [Header("R�glages")]
    [SerializeField] float jumpForce = 12f;
    [SerializeField] Transform groundCheck;         // un empty sous les pieds
    [SerializeField] Vector2 groundCheckSize = new Vector2(0.8f, 0.1f);
    [SerializeField] LayerMask groundLayer;         // couche du sol

    Rigidbody2D rb;
    bool jumpPressed;

    void Awake() => rb = GetComponent<Rigidbody2D>();

    void Update()
    {
        // Prend en charge clavier + manettes via l'axe "Jump" du Input Manager
        if (Input.GetButtonDown("Jump")) jumpPressed = true;
        // Option: saut variable (rel�che)
        if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0f)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
    }

    void FixedUpdate()
    {
        bool grounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);

        if (grounded && jumpPressed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        jumpPressed = false; // on consomme l'input � chaque FixedUpdate
    }

    // Pour visualiser la zone de d�tection du sol dans la Scene
    void OnDrawGizmosSelected()
    {
        if (!groundCheck) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
    }
}
