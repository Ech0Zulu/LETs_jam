using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements.Experimental;

public class FlowGauge : MonoBehaviour
{
    public Slider FLOWSlider; // Reference to the Slider
    public float maxFLOW = 100f; // Maximum value of FLOW
    public PlayerMovement player; // Reference to the player script
    public float value = 0f;

    void Start()
    {
        player = FindObjectOfType<PlayerMovement>(); // Find the player in the scene
        maxFLOW = player.ultimateMaxSpeed;
        FLOWSlider.maxValue = maxFLOW; // Set the slider's max value
    }

    void Update()
    {
        if (player != null)
        {
            FLOWSlider.value = Mathf.Clamp(player.FLOW, 0, maxFLOW); // Update the slider
        }
    }
}
