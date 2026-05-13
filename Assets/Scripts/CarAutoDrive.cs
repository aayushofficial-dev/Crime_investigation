using UnityEngine;

public class CarAutoDrive : MonoBehaviour
{
    public float speed = 15f;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        rb.MovePosition(
            rb.position + transform.forward * speed * Time.fixedDeltaTime
        );
    }
}