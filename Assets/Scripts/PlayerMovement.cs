using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float speed = 8f;

    Rigidbody2D rb;
    float x, y;

    void Awake() => rb = GetComponent<Rigidbody2D>();

    void Update()
    {
        // Prend en charge clavier (WASD/fl�ches) + manettes (stick gauche)
        x = Input.GetAxisRaw("Horizontal");
        y = Input.GetAxisRaw("Vertical");
    }

    void FixedUpdate()
    {

            rb.linearVelocity = new Vector2(x * speed, rb.linearVelocity.y); // platformer (gravit� pour Y)
       
    }

    public void OnMove(InputAction.CallbackContext callback)
    {
        x  = callback.ReadValue<Vector2>().x;
        y  = callback.ReadValue<Vector2>().y;
    }
}