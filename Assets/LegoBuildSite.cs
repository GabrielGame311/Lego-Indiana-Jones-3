using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class LegoBuildSite : MonoBehaviour
{
    [Header("Byggbitar")]
    public List<Transform> looseBricks = new List<Transform>(); // Bitar på marken
    public GameObject completedLever;                         // Den färdigbyggda spaken

    [Header("Studs-inställningar (Idle)")]
    public float bounceHeight = 0.15f;
    public float bounceSpeed = 12f;

    [Header("Bygg-inställningar")]
    public float timePerBrick = 0.35f;   // Hur lång tid varje enskild bit tar
    public KeyCode buildKey = KeyCode.E;
    public float interactDistance = 3.5f;

    [Header("Slutanimation (Upp & Ner)")]
    public float finishJumpHeight = 0.6f; 
    public float finishJumpSpeed = 3.0f;  

    [Header("Ljud & Effekter")]
    public AudioClip buildSound;         
    public AudioClip completeSound;      
    public ParticleSystem completeEffect; 

    private List<Vector3> startPositions = new List<Vector3>();
    private List<Transform> leverParts = new List<Transform>();
    private bool isCompleted = false;
    private bool isBuilding = false;

    private int currentBrickIndex = 0;
    private AudioSource audioSource;
    private Animator playerAnimator;

    void Start()
    {
        // Konfigurera AudioSource
        audioSource = GetComponent<AudioSource>();
        //audioSource.spatialBlend = 1.0f; // 3D-ljud
        audioSource.volume = 1.0f;
        audioSource.playOnAwake = false;

        foreach (Transform brick in looseBricks)
        {
            if (brick != null)
                startPositions.Add(brick.localPosition);
        }

        if (completedLever != null)
        {
            completedLever.SetActive(true);

            MeshRenderer[] renderers = completedLever.GetComponentsInChildren<MeshRenderer>(true);
            foreach (MeshRenderer mr in renderers)
            {
                leverParts.Add(mr.transform);
                mr.gameObject.SetActive(false);
            }
        }
    }

    void Update()
    {
        if (isCompleted) return;

        CheckPlayerInteraction();

        if (!isBuilding)
        {
            AnimateBouncingBricks();
        }
    }

    void AnimateBouncingBricks()
    {
        for (int i = currentBrickIndex; i < looseBricks.Count; i++)
        {
            if (looseBricks[i] == null || !looseBricks[i].gameObject.activeSelf) continue;

            float offset = i * 0.8f;
            float newY = startPositions[i].y + Mathf.Abs(Mathf.Sin(Time.time * bounceSpeed + offset)) * bounceHeight;
            looseBricks[i].localPosition = new Vector3(startPositions[i].x, newY, startPositions[i].z);
        }
    }

    void CheckPlayerInteraction()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            PlayerHealth health = FindObjectOfType<PlayerHealth>();
            if (health != null) player = health.gameObject;
        }

        if (player == null)
        {
            SetPlayerBuildingAnimation(false);
            return;
        }

        // Hämta animatören från spelaren eller en av dess undermappar/children
        if (playerAnimator == null || playerAnimator.gameObject != player)
        {
            playerAnimator = player.GetComponentInChildren<Animator>();
        }

        float dist = Vector3.Distance(transform.position, player.transform.position);

        // Håll inne knappen inom räckvidd
        if (dist <= interactDistance && Input.GetKey(buildKey))
        {
            SetPlayerBuildingAnimation(true);

            if (!isBuilding && currentBrickIndex < looseBricks.Count)
            {
                StartCoroutine(BuildNextBrick());
            }
        }
        else
        {
            SetPlayerBuildingAnimation(false);
        }
    }

    void SetPlayerBuildingAnimation(bool building)
    {
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("Build", building);
        }
    }

    IEnumerator BuildNextBrick()
    {
        isBuilding = true;

        Transform currentLoose = looseBricks[currentBrickIndex];
        Transform targetPart = (currentBrickIndex < leverParts.Count) ? leverParts[currentBrickIndex] : completedLever.transform;

        Vector3 startPos = currentLoose.position;
        Quaternion startRot = currentLoose.rotation;
        Vector3 endPos = targetPart.position;
        Quaternion endRot = targetPart.rotation;

        float elapsed = 0f;

        while (elapsed < timePerBrick)
        {
            // Om spelaren släpper knappen mitt i en flygning
            if (!Input.GetKey(buildKey))
            {
                isBuilding = false;
                SetPlayerBuildingAnimation(false);
                yield break;
            }

            elapsed += Time.deltaTime;
            float t = elapsed / timePerBrick;

            Vector3 currentPos = Vector3.Lerp(startPos, endPos, t);
            currentPos.y += Mathf.Sin(t * Mathf.PI) * 0.8f;

            currentLoose.position = currentPos;
            currentLoose.rotation = Quaternion.Slerp(startRot, endRot, t) * Quaternion.Euler(0, t * 720f, 0);

            yield return null;
        }

        // Klossen landar och snäpper fast
        currentLoose.gameObject.SetActive(false);

        if (currentBrickIndex < leverParts.Count)
        {
            leverParts[currentBrickIndex].gameObject.SetActive(true);
            StartCoroutine(PopScaleEffect(leverParts[currentBrickIndex]));
        }

        if (buildSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(buildSound);
        }

        currentBrickIndex++;

        // När hela bygget är färdigt
        if (currentBrickIndex >= looseBricks.Count || currentBrickIndex >= leverParts.Count)
        {
            StartCoroutine(FinishBuildAnimation());
        }

        isBuilding = false;
    }

    IEnumerator FinishBuildAnimation()
    {
        isCompleted = true;

        // Sluta spela spelarens bygganimation direkt
        SetPlayerBuildingAnimation(false);

        foreach (Transform part in leverParts)
        {
            if (part != null) part.gameObject.SetActive(true);
        }
        foreach (Transform brick in looseBricks)
        {
            if (brick != null) brick.gameObject.SetActive(false);
        }

        if (completeSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(completeSound);
        }

        if (completeEffect != null)
        {
            completeEffect.Play();
        }

        // Slutanimation på spaken (lyfts upp och landar)
        Vector3 basePosition = completedLever.transform.localPosition;
        float elapsed = 0f;
        float duration = 0.4f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            float heightOffset = Mathf.Sin(t * Mathf.PI) * finishJumpHeight;
            completedLever.transform.localPosition = basePosition + Vector3.up * heightOffset;

            yield return null;
        }

        completedLever.transform.localPosition = basePosition;
    }

    IEnumerator PopScaleEffect(Transform target)
    {
        Vector3 originalScale = target.localScale;
        target.localScale = originalScale * 1.3f;

        float popTime = 0f;
        while (popTime < 0.1f)
        {
            popTime += Time.deltaTime;
            target.localScale = Vector3.Lerp(target.localScale, originalScale, popTime / 0.1f);
            yield return null;
        }

        target.localScale = originalScale;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactDistance);
    }
}