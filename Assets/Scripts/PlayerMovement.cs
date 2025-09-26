using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 moveInput;

    public float playerSpeed = 5f;
    public float playerJumpForce = 2f;
    private bool authorizeToMove = true;
    private bool authorizeJump = true;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    //private void Update()
    //{

    //}

    void FixedUpdate()
    {
        if (authorizeToMove)
        {
            moveInput = Vector2.zero;

            if (Input.GetKey(KeyCode.A)) { moveInput.x -= 1; }
            if (Input.GetKey(KeyCode.D)) { moveInput.x += 1; }

            Move();

            moveInput = moveInput.normalized;
        }

        if (authorizeJump && (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.UpArrow)))
        {
            Jump();
        }
    }

    private void Move()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x + playerSpeed * Time.fixedDeltaTime * moveInput.x, rb.linearVelocity.y);
        //rb.MovePosition(new Vector2(rb.position.x, rb.linearVelocity.y) + playerSpeed * Time.fixedDeltaTime * moveInput);
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, playerJumpForce * Time.fixedDeltaTime + rb.linearVelocity.y);
        //rb.linearVelocity += Vector2.up * playerJumpForce * Time.fixedDeltaTime;
        //Debug.Log("Je saute !");
    }
}