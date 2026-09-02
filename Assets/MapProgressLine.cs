using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class MapProgressLine : MonoBehaviour
{
    [Header("UI Element")]
    public Image progressLine;        // Dra in din RedLine (Image) här!
    public RectTransform planeIcon;   // Dra in din flygplansikon här!

    [Header("Linjens punkter")]
    public RectTransform startPoint;  // Startposition (t.ex. London)
    public RectTransform endPoint;    // Målposition (t.ex. Kairo)

    [Header("Inställningar")]
    public string sceneToLoad = "Level1";
    public float loadSpeed = 0.5f;    // Hastighet på utritningen

    void Start()
    {
        StartCoroutine(LoadAndDrawLine());
    }

    private IEnumerator LoadAndDrawLine()
    {
        // Starta laddningen i bakgrunden utan att öppna banan direkt
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad);
        operation.allowSceneActivation = false;

        float currentProgress = 0f;

        while (currentProgress < 1f)
        {
            // Beräkna hur långt laddningen har kommit (0 till 1)
            float targetProgress = Mathf.Clamp01(operation.progress / 0.9f);
            
            // Få linjen att ritas ut mjukt och jämnt
            currentProgress = Mathf.MoveTowards(currentProgress, targetProgress, Time.deltaTime * loadSpeed);

            // 1. Öka linjens längd (fyller Image från 0% till 100%)
            if (progressLine != null)
            {
                progressLine.fillAmount = currentProgress;
            }

            // 2. Flytta flygplanet längst fram på linjen
            if (planeIcon != null && startPoint != null && endPoint != null)
            {
                planeIcon.anchoredPosition = Vector2.Lerp(startPoint.anchoredPosition, endPoint.anchoredPosition, currentProgress);
            }

            yield return null;
        }

        // När linjen är helt utritad och banan laddad -> öppna spelet
        yield return new WaitForSeconds(0.3f);
        operation.allowSceneActivation = true;
    }
}