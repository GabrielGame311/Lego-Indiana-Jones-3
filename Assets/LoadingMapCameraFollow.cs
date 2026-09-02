using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class LoadingMapCameraFollow : MonoBehaviour
{
    [System.Serializable]
    public struct LevelRoute
    {
#if UNITY_EDITOR
        [Header("Dra in scenfilen här:")]
        public SceneAsset sceneAsset;
#endif
        [HideInInspector]
        public string sceneName;

        public RectTransform startPoint;
        public RectTransform endPoint;

        [Header("UI Text för denna bana")]
        public TMP_Text routeNameText;
    }

    [Header("UI Element")]
    public Slider loadingSlider;
    public RectTransform planeIcon;
    public RectTransform mapTransform;

    [Header("Kamera & Zoom-inställningar")]
    public float zoomLevel = 1.8f;
    public float smoothSpeed = 4f;
    public float fadeSpeed = 2f;

    [Header("Banalista")]
    public LevelRoute[] levelRoutes;
    public float loadSpeed = 0.5f;

    [Header("Bana som ska laddas")]
    public static int levelLoading = 0; // Ange index för vilken rutt i listan som ska laddas

    private Vector3 defaultMapScale;
    private Vector2 defaultMapPosition;

    private void OnValidate()
    {
#if UNITY_EDITOR
        if (levelRoutes != null)
        {
            for (int i = 0; i < levelRoutes.Length; i++)
            {
                if (levelRoutes[i].sceneAsset != null)
                {
                    levelRoutes[i].sceneName = levelRoutes[i].sceneAsset.name;
                }
            }
        }
#endif
    }

    void Start()
    {
        if (loadingSlider != null)
        {
            loadingSlider.minValue = 0f;
            loadingSlider.maxValue = 1f;
            loadingSlider.value = 0f;
        }

        if (mapTransform != null)
        {
            defaultMapScale = mapTransform.localScale;
            defaultMapPosition = mapTransform.anchoredPosition;
        }

        // Hämta rutten från listan baserat på levelLoading-index
        LevelRoute currentRoute = GetRouteByIndex(levelLoading);

        // Aktivera endast texten för den valda rutten i listan
        for (int i = 0; i < levelRoutes.Length; i++)
        {
            if (levelRoutes[i].routeNameText != null)
            {
                bool isCurrent = (i == levelLoading);
                levelRoutes[i].routeNameText.gameObject.SetActive(isCurrent);

                if (isCurrent)
                {
                    levelRoutes[i].routeNameText.text = levelRoutes[i].sceneName;
                }
            }
        }

        StartCoroutine(LoadAndDrawLine(currentRoute));
    }

    private LevelRoute GetRouteByIndex(int index)
    {
        if (levelRoutes != null && index >= 0 && index < levelRoutes.Length)
        {
            return levelRoutes[index];
        }

        Debug.LogWarning($"Index {index} är utanför listans gränser! Laddar rutt 0.");
        return levelRoutes.Length > 0 ? levelRoutes[0] : default;
    }

    private IEnumerator LoadAndDrawLine(LevelRoute route)
    {
        if (route.startPoint == null || route.endPoint == null)
        {
            Debug.LogError("StartPoint eller EndPoint saknas!");
            yield break;
        }

        Vector2 startPosUI = route.startPoint.anchoredPosition;
        Vector2 endPosUI = route.endPoint.anchoredPosition;

        if (loadingSlider != null)
        {
            RectTransform sliderRect = loadingSlider.GetComponent<RectTransform>();
            sliderRect.anchoredPosition = startPosUI;

            Vector2 dir = endPosUI - startPosUI;
            float distance = dir.magnitude;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            sliderRect.sizeDelta = new Vector2(distance, sliderRect.sizeDelta.y);
            sliderRect.rotation = Quaternion.Euler(0, 0, angle);
        }

        AsyncOperation operation = SceneManager.LoadSceneAsync(route.sceneName, LoadSceneMode.Additive);
        operation.allowSceneActivation = false;

        float currentProgress = 0f;

        while (currentProgress < 1f)
        {
            float targetProgress = Mathf.Clamp01(operation.progress / 0.9f);
            currentProgress = Mathf.MoveTowards(currentProgress, targetProgress, Time.deltaTime * loadSpeed);

            if (loadingSlider != null) loadingSlider.value = currentProgress;

            if (planeIcon != null)
            {
                planeIcon.anchoredPosition = Vector2.Lerp(startPosUI, endPosUI, currentProgress);

                Vector2 dir = endPosUI - startPosUI;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                planeIcon.rotation = Quaternion.Euler(0, 0, angle);
            }

            if (mapTransform != null && planeIcon != null)
            {
                mapTransform.localScale = Vector3.Lerp(mapTransform.localScale, defaultMapScale * zoomLevel, Time.deltaTime * smoothSpeed);
                Vector2 targetPos = defaultMapPosition - (planeIcon.anchoredPosition * (zoomLevel - 1f));
                mapTransform.anchoredPosition = Vector2.Lerp(mapTransform.anchoredPosition, targetPos, Time.deltaTime * smoothSpeed);
            }

            yield return null;
        }

        if (loadingSlider != null) loadingSlider.value = 1f;

        operation.allowSceneActivation = true;

        while (!operation.isDone)
        {
            yield return null;
        }

        Scene newLoadedScene = SceneManager.GetSceneByName(route.sceneName);
        if (newLoadedScene.IsValid())
        {
            SceneManager.SetActiveScene(newLoadedScene);
        }

        Image[] allImages = FindObjectsOfType<Image>();
        TMP_Text[] allTexts = FindObjectsOfType<TMP_Text>();

        float fadeAlpha = 1f;
        while (fadeAlpha > 0f)
        {
            fadeAlpha -= Time.deltaTime * fadeSpeed;

            foreach (Image img in allImages)
            {
                if (img != null)
                {
                    Color c = img.color;
                    c.a = fadeAlpha;
                    img.color = c;
                }
            }

            foreach (TMP_Text txt in allTexts)
            {
                if (txt != null)
                {
                    Color c = txt.color;
                    c.a = fadeAlpha;
                    txt.color = c;
                }
            }

            yield return null;
        }

        foreach (var r in levelRoutes)
        {
            if (r.routeNameText != null)
            {
                r.routeNameText.gameObject.SetActive(false);
            }
        }

        SceneManager.UnloadSceneAsync(gameObject.scene);
    }
}