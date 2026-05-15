using UnityEngine;

public class CarMovement : MonoBehaviour
{
    public float speed = 18f;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // Increase volume
        audioSource.volume = 1f;
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        // Engine pitch based on speed
        audioSource.pitch = 1f + (speed / 40f);
    }
}