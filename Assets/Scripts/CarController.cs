using UnityEngine;

public class CarController : MonoBehaviour
{
    public float speed = 10f;
    public float turnSpeed = 100f;

    void Update()
    {
        float move = Input.GetAxis("Vertical");   // W/S or Up/Down
        float turn = Input.GetAxis("Horizontal"); // A/D or Left/Right

        transform.Translate(Vector3.forward * move * speed * Time.deltaTime);
        transform.Rotate(Vector3.up * turn * turnSpeed * Time.deltaTime);
    }
}