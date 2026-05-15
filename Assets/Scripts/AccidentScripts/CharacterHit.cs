using UnityEngine;

public class CharacterHit : MonoBehaviour
{
    private Animator animator;
    private Rigidbody rb;

    public bool isHit = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        rb.isKinematic = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Car") && !isHit)
        {
            isHit = true;

            animator.enabled = false;

            rb.isKinematic = false;

            Vector3 hitDirection = (transform.position - collision.transform.position).normalized;

            rb.AddForce((hitDirection + Vector3.up) * 8f, ForceMode.Impulse);

            StartCoroutine(SlowMotionEffect());
        }
    }

    System.Collections.IEnumerator SlowMotionEffect()
    {
        Time.timeScale = 0.4f;

        yield return new WaitForSecondsRealtime(1.5f);

        Time.timeScale = 1f;
    }
}