using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    public Image[] hearts; // Dra in dina Heart-bilder här i Inspector
    public Sprite fullHeart;
    public Sprite emptyHeart; // Valfritt: om du vill visa tomma konturer

    public void UpdateHearts(int currentHealth)
    {
        // 1. Uppdatera synlighet/sprites för alla hjärtan och stäng av animationer
        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] == null) continue;

            // Återställ animatorn så att inte gamla hjärtan står kvar i animerat läge
            Animator anim = hearts[i].GetComponent<Animator>();
            if (anim != null)
            {
                anim.SetBool("Health", false);
            }

            if (i < currentHealth)
            {
                hearts[i].enabled = true;
                if (fullHeart != null) hearts[i].sprite = fullHeart;
            }
            else
            {
                if (emptyHeart != null)
                {
                    hearts[i].sprite = emptyHeart;
                }
                else
                {
                    hearts[i].enabled = false;
                }
            }
        }

        // 2. Aktivera animationen ENDAST på det sista aktiva hjärtat (currentHealth - 1)
        int lastHeartIndex = currentHealth - 1;

        if (lastHeartIndex >= 0 && lastHeartIndex < hearts.Length)
        {
            Animator lastAnim = hearts[lastHeartIndex].GetComponent<Animator>();
            if (lastAnim != null)
            {
                lastAnim.SetBool("Health", true);
            }
        }
    }
}