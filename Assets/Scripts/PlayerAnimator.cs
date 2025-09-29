using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    [Header("Refs (auto si vide)")]
    public Animator animator;
    public SpriteRenderer sprite;
    public Transform groundCheck;
    public Vector2 groundSize = new(0.8f, 0.16f);
    public Rigidbody2D rb;          // conseill�
    public LayerMask groundLayer; // pour isGrounded

    [Header("Animator")]
    public string moveBool = "Move";

    [Header("R�glages")]
    public float moveThreshold = 0.05f; // vitesse mini pour dire "on bouge"

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
    }

    void Update()
    {
        // 1) �tat Move
        Vector2 v = rb ? rb.linearVelocity : Vector2.zero;
        bool moving = v.sqrMagnitude > moveThreshold * moveThreshold;
        animator.SetBool(moveBool, moving);

        // 2) SpriteDirectionChecker (flip)
        if (Mathf.Abs(v.x) > 0.001f)
            sprite.flipX = v.x < 0f;

        bool grounded = Physics2D.OverlapBox(groundCheck.position, groundSize, 0f, groundLayer);
        animator.SetBool("Grounded", grounded);
        animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        // si tu utilises Fall : anim.SetFloat("VelY", rb.velocity.y);
    }
}
