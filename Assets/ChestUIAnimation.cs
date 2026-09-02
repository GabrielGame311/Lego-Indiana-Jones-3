using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ChestUIAnimation : MonoBehaviour
{
    public static ChestUIAnimation Instance;

    [Header("UI Komponenter")]
    public RawImage rawImage;
    public CanvasGroup canvasGroup; // Används för att styra genomskinlighet (fade)

    [Header("Animationsinställningar")]
    public float popScale = 1.4f;   // Hur mycket ikonen växer vid upplockning
    public float animSpeed = 5f;    // Hastighet på pop-effekten
    public bool hideWhenZero = true; // Om den ska vara osynlig innan du tar första kistan
    public AudioClip popSound; // Ljud som spelas när kistan plockas upp
    private Vector3 originalScale;

    private void Awake()
    {
        if (Instance == null) Instance = this;

        if (rawImage == null) rawImage = GetComponent<RawImage>();
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        if (rawImage != null)
        {
            originalScale = rawImage.transform.localScale;
        }

        // Om den ska vara helt osynlig från början
        if (hideWhenZero && canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
    }

    // Anropa denna metod när en kista plockas upp!
    public void TriggerChestPop()
    {
        StopAllCoroutines();
        StartCoroutine(AnimateChestUI());
    }

    private IEnumerator AnimateChestUI()
    {
        // 1. Tona fram ikonen om den var osynlig
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
        GameObject.FindObjectOfType<AudioSource>().PlayOneShot(popSound);
        // 2. Skala upp (Pop out)
        Vector3 targetScale = originalScale * popScale;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * animSpeed;
            rawImage.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }

        // 3. Skala tillbaka till normal storlek
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * animSpeed;
            rawImage.transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }

        rawImage.transform.localScale = originalScale;

        // 4. (Valfritt) Tona bort ikonen igen efter 3 sekunder
        
        yield return new WaitForSeconds(3f);
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 2f;
            if (canvasGroup != null) canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }
        
    }
}