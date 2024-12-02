using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnerScript : MonoBehaviour
{
    [SerializeField]
    private GameObject enemyPrefab;
    public GameObject enemy;
    public float curRespawnTime = 0f;
    public float respawnTime = 5f;
    public bool isDead;

    private bool IsDead()
    {
        return (enemy == null);
    }

    // Start is called before the first frame update
    void Start()
    {
        enemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
        isDead = false;
    }

    // Update is called once per frame
    void Update()
    {
        isDead = IsDead();
        if (isDead) curRespawnTime += Time.deltaTime;
        if (curRespawnTime > respawnTime)
        {
            enemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
            curRespawnTime = 0;
        }
    }
}
