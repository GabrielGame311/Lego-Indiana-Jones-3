using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 4;
    public int currentHealth;
    public HealthUI healthUI;

    public float invincibilityTime = 1.5f;
    private bool isInvincible = false;

    [Header("LEGO Death Effect")]
    public GameObject playerShatterPrefab;
    public float respawnDelay = 2.5f;
    public float explosionForce = 5f;

    [Header("Hit Flash Settings")]
    public Color damageColor = Color.red;
    public float flashDuration = 0.2f;

    private Renderer[] allRenderers;
    private Color[] originalColors;
    private CharacterController controller;
    private LegoCharacterController movementScript;
    public bool isControlledByPlayer = true;      
    public static PlayerHealth Instance { get; private set; } // Singleton för enkel åtkomst  
    void Start()
    {
        currentHealth = maxHealth;
        allRenderers = GetComponentsInChildren<Renderer>();
        controller = GetComponent<CharacterController>();
        movementScript = GetComponent<LegoCharacterController>();
        healthUI = GameObject.FindObjectOfType<HealthUI>();
        Instance = this;
        // Spara alla originalfärger så vi kan återställa efter rödblinket
        originalColors = new Color[allRenderers.Length];
        for (int i = 0; i < allRenderers.Length; i++)
        {
            if (allRenderers[i].material.HasProperty("_Color"))
            {
                originalColors[i] = allRenderers[i].material.color;
            }
        }

        if (healthUI != null)
        {
            healthUI.UpdateHearts(currentHealth);
        }
    }

   public void TakeDamage(int damageAmount)
    {
        if (!isControlledByPlayer || isInvincible || currentHealth <= 0) return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Uppdatera UI endast om den aktiva spelaren tar skada
        if (healthUI != null)
        {
            healthUI.UpdateHearts(currentHealth);
        }

        if (currentHealth <= 0)
        {
            StartCoroutine(DieAndRespawn());
        }
        else
        {
            StartCoroutine(DamageFlash());
        }
    }

    // 1. Blinka röd vid skada
    private IEnumerator DamageFlash()
    {
        // Ändra färg till röd
        for (int i = 0; i < allRenderers.Length; i++)
        {
            if (allRenderers[i] != null && allRenderers[i].material.HasProperty("_Color"))
            {
                allRenderers[i].material.color = damageColor;
            }
        }

        yield return new WaitForSeconds(flashDuration);

        // Återställ till originalfärg
        for (int i = 0; i < allRenderers.Length; i++)
        {
            if (allRenderers[i] != null && allRenderers[i].material.HasProperty("_Color"))
            {
                allRenderers[i].material.color = originalColors[i];
            }
        }
    }

    // 2. Blinka (synlig/osynlig) under odödligheten efter respawn eller skada
    private IEnumerator InvincibilityFrames()
    {
        isInvincible = true;

        for (float i = 0; i < invincibilityTime; i += 0.15f)
        {
            foreach (var r in allRenderers)
            {
                if (r != null) r.enabled = !r.enabled;
            }
            yield return new WaitForSeconds(0.15f);
        }

        foreach (var r in allRenderers)
        {
            if (r != null) r.enabled = true;
        }

        isInvincible = false;
    }

    private IEnumerator DieAndRespawn()
    {
        isInvincible = true;

        // Dölj spelaren och stäng av styrning
        SetPlayerVisible(false);

        // Skapa LEGO-bitarna
        if (playerShatterPrefab != null)
        {
            GameObject shatteredParts = Instantiate(playerShatterPrefab, transform.position, transform.rotation);
            
            foreach (Rigidbody rb in shatteredParts.GetComponentsInChildren<Rigidbody>())
            {
                rb.AddExplosionForce(explosionForce, transform.position + Vector3.up * 0.5f, 2f, 1f, ForceMode.Impulse);
            }

            Destroy(shatteredParts, respawnDelay);
        }

        yield return new WaitForSeconds(respawnDelay);

        // Återställ liv
        currentHealth = maxHealth;
        if (healthUI != null) healthUI.UpdateHearts(currentHealth);

        // Visa spelaren igen och starta odödlighetsblinket
        SetPlayerVisible(true);
        StartCoroutine(InvincibilityFrames());
    }

    private void SetPlayerVisible(bool visible)
    {
        foreach (var r in allRenderers)
        {
            if (r != null) r.enabled = visible;
        }
        if (controller != null) controller.enabled = visible;
        if (movementScript != null) movementScript.enabled = visible;
    }
}