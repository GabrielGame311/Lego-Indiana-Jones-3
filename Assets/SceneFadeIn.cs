using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class SceneFadeIn : MonoBehaviour
{
    [Header("Inställningar")]
    public float fadeInSpeed = 2f; // Hastighet på intoningen

    private List<Graphic> targetGraphics = new List<Graphic>();
    private List<float> originalAlphas = new List<float>();

    void Start()
    {
        // Hitta alla UI-komponenter (både Image och TMP_Text är arvtagare till Graphic)
        Graphic[] allGraphics = FindObjectsOfType<Graphic>();

        foreach (Graphic g in allGraphics)
        {
            if (g != null)
            {
                targetGraphics.Add(g);
                
                // Spara elementets ursprungliga alfa-värde
                float originalAlpha = g.color.a;
                originalAlphas.Add(originalAlpha);

                // Sätt alfa till 0 (helt genomskinlig) vid start
                Color c = g.color;
                c.a = 0f;
                g.color = c;
            }
        }

        // Starta intoningen om det finns element att tona
        if (targetGraphics.Count > 0)
        {
            StartCoroutine(DoFadeInGraphics());
        }
    }

    private IEnumerator DoFadeInGraphics()
    {
        float currentProgress = 0f;

        while (currentProgress < 1f)
        {
            currentProgress += Time.deltaTime * fadeInSpeed;

            // Tona upp varje element mot dess original-alfa
            for (int i = 0; i < targetGraphics.Count; i++)
            {
                if (targetGraphics[i] != null)
                {
                    Color c = targetGraphics[i].color;
                    c.a = Mathf.Lerp(0f, originalAlphas[i], currentProgress);
                    targetGraphics[i].color = c;
                }
            }

            yield return null;
        }

        // Säkerställ att alla element når exakt sitt slutgiltiga alfa-värde
        for (int i = 0; i < targetGraphics.Count; i++)
        {
            if (targetGraphics[i] != null)
            {
                Color c = targetGraphics[i].color;
                c.a = originalAlphas[i];
                targetGraphics[i].color = c;
            }
        }
    }
}