using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private Animator anime;
    private LegoCharacterController characterController;

    [Header("Status")]
    public bool isControlledByPlayer = true; // Sätts av CharacterSwitcher

    [Header("Attack Inställningar")]
    public Transform attackPoint;      
    public float attackRange = 1.2f;    
    public int attackDamage = 1;       
    public LayerMask enemyLayers;      

    void Start()
    {
        anime = GetComponent<Animator>();
        characterController = GetComponent<LegoCharacterController>();
    }

    void Update()
    {
        // Slå manuellt endast om denna karaktär styrs av spelaren
        if (isControlledByPlayer && Input.GetKeyDown(KeyCode.Mouse0) && HasEnemyInRange())
        {
            if (anime != null)
            {
                anime.SetTrigger("Attack");
            }
        }
    }

    private bool HasEnemyInRange()
    {
        Vector3 point = attackPoint != null ? attackPoint.position : transform.position + transform.forward * 1f;

        if (characterController != null)
        {
            return characterController.HasEnemyInRange(point, attackRange, enemyLayers);
        }

        return Physics.OverlapSphere(point, attackRange, enemyLayers).Length > 0;
    }

    public void Attack() 
    {
        // Om detta är följaren (inte spelaren), gör ingen skada på fienden!
        if (!isControlledByPlayer) return;

        Vector3 point = attackPoint != null ? attackPoint.position : transform.position + transform.forward * 1f;

        Collider[] hitEnemies = Physics.OverlapSphere(point, attackRange, enemyLayers);

        foreach (Collider enemy in hitEnemies)
        {
            EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                enemyAI.TakeDamage(attackDamage);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 point = attackPoint != null ? attackPoint.position : transform.position + transform.forward * 1f;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(point, attackRange);
    }
}