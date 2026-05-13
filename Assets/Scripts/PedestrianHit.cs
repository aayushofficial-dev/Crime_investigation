using UnityEngine;

public class PedestrianHit : MonoBehaviour
{
    private Animator animator;
    private Rigidbody rb;

    private bool hit = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hit) return;

        if (collision.gameObject.CompareTag("PlayerCar"))
        {
            hit = true;

            animator.enabled = false;

            Vector3 forceDir =
                collision.transform.forward + Vector3.up;

            rb.AddForce(forceDir * 500f);

            Debug.Log("Pedestrian Hit!");
        }
    }
}