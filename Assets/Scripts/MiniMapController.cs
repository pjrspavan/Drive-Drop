using UnityEngine;

public class MiniMapFollow : MonoBehaviour
{
    public Transform player;  // Reference to the player's transform

    void LateUpdate()
    {
        // Update the camera position to follow the player
        Vector3 newPosition = player.position;
        newPosition.y = transform.position.y;  // Maintain the camera's height
        transform.position = newPosition;
    }
}
