using UnityEngine;
using TMPro;

public class LegoScoreManager : MonoBehaviour
{
    public static LegoScoreManager Instance;

    [Header("Studs / Mynt")]
    public int totalStuds = 0;
    public AudioClip SoundCount; 
    public TextMeshProUGUI studText;

    [Header("Kistor / Chests")]
    public int currentChests = 0;
    public int maxChests = 10;
    public TextMeshProUGUI chestText; // Dra in din UI-text (Text (TMP)) under Chest här!

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateUI();
    }

    public void AddStuds(int amount)
    {
        totalStuds += amount;
        
        AudioSource audio = GameObject.FindObjectOfType<AudioSource>();
        if (audio != null && SoundCount != null)
        {
            audio.PlayOneShot(SoundCount);
        }

        UpdateUI();
    }

    public void AddChest(int amount = 1)
    {
        currentChests += amount;
        UpdateUI();

        // Starta pop-effekten på UI-ikonen
        if (ChestUIAnimation.Instance != null)
        {
            ChestUIAnimation.Instance.TriggerChestPop();
        }
    }

    private void UpdateUI()
    {
        if (studText != null)
        {
            studText.text = totalStuds.ToString("N0");
        }

        if (chestText != null)
        {
            // Uppdaterar texten till formatet "1/10"
            chestText.text = $"{currentChests}/{maxChests}";
        }
    }
}