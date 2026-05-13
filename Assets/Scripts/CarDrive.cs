using UnityEngine;

public class CarDrive : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float speed = 12f;

    // Update is called once per frame
    void Update()
    {
       transform.Translate(Vector3.forward * speed * Time.deltaTime); 
    }
}
