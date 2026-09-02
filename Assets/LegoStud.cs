using UnityEngine;

public class LegoStud : MonoBehaviour
{
    public enum StudType { Silver, Gold, Blue, Purple }
    public StudType studType = StudType.Gold;

    [Header("Inställningar")]
    public int scoreValue = 100;
    public float rotateSpeed = 100f;
    public AudioClip collectSound;

    private bool isCollected = false;

    void Start()
    {
        switch (studType)
        {
            case StudType.Silver: scoreValue = 10; break;
            case StudType.Gold:   scoreValue = 100; break;
            case StudType.Blue:   scoreValue = 1000; break;
            case StudType.Purple: scoreValue = 10000; break;
        }
    }

    void Update()
    {
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Förhindra att koden körs två gånger i samma bildruta
        if (isCollected) return;

        if (other.CompareTag("Player"))
        {
            PlayerHealth player = other.GetComponent<PlayerHealth>();

            // Kontrollera att det är den spelarkontrollerade karaktären
            if (player != null && player.isControlledByPlayer)
            {
                isCollected = true;

                // Lägg till poäng
                if (LegoScoreManager.Instance != null)
                {
                    LegoScoreManager.Instance.AddStuds(scoreValue);
                }

                // Spela ljud på myntets position i världen (avbryts inte när myntet tas bort)
                if (collectSound != null)
                {
                    AudioSource.PlayClipAtPoint(collectSound, transform.position);
                }

                // Ta bort myntet
                Destroy(gameObject);
            }
        }
    }
}