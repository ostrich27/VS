using UnityEngine;

public class AxeGravity : MonoBehaviour
{
    public float gravity = 9.81f;
    Rigidbody2D rb;

    void Awake() => rb = GetComponent<Rigidbody2D>();

    void FixedUpdate()
    {
        rb.velocity += Vector2.down * gravity * Time.fixedDeltaTime;
    }
}