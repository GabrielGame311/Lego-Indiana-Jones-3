using UnityEngine;
using System.Collections;

public class GameObjectBlinker : MonoBehaviour
{
    [Header("Objektet som ska blinka")]
    public GameObject targetObject;

    private CanvasGroup canvasGroup;
    private Renderer objRenderer;

    void Awake()
    {
        if (targetObject != null)
        {
            canvasGroup = targetObject.GetComponent<CanvasGroup>();
            objRenderer = targetObject.GetComponent<Renderer>();
        }
    }

    void Update() 
    {

        targetObject.transform.LookAt(Camera.main.transform);

    }

    // Blinkar fram indikatorn på den nya spelaren
    public void BlinkAndShow()
    {
        StopAllCoroutines();
        StartCoroutine(BlinkInRoutine());
    }

    // Släcker indikatorn direkt på spelaren du lämnar (ingen blinkning)
    public void HideDirectly()
    {
        StopAllCoroutines();
        SetVisibility(false);
        if (targetObject != null) targetObject.SetActive(false);
    }

    private IEnumerator BlinkInRoutine()
    {
        if (targetObject == null) yield break;

        targetObject.SetActive(true);

        // Blinka 3 gånger när den nya spelaren tar över
        for (int i = 0; i < 5; i++)
        {
            SetVisibility(false);
            yield return new WaitForSeconds(0.3f);
            SetVisibility(true);
            yield return new WaitForSeconds(0.3f);
        }


        // Stanna kvar påslagen
        HideDirectly();
    }

    private void SetVisibility(bool visible)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
        }
        else if (objRenderer != null)
        {
            objRenderer.enabled = visible;
        }
        else if (targetObject != null)
        {
            targetObject.SetActive(visible);
        }
    }
}