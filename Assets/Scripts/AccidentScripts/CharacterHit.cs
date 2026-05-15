using System.Collections;
using UnityEngine;

public class CharacterHit : MonoBehaviour
{
    public Transform car;

    private bool hit = false;

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Car") && !hit)
        {
            hit = true;

            // Disable Character Controller
            CharacterController cc =
                GetComponent<CharacterController>();

            if (cc != null)
            {
                cc.enabled = false;
            }

            // Disable Animator
            if (animator != null)
            {
                animator.enabled = false;
            }

            StartCoroutine(HitReaction());

            StartCoroutine(SlowMotion());
        }
    }

    IEnumerator HitReaction()
    {
        Vector3 startPos = transform.position;

        Vector3 hitDirection =
            (transform.position - car.position).normalized;

        Vector3 targetPos =
            startPos + hitDirection * 2f;

        targetPos.y = startPos.y;

        float time = 0;

        while (time < 0.4f)
        {
            transform.position =
                Vector3.Lerp(startPos, targetPos, time / 0.4f);

            time += Time.deltaTime;

            yield return null;
        }
    }

    IEnumerator SlowMotion()
    {
        Time.timeScale = 0.4f;

        yield return new WaitForSecondsRealtime(1.2f);

        Time.timeScale = 1f;
    }
}