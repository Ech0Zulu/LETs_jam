using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public PlayerMovement player; // Assign the player transform here
    private Transform playerTransform;
    public float smoothSpeed = 1f; // Adjust for smoother or quicker following
    public Vector3 offset = new Vector3(0, 0, -20); // Offset between camera and player

    void Start()
    {
        playerTransform = player.transform;
        transform.position = playerTransform.position + offset;
    }

    void LateUpdate()
    {
        if (player == null) return;

        // Desired position for the camera
        Vector3 desiredPosition = playerTransform.position + offset;
        if (player.isGrounded) desiredPosition.y = transform.position.y;

        // Smoothly interpolate to the desired position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}
