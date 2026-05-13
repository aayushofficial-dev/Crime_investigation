using UnityEngine;

public class CrashTrigger : MonoBehaviour
{
    public float hitForce = 500f;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.AddForce((transform.forward + Vector3.up) * hitForce);
            }

            Animator anim = collision.gameObject.GetComponent<Animator>();

            if (anim != null)
            {
                anim.enabled = false;
            }

            Debug.Log("Accident Scene Triggered");
        }
    }
}