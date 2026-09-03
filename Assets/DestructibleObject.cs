using UnityEngine;

public class DestructibleObject : MonoBehaviour
{
    [Header("Hälsa")]
    public int maxHealth = 3;
    private int currentHealth;

    [Header("Trasiga bitar (Fractured Prefab)")]
    [Tooltip("Prefab med de sönderslagna bitarna (modellen uppdelad i mindre bitar)")]
    public GameObject brokenVersionPrefab;

    [Tooltip("Kraften som kastar ut de trasiga rest-bitarna")]
    public float explosionForce = 5f;

    [Tooltip("Hur många sekunder de trasiga bitarna ligger kvar innan de tas bort")]
    public float brokenPiecesLifetime = 5f;

    [Header("Pynt / Loot-inställningar")]
    [Tooltip("Lista över pynt/prefabs som kan spawnas")]
    public GameObject[] lootPrefabs;
    public int minLootAmount = 1;
    public int maxLootAmount = 3;
    public float lootDropForce = 4f;

    [Header("Effekter & Ljud")]
    public GameObject destroyEffectPrefab; 
    public AudioClip hitSound;             
    public AudioClip destroySound;         

    private AudioSource audioSource;

    void Start()
    {
        currentHealth = maxHealth;
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (hitSound != null || destroySound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1.0f;
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (hitSound != null && audioSource != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(hitSound);
        }

        if (currentHealth <= 0)
        {
            DestroyObject();
        }
    }

    private void DestroyObject()
    {
        // Spara exakt position och rotation för det hela objektet
        Vector3 spawnPosition = transform.position;
        Quaternion spawnRotation = transform.rotation;

        // 1. Spawna den trasiga versionen (rest-bitarna)
        if (brokenVersionPrefab != null)
        {
            GameObject brokenInstance = Instantiate(brokenVersionPrefab, spawnPosition, spawnRotation);

            // Ge en mindre explosionskraft på alla Rigidbody-komponenter i rest-bitarna
            Rigidbody[] pieces = brokenInstance.GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody rb in pieces)
            {
                // Slunga ut bitarna från mitten av objektet
                Vector3 randomDirection = (rb.transform.position - spawnPosition).normalized + Vector3.up * 0.5f;
                rb.AddForce(randomDirection * explosionForce, ForceMode.Impulse);
            }

            // Städa bort de trasiga bitarna efter X sekunder så att de inte tynger ner spelet
            Destroy(brokenInstance, brokenPiecesLifetime);
        }

        // 2. Spawna partikeleffekt
        if (destroyEffectPrefab != null)
        {
            Instantiate(destroyEffectPrefab, spawnPosition, Quaternion.identity);
        }

        // 3. Spela förstörelseljud i världen
        if (destroySound != null)
        {
            AudioSource.PlayClipAtPoint(destroySound, spawnPosition);
        }

        // 4. Spawna pyntet
        SpawnLoot(spawnPosition);

        // 5. Ta bort det hela originalobjektet
        Destroy(gameObject);
    }

    private void SpawnLoot(Vector3 position)
    {
        if (lootPrefabs == null || lootPrefabs.Length == 0) return;

        int amountToSpawn = Random.Range(minLootAmount, maxLootAmount + 1);

        for (int i = 0; i < amountToSpawn; i++)
        {
            int randomIndex = Random.Range(0, lootPrefabs.Length);
            GameObject selectedLoot = lootPrefabs[randomIndex];

            if (selectedLoot != null)
            {
                Vector3 spawnPos = position + Vector3.up * 0.5f;
                GameObject spawnedLoot = Instantiate(selectedLoot, spawnPos, Quaternion.identity);

                Rigidbody rb = spawnedLoot.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 randomForce = new Vector3(
                        Random.Range(-1f, 1f),
                        Random.Range(0.8f, 1.5f),
                        Random.Range(-1f, 1f)
                    ).normalized * lootDropForce;

                    rb.AddForce(randomForce, ForceMode.Impulse);
                }
            }
        }
    }
}