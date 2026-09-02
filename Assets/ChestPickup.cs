using UnityEngine;
using System.Collections;

public class ChestPickup : MonoBehaviour
{
    [Header("Rörelse & Animation")]
    public float rotationSpeed = 360f;     
    public float floatSpeed = 3f;          
    public float floatAmount = 0.15f;      

    [Header("LEGO Attraktion mot UI")]
    public float triggerRadius = 3f;       
    public float flySpeed = 18f;            
    public RectTransform targetUIElement;  
    public Camera mainCamera;

    [Header("UI Pop-inställningar")]
    public float popScale = 1.45f;         // Hur mycket 2D-ikonen förstoras vid popup
    public float popDuration = 0.18f;      // Snabb och rapp animationstid för popet

    [Header("Skatt & Effekter")]
    public int chestValue = 1;           
    public GameObject collectEffect;      
    public AudioClip collectSound;         
   
    private Vector3 startPos;
    private bool isBeingCollected = false;

    void Start()
    {
        startPos = transform.position;
        
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (targetUIElement != null && !isBeingCollected)
        {
            CanvasGroup group = targetUIElement.GetComponentInParent<CanvasGroup>();
            if (group != null && LegoScoreManager.Instance != null && LegoScoreManager.Instance.currentChests == 0)
            {
                group.alpha = 0f;
            }
        }
    }

    void Update()
    {
        if (isBeingCollected) return;

        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatAmount;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        CheckPlayerProximity();
    }

    private void CheckPlayerProximity()
    {
        PlayerHealth[] players = FindObjectsOfType<PlayerHealth>();

        foreach (PlayerHealth player in players)
        {
            if (player != null && player.isControlledByPlayer)
            {
                float distance = Vector3.Distance(transform.position, player.transform.position);
                if (distance <= triggerRadius)
                {
                        if (collectSound != null)
                        {
                            GameObject.FindObjectOfType<AudioSource>().PlayOneShot(collectSound);
                        }
                    isBeingCollected = true;
                    ShowUI();
                    StartCoroutine(FlyToUIAndAttach());
                    break;
                }
            }
        }
    }

    private void ShowUI()
    {
        if (targetUIElement != null)
        {
            CanvasGroup group = targetUIElement.GetComponentInParent<CanvasGroup>();
            if (group != null)
            {
                group.alpha = 1f;
            }
        }
    }

    private IEnumerator FlyToUIAndAttach()
    {
        Vector3 startFlyPos = transform.position;
        Vector3 startScale = transform.localScale;
        float progress = 0f;

        Vector3 GetTargetWorldPos()
        {
            if (targetUIElement != null && mainCamera != null)
            {
                Vector3 screenPoint = targetUIElement.position;
                screenPoint.z = Mathf.Abs(mainCamera.transform.position.z - transform.position.z);
                return mainCamera.ScreenToWorldPoint(screenPoint);
            }
            return transform.position;
        }

        Vector3 targetPos = GetTargetWorldPos();
        float totalDistance = Vector3.Distance(startFlyPos, targetPos);
        float duration = totalDistance / flySpeed;

        Vector3 targetScale = startScale * 0.25f; 

        while (progress < 1f)
        {
            progress += Time.deltaTime / duration;
            targetPos = GetTargetWorldPos();

            transform.position = Vector3.Lerp(startFlyPos, targetPos, progress);
            transform.localScale = Vector3.Lerp(startScale, targetScale, progress);

            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);

            yield return null;
        }

        AttachToUI();
    }

    private void AttachToUI()
    {
       
        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }

       

        if (LegoScoreManager.Instance != null)
        {
            LegoScoreManager.Instance.AddChest(chestValue);
        }

        if (targetUIElement != null)
        {
            targetUIElement.GetComponent<MonoBehaviour>()?.StartCoroutine(Do2DPopupAnimation(targetUIElement));
        }

        Destroy(gameObject);
    }

    private IEnumerator Do2DPopupAnimation(RectTransform uiElement)
    {
        Vector3 originalScale = Vector3.one;
        Vector3 targetScale = originalScale * popScale;

        float halfDuration = popDuration / 2f;
        float elapsed = 0f;

        // Blixtsnabb uppskalning med mjuk utjämning (Easing)
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            // Sin-kurva ger en snabb "bounce-out"-känsla när 3D-kistan klickar fast
            uiElement.localScale = Vector3.Lerp(originalScale, targetScale, Mathf.Sin(t * Mathf.PI * 0.5f));
            yield return null;
        }

        elapsed = 0f;

        // Mjuk återgång till standardstorleken
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            uiElement.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }

        uiElement.localScale = originalScale;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, triggerRadius);
    }
}