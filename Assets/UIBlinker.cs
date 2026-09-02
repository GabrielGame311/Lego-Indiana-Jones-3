using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIBlinker : MonoBehaviour
{
    private CanvasGroup canvasGroup;

    void Awake()
    {
        // Se till att det finns en CanvasGroup för att kontrollera genomskinlighet (Alpha)
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    // Anropas när man byter till denna gubbe (Visar ikonen)
    public void ShowUI()
    {
        StopAllCoroutines();
        gameObject.SetActive(true);
        canvasGroup.alpha = 1f;
    }

    // Anropas när man byter från denna gubbe (Blinkar och försvinner)
    public void BlinkAndHide()
    {
        StopAllCoroutines();
        StartCoroutine(BlinkRoutine());
    }

    private IEnumerator BlinkRoutine()
    {
        gameObject.SetActive(true);

        // 1. Blink-effekt (snabb tänd/släck 3 gånger)
        for (int i = 0; i < 3; i++)
        {
            canvasGroup.alpha = 0.2f;
            yield return new WaitForSeconds(0.06f);
            canvasGroup.alpha = 1f;
            yield return new WaitForSeconds(0.06f);
        }

        // 2. Tona ut (Fade Out)
        float fadeTime = 0.2f;
        float elapsed = 0f;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeTime);
            yield return null;
        }

        // 3. Stäng av objektet helt när det försvunnit
        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }
}