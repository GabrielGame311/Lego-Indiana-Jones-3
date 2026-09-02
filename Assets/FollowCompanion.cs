using UnityEngine;

public class FollowCompanion : MonoBehaviour
{
    public float followSpeed = 4f;
    public float stoppingDistance = 2.5f;
    
    [Header("Attack Inställningar")]
    public float attackRange = 2f;
    public float attackRate = 1.5f;
    private float nextAttackTime = 0f;

    private Transform target;
    private Animator anime;
    private PlayerAttack attackScript;

    void Awake()
    {
        anime = GetComponent<Animator>();
        attackScript = GetComponent<PlayerAttack>();
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        enabled = true;
    }

    void Update()
    {
        if (target == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, target.position);

        // 1. Leta efter fiender nära följaren
        Collider[] enemies = Physics.OverlapSphere(transform.position, attackRange, LayerMask.GetMask("Enemy"));

        if (enemies.Length > 0)
        {
            // Titta mot närmsta fiende och attackera
            Transform enemyTarget = enemies[0].transform;
            Vector3 lookPos = new Vector3(enemyTarget.position.x, transform.position.y, enemyTarget.position.z);
            transform.LookAt(lookPos);

            if (Time.time >= nextAttackTime)
            {
                if (anime != null) anime.SetTrigger("Attack");
                if (attackScript != null) attackScript.Attack();
                nextAttackTime = Time.time + attackRate;
            }
            return;
        }

        // 2. Följ efter spelaren om inga fiender är nära
        if (distanceToPlayer > stoppingDistance)
        {
            Vector3 targetPosition = new Vector3(target.position.x, transform.position.y, target.position.z);
            transform.LookAt(targetPosition);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, followSpeed * Time.deltaTime);

            if (anime != null) anime.SetBool("Run", true);
        }
        else
        {
            if (anime != null) anime.SetBool("Run", false);
        }
    }
}