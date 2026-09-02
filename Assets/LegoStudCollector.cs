using UnityEngine;
using TMPro; // Använd UnityEngine.UI om du inte använder TextMeshPro

public class LegoStudCollector : MonoBehaviour
{
    public int totalStuds = 0;
    public TextMeshProUGUI studText; // Dra in din UI Text här

    void Start()
    {
        
        UpdateUI();
    }

    public void AddStuds(int amount)
    {
        totalStuds += amount;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (studText != null)
        {
            // Formaterar siffran så den ser snygg ut (t.ex. 1 000 eller 1,000)
            studText.text = totalStuds.ToString("N0");
        }
    }
}