using UnityEngine;
using System.Collections;
using Cinemachine;

public class CharacterSwitcher : MonoBehaviour
{
    [Header("Spelare")]
    public GameObject player1;
    public GameObject player2;

    [Header("Effekt Prefab")]
    public GameObject switchTrailPrefab; // Dra in din SwitchTrailEffect-prefab här!

    [Header("UI & Kamera")]
    public HealthUI healthUI;
    public CinemachineVirtualCamera virtualCamera;

    [Header("Inställningar")]
    public float flySpeed = 15f;    // Hur snabbt effekten flyger mellan gubbarna
    public float spinDuration = 0.2f; // Snurr när effekten kommer fram
    public GameObjectBlinker player1Indicator;
    public GameObjectBlinker player2Indicator;    
    private bool isControllingPlayer1 = true;
    private bool isSwitching = false;
    public AudioClip SwitchSound;


    void Start()
    {
        SetActivePlayer(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && !isSwitching)
        {
            StartCoroutine(FlyEffectAndSwitch());
        }
    }

   private IEnumerator FlyEffectAndSwitch()
{
    isSwitching = true;

    if (SwitchSound != null)
    {
        GameObject.FindObjectOfType<AudioSource>().PlayOneShot(SwitchSound);
    }

    // 1. Skapa flygande trail-objektet
    GameObject fromPlayer = isControllingPlayer1 ? player1 : player2;
    GameObject toPlayer = isControllingPlayer1 ? player2 : player1;

    Vector3 startPos = fromPlayer.transform.position + Vector3.up * 1f;
    Vector3 targetPos = toPlayer.transform.position + Vector3.up * 1f;

    GameObject trailObj = null;
    if (switchTrailPrefab != null)
    {
        trailObj = Instantiate(switchTrailPrefab, startPos, Quaternion.identity);
    }

    // 2. Flytta trail-effekten
    if (trailObj != null)
    {
        float distance = Vector3.Distance(startPos, targetPos);
        float progress = 0f;

        while (progress < 1f)
        {
            progress += (flySpeed * Time.deltaTime) / distance;
            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, progress);
            currentPos.y += Mathf.Sin(progress * Mathf.PI) * 1.5f;
            trailObj.transform.position = currentPos;
            yield return null;
        }

        Destroy(trailObj, 0.3f);
    }

    // 3. Gör karaktärsbytet när effekten når fram (Blinkar fram den NVA indikatorn)
    isControllingPlayer1 = !isControllingPlayer1;
    SetActivePlayer(isControllingPlayer1);

    // 4. Snurra den nya spelaren
    float elapsed = 0f;
    Quaternion startRot = toPlayer.transform.rotation;

    while (elapsed < spinDuration)
    {
        elapsed += Time.deltaTime;
        float angle = (elapsed / spinDuration) * 360f;
        toPlayer.transform.rotation = startRot * Quaternion.Euler(0, angle, 0);
        yield return null;
    }

    toPlayer.transform.rotation = startRot;
    isSwitching = false;
}

    void SetActivePlayer(bool controlP1)
{
    var p1Movement = player1.GetComponent<LegoCharacterController>();
    var p2Movement = player2.GetComponent<LegoCharacterController>();

    var p1Follow = player1.GetComponent<FollowCompanion>();
    var p2Follow = player2.GetComponent<FollowCompanion>();

   

    var p1Health = player1.GetComponent<PlayerHealth>();
    var p2Health = player2.GetComponent<PlayerHealth>();

    var p1Attack = player1.GetComponent<PlayerAttack>();
    var p2Attack = player2.GetComponent<PlayerAttack>();

    if (controlP1)
    {
        // --- SPELARE 1 AKTIVERAS ---
       
        if (p1Movement != null) p1Movement.enabled = true;
        if (p1Follow != null) p1Follow.enabled = false;
        if (p1Health != null) p1Health.isControlledByPlayer = true;
        if (p1Attack != null) p1Attack.isControlledByPlayer = true;
        player1.tag = "Player";

        // Blinka fram indikatorn på Spelare 1
        if (player1Indicator != null) player1Indicator.BlinkAndShow();

        // --- SPELARE 2 SLÄCKS ---
        
        if (p2Movement != null) p2Movement.enabled = false;
        if (p2Follow != null) p2Follow.SetTarget(player1.transform);
        if (p2Health != null) p2Health.isControlledByPlayer = false;
        if (p2Attack != null) p2Attack.isControlledByPlayer = false;
        player2.tag = "Untagged";

        // Släck indikatorn direkt på Spelare 2 (ingen blinkning)
        if (player2Indicator != null) player2Indicator.HideDirectly();

        if (virtualCamera != null) virtualCamera.Follow = player1.transform;
        if (healthUI != null && p1Health != null) healthUI.UpdateHearts(p1Health.currentHealth);
    }
    else
    {
        // --- SPELARE 2 AKTIVERAS ---
       
        if (p2Movement != null) p2Movement.enabled = true;
        if (p2Follow != null) p2Follow.enabled = false;
        if (p2Health != null) p2Health.isControlledByPlayer = true;
        if (p2Attack != null) p2Attack.isControlledByPlayer = true;
        player2.tag = "Player";

        // Blinka fram indikatorn på Spelare 2
        if (player2Indicator != null) player2Indicator.BlinkAndShow();

        // --- SPELARE 1 SLÄCKS ---
        
        if (p1Movement != null) p1Movement.enabled = false;
        if (p1Follow != null) p1Follow.SetTarget(player2.transform);
        if (p1Health != null) p1Health.isControlledByPlayer = false;
        if (p1Attack != null) p1Attack.isControlledByPlayer = false;
        player1.tag = "Untagged";

        // Släck indikatorn direkt på Spelare 1 (ingen blinkning)
        if (player1Indicator != null) player1Indicator.HideDirectly();

        if (virtualCamera != null) virtualCamera.Follow = player2.transform;
        if (healthUI != null && p2Health != null) healthUI.UpdateHearts(p2Health.currentHealth);
    }
}
}