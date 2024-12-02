using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ProjectilSpawnBehaviours : MonoBehaviour
{
    [SerializeField]
    private GameObject projectilPrefab;

    public float attackRange = 20f;
    public float attackSpeed = 1f;

    private float timeSinceLastAttack = 0f;
    private float playerDistance = 0f;

    void Update()
    {
        timeSinceLastAttack += Time.deltaTime;
        Collider2D[] detected = Physics2D.OverlapCircleAll(transform.position, attackRange, LayerMask.GetMask("Default"));

        if (detected.Length > 0 && timeSinceLastAttack > attackSpeed)
        {
            foreach (Collider2D enemy in detected)
            {
                if (detected.Length > 0 && timeSinceLastAttack > attackSpeed)
                {
                    if (enemy.CompareTag("Player"))
                    {
                        Debug.Log("Found an enemy");
                        timeSinceLastAttack = 0f;
                        GameObject projectile = Instantiate(projectilPrefab, transform.position, Quaternion.identity);
                        projectile.GetComponent<ProjectilBehaviours>().SetTarget(enemy.gameObject);
                    }
                }
            }
        }
    }
}
