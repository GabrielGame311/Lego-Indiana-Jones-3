using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Egenskaper")]
    public int maxHealth = 3;
    public int currentHealth;

    public float moveSpeed = 3.5f;
    public float detectionRange = 15f;
    public float attackRange = 10f;

    [Header("Skjutning")]
    public GameObject enemyBulletPrefab;
    public Transform firePoint;
    public float fireRate = 1.5f;
    private float nextFireTime = 0f;

    [Header("LEGO Death Effect")]
    public GameObject enemyShatterPrefab;
    public float explosionForce = 4f;
    public float bulletSpeed = 20f;
    
    private Transform playerTarget;
    private Animator anime;

    void Start()
    {
        currentHealth = maxHealth;
        anime = GetComponent<Animator>();
        FindActivePlayer();
    }

    void Update()
    {
        // Uppdatera målet om spelaren saknas eller har bytt tagg
        if (playerTarget == null || !playerTarget.gameObject.CompareTag("Player"))
        {
            FindActivePlayer();
        }

        if (playerTarget == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        if (distanceToPlayer <= detectionRange)
        {
            Vector3 targetPosition = new Vector3(playerTarget.position.x, transform.position.y, playerTarget.position.z);
            transform.LookAt(targetPosition);
            
            if (anime != null) anime.SetBool("IdleGun", true);

            if (distanceToPlayer > attackRange)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                if (anime != null) anime.SetBool("Run", true);
            }
            else
            {
                if (anime != null) anime.SetBool("Run", false);

                if (Time.time >= nextFireTime)
                {
                    ShootAtPlayer();
                    nextFireTime = Time.time + fireRate;
                }
            }
        }
        else
        {
            if (anime != null)
            {
                anime.SetBool("Run", false);
                anime.SetBool("IdleGun", false);
            }
        }
    }

    // Hittar karaktären som har taggen "Player" just nu
    void FindActivePlayer()
    {
        GameObject activePlayer = GameObject.FindGameObjectWithTag("Player");
        if (activePlayer != null)
        {
            playerTarget = activePlayer.transform;
        }
    }

    void ShootAtPlayer()
    {
        if (enemyBulletPrefab == null || firePoint == null || playerTarget == null) return;

        // Rikta firePoint mot den aktiva spelaren
        Vector3 direction = (playerTarget.position + Vector3.up * 1f) - firePoint.position;
        Quaternion bulletRotation = Quaternion.LookRotation(direction);

        GameObject bullet = Instantiate(enemyBulletPrefab, firePoint.position, bulletRotation);

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = direction.normalized * bulletSpeed;
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (enemyShatterPrefab != null)
        {
            GameObject shatteredParts = Instantiate(enemyShatterPrefab, transform.position, transform.rotation);

            foreach (Rigidbody rb in shatteredParts.GetComponentsInChildren<Rigidbody>())
            {
                rb.AddExplosionForce(explosionForce, transform.position + Vector3.up * 0.5f, 2f, 1f, ForceMode.Impulse);
                Destroy(rb.gameObject, 3f);
            }
        }

        Destroy(gameObject);
    }
}