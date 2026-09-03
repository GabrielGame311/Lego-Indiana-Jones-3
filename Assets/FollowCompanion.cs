using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FollowCompanion : MonoBehaviour
{
    public float followSpeed = 4f;
    public float stoppingDistance = 2.5f;
    
    [Header("Tyngdkraft & Markkontakt")]
    public float gravity = -19.62f;       // Dubbel tyngdkraft för stabil LEGO-känsla
    private float verticalVelocity;

    [Header("Attack Inställningar")]
    public float attackRange = 2f;
    public float attackRate = 1.5f;
    private float nextAttackTime = 0f;

    private Transform target;
    private Animator anime;
    private PlayerAttack attackScript;
    private CharacterController controller;

    void Awake()
    {
        anime = GetComponent<Animator>();
        attackScript = GetComponent<PlayerAttack>();
        controller = GetComponent<CharacterController>();
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        enabled = true;
    }

    void Update()
    {
        // 1. Hantera tyngdkraft och markkontakt
        if (controller.isGrounded && verticalVelocity < 0)
        {
            // Tryck ner lätt mot marken/Terrain så den inte tappar greppet
            verticalVelocity = -5f; 
        }
        else
        {
            // Applicera tyngdkraft när den faller eller är i luften
            verticalVelocity += gravity * Time.deltaTime;
        }

        if (target == null) 
        {
            // Även om spelaren saknas appliceras tyngdkraften
            controller.Move(new Vector3(0, verticalVelocity, 0) * Time.deltaTime);
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, target.position);

        // 2. Leta efter fiender nära följaren
        Collider[] enemies = Physics.OverlapSphere(transform.position, attackRange, LayerMask.GetMask("Enemy"));

        if (enemies.Length > 0)
        {
            // Titta mot närmsta fiende och attackera
            Transform enemyTarget = enemies[0].transform;
            Vector3 lookPos = new Vector3(enemyTarget.position.x, transform.position.y, enemyTarget.position.z);
            transform.LookAt(lookPos);

            if (anime != null) anime.SetBool("Run", false);

            if (Time.time >= nextAttackTime)
            {
                if (anime != null) anime.SetTrigger("Attack");
                if (attackScript != null) attackScript.Attack();
                nextAttackTime = Time.time + attackRate;
            }

            // Tillämpa tyngdkraft även under attack
            controller.Move(new Vector3(0, verticalVelocity, 0) * Time.deltaTime);
            return;
        }

        // 3. Följ efter spelaren om inga fiender är nära
        Vector3 moveDirection = Vector3.zero;

        if (distanceToPlayer > stoppingDistance)
        {
            Vector3 targetPosition = new Vector3(target.position.x, transform.position.y, target.position.z);
            transform.LookAt(targetPosition);

            // Beräkna riktning horisontellt
            moveDirection = (targetPosition - transform.position).normalized;

            if (anime != null) anime.SetBool("Run", true);
        }
        else
        {
            if (anime != null) anime.SetBool("Run", false);
        }

        // 4. Kombinera horisontell rörelse och tyngdkraft i ETT Move-anrop
        Vector3 finalVelocity = (moveDirection * followSpeed) + (Vector3.up * verticalVelocity);
        controller.Move(finalVelocity * Time.deltaTime);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}