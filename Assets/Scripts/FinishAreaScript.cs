using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishAreaScript : MonoBehaviour
{
    private float timer = 0f;
    public bool isRunning = false;
    // Start is called before the first frame update
    void Start()
    {
        isRunning = true;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object entering the trigger is the player
        if (other.CompareTag("Player"))
        {
            EndTheGame();
        }
    }

    public void EndTheGame()
    {
        isRunning = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        // CONTINUUU ISHI
    }
}