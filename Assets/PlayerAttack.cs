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
    public LayerMask attackableLayers; // Välj både Enemy och Default/Destructible i Inspector!

    void Start()
    {
        anime = GetComponent<Animator>();
        characterController = GetComponent<LegoCharacterController>();
    }

    void Update()
    {
        // Slå när spelaren klickar
        if (isControlledByPlayer && Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (anime != null)
            {
                anime.SetTrigger("Attack");
            }
        }
    }

    // Körs av Animation Event ELLER av FollowCompanion
    public void Attack() 
    {
        Vector3 point = attackPoint != null ? attackPoint.position : transform.position + transform.forward * 1f;

        // Hämta alla collider-objekt inom räckhåll på valda lager
        Collider[] hitColliders = Physics.OverlapSphere(point, attackRange, attackableLayers);

        foreach (Collider hit in hitColliders)
        {
            // 1. Skada fiende
            EnemyAI enemyAI = hit.GetComponentInParent<EnemyAI>();
            if (enemyAI != null)
            {
                enemyAI.TakeDamage(attackDamage);
            }

            // 2. Skada förstörbart objekt (t.ex. låda/kruka)
            DestructibleObject destructible = hit.GetComponentInParent<DestructibleObject>();
            if (destructible != null)
            {
                destructible.TakeDamage(attackDamage);
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