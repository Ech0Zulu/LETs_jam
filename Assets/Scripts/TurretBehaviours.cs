using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ProjectilSpawnBehaviours : MonoBehaviour
{
    [SerializeField]
    private GameObject projectilPrefab;
    [SerializeField]
    private LayerMask playerLayer;

    public float attackRange = 20f;
    public float attackSpeed = 1f;

    private float timeSinceLastAttack = 0f;
    private float playerDistance = 0f;

    void Update()
    {
        timeSinceLastAttack += Time.deltaTime;
        Collider2D[] player = Physics2D.OverlapCircleAll(transform.position, attackRange, playerLayer);

        if ( player.Length > 0 && timeSinceLastAttack > attackSpeed)
        {
            timeSinceLastAttack = 0f;
            GameObject projectile = Instantiate(projectilPrefab, transform.position, Quaternion.identity);
            projectile.GetComponent<ProjectilBehaviours>().SetTarget(player[0].gameObject);
        }
    }
}
