using UnityEngine;

public class InfiniteMapScroll : MonoBehaviour
{
    [Header("Kartobjekt")]
    public RectTransform map1;
    public RectTransform map2;

    [Header("Inställningar")]
    public float scrollSpeed = 100f; // Hastighet åt höger
    
    private float imageWidth;

    void Start()
    {
        // Hämtar bredden på kartbilden
        if (map1 != null)
        {
            imageWidth = map1.rect.width;
        }

        // Se till att Map2 ligger exakt till vänster om Map1 i starten
        if (map1 != null && map2 != null)
        {
            map1.anchoredPosition = new Vector2(0, 0);
            map2.anchoredPosition = new Vector2(-imageWidth, 0);
        }
    }

    void Update()
    {
        if (map1 == null || map2 == null) return;

        // Flytta båda kartorna åt höger
        map1.anchoredPosition += Vector2.right * scrollSpeed * Time.deltaTime;
        map2.anchoredPosition += Vector2.right * scrollSpeed * Time.deltaTime;

        // När Map1 har åkt helt till höger -> Flytta den till vänster om Map2
        if (map1.anchoredPosition.x >= imageWidth)
        {
            map1.anchoredPosition = new Vector2(map2.anchoredPosition.x - imageWidth, 0);
        }

        // När Map2 har åkt helt till höger -> Flytta den till vänster om Map1
        if (map2.anchoredPosition.x >= imageWidth)
        {
            map2.anchoredPosition = new Vector2(map1.anchoredPosition.x - imageWidth, 0);
        }
    }
}