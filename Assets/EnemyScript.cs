using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    private bool isDead = false;
    private float curRespawnTime = 0f;
    public float respawnTime = 5f;

    // Update is called once per frame
    void Update()
    {
        if (isDead) curRespawnTime+=Time.deltaTime;
        if (curRespawnTime > respawnTime)
        {
            isDead = false;
            GetComponent<SpriteRenderer>().enabled = true;
            GetComponent<CircleCollider2D>().enabled = true;
        }

    }

    public void Die()
    {
        isDead = true;
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<CircleCollider2D>().enabled = false;
    }
}
