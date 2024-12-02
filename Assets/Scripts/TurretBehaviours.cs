using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectilSpawnBehaviours : MonoBehaviour
{
    [SerializeField]
    private GameObject projectilPrefab;
    [SerializeField]
    private GameObject player;

    public float attackRange = 20f;
    public float attackSpeed = 1f;

    private float timeSinceLastAttack = 0f;
    private float playerDistance = 0f;

    void Update()
    {
        timeSinceLastAttack += Time.deltaTime;
        playerDistance = Vector2.Distance(player.transform.position, transform.position);
        if ( playerDistance < attackRange && timeSinceLastAttack > attackSpeed)
            AttackPlayer();
    }

    private void AttackPlayer()
    {
        timeSinceLastAttack = 0f;
        GameObject projectile = Instantiate(projectilPrefab, transform.position, Quaternion.identity);
        projectile.GetComponent<ProjectilBehaviours>().SetTarget(player);
    }

}
