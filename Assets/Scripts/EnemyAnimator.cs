using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EnemyAnimator : MonoBehaviour
{
    public Animator anim;
    public Rigidbody2D rb;           // si le RB est sur le parent, glisse-le ici
    public Transform movingRoot;     // l’objet qui se déplace réellement (souvent le parent)

    [Header("Paramètre Blend Tree")]
    public string speedFloat = "Speed";

    [Header("Réglages")]
    public float tinyKill = 0.02f;   // micro-vitesse annulée
    public float smooth = 0.12f;   // lissage expo (0.1–0.2)
    public bool useAbsX = true;    // true: on anime sur la vitesse horizontale; false: magnitude 2D

    Vector3 lastPos;
    float smoothed;

    void Reset()
    {
        anim = GetComponent<Animator>();
        rb = GetComponentInParent<Rigidbody2D>();
        movingRoot = transform.root;
    }

    void Awake()
    {
        if (!anim) anim = GetComponent<Animator>();
        if (!rb) rb = GetComponentInParent<Rigidbody2D>();
        if (!movingRoot) movingRoot = transform.root;
        lastPos = (movingRoot ? movingRoot.position : transform.position);
    }

    void Update()
    {
        // 1) calcule la vitesse
        Vector2 vel = Vector2.zero;

        // a) priorité: rb.velocity si ça reflète le mouvement
        if (rb) vel = rb.linearVelocity;

        // b) fallback: delta position du root (utile si MovePosition, Translate, ou kinematic)
        if (vel.sqrMagnitude < 0.0001f)
        {
            Vector3 cur = movingRoot ? movingRoot.position : transform.position;
            Vector3 delta = (cur - lastPos);
            float dt = Mathf.Max(Time.deltaTime, 0.0001f);
            vel = delta / dt;
            lastPos = cur;
        }

        // 2) choix de la métrique et nettoyage des micro-vitesses
        float speed = useAbsX ? Mathf.Abs(vel.x) : vel.magnitude;
        if (speed < tinyKill) speed = 0f;

        // 3) lissage et push vers l’Animator
        smoothed = Mathf.Lerp(smoothed, speed, 1f - Mathf.Exp(-smooth * Time.deltaTime));
        anim.SetFloat(speedFloat, smoothed);

        // DEBUG (temporaire) : décommente si besoin
        // Debug.Log($"[EnemyAnimatorBlend] raw={speed:F3} smoothed={smoothed:F3}");
    }
}
