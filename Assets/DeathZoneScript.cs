using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathZoneScript : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the player entered the trigger zone
        if (other.CompareTag("Player"))
        {
            PlayerMovement playerScript = other.GetComponent<PlayerMovement>();
            if (playerScript != null)
            {
                playerScript.Respawn(new Vector2(0, 0)); // Call the respawn function
            }
        }
    }
}
