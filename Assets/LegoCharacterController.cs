using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(CharacterController))]
public class LegoCharacterController : MonoBehaviour
{
    public float moveSpeed = 6f;
    public float rotationSpeed = 15f;
    public float modelRotationOffset = 0f;

    [Header("Jumping & Gravity")]
    public float jumpHeight = 1.5f;       // Hur högt karaktären hoppar
    public float gravity = -19.62f;       // Tyngdkraften (standard -9.81 * 2)

    [Header("Terrain Markkontroll (Fix för Maximized)")]
    public float groundDistance = 0.4f;   // Radie vid fötterna
    public LayerMask groundMask;          // Välj Default / Terrain i Inspector
    private bool isGrounded;
    
    private float verticalVelocity;       // Håller koll på hoppet/fallet

    [Header("Fotstegsljud")]
    public AudioClip[] footstepSounds;    // Lista med fotstegsljud
    public float stepInterval = 0.3f;     // Tid mellan varje steg när man springer
    private float stepTimer;

    private Animator anime;
    private CharacterController controller;
    private AudioSource audioSource;
    public Camera mainCamera;
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
        anime = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        
        // Ställ in AudioSource för 3D-ljud
        if (audioSource != null)
        {
            audioSource.spatialBlend = 1.0f;
            audioSource.playOnAwake = false;
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    void Update()
    {
        // 1. Stabil markkontroll med sfär vid fötterna (fixar Maximized/Terrain)
        Vector3 spherePos = transform.position + Vector3.down * (controller.height / 2f - controller.radius / 2f);

        if (groundMask == 0)
        {
            isGrounded = controller.isGrounded || Physics.CheckSphere(spherePos, groundDistance);
        }
        else
        {
            isGrounded = controller.isGrounded || Physics.CheckSphere(spherePos, groundDistance, groundMask);
        }

        // 2. Hantera tyngdkraft vid markkontakt
        if (isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -5f; // Håll kvar spelaren stadigt mot Terrain
        }
        else
        {
            // Applicera tyngdkraft BARA EN GÅNG per ram när vi är i luften
            verticalVelocity += gravity * Time.deltaTime;
        }

        // 3. Hämta input för rörelse
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 inputDir = new Vector3(horizontal, 0f, vertical).normalized;

        Vector3 moveDirection = Vector3.zero;
        bool isMoving = inputDir.sqrMagnitude > 0.01f;

        if (isMoving)
        {
            // Beräkna framåt/höger relativt kameran
            Vector3 cameraForward = mainCamera.transform.forward;
            Vector3 cameraRight = mainCamera.transform.right;

            cameraForward.y = 0f;
            cameraRight.y = 0f;
            cameraForward.Normalize();
            cameraRight.Normalize();

            moveDirection = cameraForward * inputDir.z + cameraRight * inputDir.x;

            // Rota karaktären mot rörelseriktningen
            if (moveDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                targetRotation *= Quaternion.Euler(0, modelRotationOffset, 0);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            if (anime != null) anime.SetBool("Run", true);

            // Spela fotstegsljud om spelaren rör sig på marken
            if (isGrounded)
            {
                HandleFootsteps();
            }
        }
        else 
        {
            if (anime != null) anime.SetBool("Run", false);
            stepTimer = stepInterval; 
        }

        // 4. Hantera Hopp (Space-knappen)
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            // Formel för hopphastighet: v = sqrt(h * -2 * g)
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

            if (anime != null)
            {
                anime.SetTrigger("Jump");
            }
        }

        // 5. Kombinera horisontell och vertikal rörelse i ETT Move-anrop
        Vector3 finalVelocity = (moveDirection * moveSpeed) + (Vector3.up * verticalVelocity);
        controller.Move(finalVelocity * Time.deltaTime);
    }

    void HandleFootsteps()
    {
        stepTimer += Time.deltaTime;

        if (stepTimer >= stepInterval)
        {
            if (footstepSounds != null && footstepSounds.Length > 0)
            {
                int randomIndex = Random.Range(0, footstepSounds.Length);
                AudioClip clip = footstepSounds[randomIndex];

                if (clip != null)
                {
                    audioSource.pitch = Random.Range(0.9f, 1.1f);
                    audioSource.PlayOneShot(clip);
                }
            }

            stepTimer = 0f;
        }
    }

    public bool HasEnemyInRange(Vector3 attackPosition, float attackRange, LayerMask enemyLayers)
    {
        Collider[] nearbyColliders = Physics.OverlapSphere(attackPosition, attackRange, enemyLayers);

        foreach (Collider nearbyCollider in nearbyColliders)
        {
            if (nearbyCollider.GetComponentInParent<EnemyAI>() != null)
            {
                return true;
            }
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        if (controller != null)
        {
            Gizmos.color = Color.green;
            Vector3 spherePos = transform.position + Vector3.down * (controller.height / 2f - controller.radius / 2f);
            Gizmos.DrawWireSphere(spherePos, groundDistance);
        }
    }
}