using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingMapManager : MonoBehaviour
{
    [System.Serializable]
    public struct LevelRoute
    {
        public string sceneName;        // Namnet på scenen som ska laddas
        public RectTransform startPoint;// Startpunkt på kartan
        public RectTransform endPoint;  // Målpunkt på kartan
    }

    [Header("UI Element")]
    public Slider loadingSlider;      // Din Slider
    public RectTransform planeIcon;   // Flygplansikonen

    [Header("Banalista")]
    public LevelRoute[] levelRoutes;
    public float loadSpeed = 0.5f;

    public static string selectedScene = "Level1"; 

    void Start()
    {
        if (loadingSlider != null)
        {
            loadingSlider.minValue = 0f;
            loadingSlider.maxValue = 1f;
            loadingSlider.value = 0f;
        }

        LevelRoute currentRoute = GetRouteForScene(selectedScene);
        StartCoroutine(LoadAndDrawLine(currentRoute));
    }

    private LevelRoute GetRouteForScene(string sceneName)
    {
        foreach (var route in levelRoutes)
        {
            if (route.sceneName == sceneName) return route;
        }
        return levelRoutes.Length > 0 ? levelRoutes[0] : default;
    }

    private IEnumerator LoadAndDrawLine(LevelRoute route)
    {
        if (route.startPoint == null || route.endPoint == null)
        {
            Debug.LogError("StartPoint eller EndPoint saknas på rutten!");
            yield break;
        }

        // Hämta världspositioner för exakt precision
        Vector3 startPos = route.startPoint.position;
        Vector3 endPos = route.endPoint.position;

        // 1. Placera och sträck ut Slidern
        if (loadingSlider != null)
        {
            RectTransform sliderRect = loadingSlider.GetComponent<RectTransform>();
            
            // Sätt position till startpunkten
            sliderRect.position = startPos;

            // Beräkna riktning och avstånd i världskoordinater
            Vector3 direction = endPos - startPos;
            float distance = Vector2.Distance(route.startPoint.anchoredPosition, route.endPoint.anchoredPosition);
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Sätt exakt längd och vinkel
            sliderRect.sizeDelta = new Vector2(distance, sliderRect.sizeDelta.y);
            sliderRect.rotation = Quaternion.Euler(0, 0, angle);
        }

        // 2. Starta asynkron laddning
        AsyncOperation operation = SceneManager.LoadSceneAsync(route.sceneName);
        operation.allowSceneActivation = false;

        float currentProgress = 0f;

        while (currentProgress < 1f)
        {
            float targetProgress = Mathf.Clamp01(operation.progress / 0.9f);
            currentProgress = Mathf.MoveTowards(currentProgress, targetProgress, Time.deltaTime * loadSpeed);

            // Uppdatera Slider
            if (loadingSlider != null)
            {
                loadingSlider.value = currentProgress;
            }

            // Flytta flygplanet längs vägen
            if (planeIcon != null)
            {
                planeIcon.position = Vector3.Lerp(startPos, endPos, currentProgress);

                Vector3 dir = endPos - startPos;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                planeIcon.rotation = Quaternion.Euler(0, 0, angle);
            }

            yield return null;
        }

        if (loadingSlider != null) loadingSlider.value = 1f;

        yield return new WaitForSeconds(0.3f);
        operation.allowSceneActivation = true;
    }
}