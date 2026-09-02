using UnityEngine;

public class LegoCharacterController : MonoBehaviour
{
    public float moveSpeed = 6f;
    public float rotationSpeed = 15f;
    public float modelRotationOffset = 0f;

    [Header("Jumping & Gravity")]
    public float jumpHeight = 1.5f;       // Hur högt karaktären hoppar
    public float gravity = -9.81f * 2f;    // Tyngdkraften (gärna dubbel för snabbare LEGO-känsla)
    
    private float verticalVelocity;       // Håller koll på hoppet/fallet

    private Animator anime;
    private CharacterController controller;
    public Camera mainCamera;
    
    void Start()
    {
      
        controller = GetComponent<CharacterController>();
        anime = GetComponent<Animator>();
        
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    void Update()
    {
        // 1. Hantera tyngdkraft & markkontakt
        bool isGrounded = controller.isGrounded;
        
        if (isGrounded && verticalVelocity < 0)
        {
            // Sätt en liten negativ kraft så spelaren hålls stadigt mot marken
            verticalVelocity = -2f;
            
        }

        // 2. Hämta input för rörelse
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 inputDir = new Vector3(horizontal, 0f, vertical).normalized;

        Vector3 moveDirection = Vector3.zero;

        if (inputDir.magnitude >= 0.1f)
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
        }
        else 
        {
            if (anime != null) anime.SetBool("Run", false);
        }

        // 3. Hantera Hopp (Space-knappen)
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            // Formel för hopphastighet: v = sqrt(h * -2 * g)
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

            if (anime != null)
            {
                anime.SetTrigger("Jump"); // Valfritt: trigga Jump-animation om du har en
            }
        }
       

        // 4. Lägg till tyngdkraft över tid
        verticalVelocity += gravity * Time.deltaTime;

        // 5. Kombinera horisontell och vertikal rörelse i ETT Move-anrop
        Vector3 finalVelocity = (moveDirection * moveSpeed) + (Vector3.up * verticalVelocity);
        controller.Move(finalVelocity * Time.deltaTime);
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
}