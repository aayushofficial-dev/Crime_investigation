using UnityEngine;

public class RainFollow : MonoBehaviour
{
    public Transform player;

    void Update()
    {
        transform.position =
            new Vector3(
                player.position.x,
                player.position.y + 15f,
                player.position.z
            );
    }
}