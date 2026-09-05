using UnityEngine;
using Cinemachine;

public class LegoOcclusionCamera : MonoBehaviour
{
    [Header("Referenser")]
    public CinemachineVirtualCamera vcam;
    public Transform player;
    public LayerMask obstacleMask;

    [Header("Höjd & Vinkel vid hinder")]
    public float heightOffset = 4f;     // Hur mycket kameran åker UPP
    public float pitchOffset = 15f;     // Hur mycket kameran vinklas NEDÅT
    public float smoothSpeed = 3f;      // Mjukhet i rörelsen

    [Header("Stabilisering & Zoner")]
    public float sphereRadius = 0.6f;   // Tjocklek på sökstrålen (motverkar skak)
    public float exitMarginX = 2.5f;    // Marginal i sidled (X) innan kameran går ner
    public float exitMarginZ = 2.5f;    // Marginal i djupled (Z) innan kameran går ner

    private CinemachineTransposer transposer;
    private Vector3 initialFollowOffset;
    private float initialPitch;
    
    private bool isCameraElevated = false;
    private Transform currentObstacle = null;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        vcam = GetComponent<CinemachineVirtualCamera>();
        if (vcam != null)
        {
            transposer = vcam.GetCinemachineComponent<CinemachineTransposer>();
            if (transposer != null)
            {
                initialFollowOffset = transposer.m_FollowOffset;
            }
            initialPitch = vcam.transform.localEulerAngles.x;
        }
    }

    void LateUpdate()
    {
        if (player == null || vcam == null || transposer == null) return;

        Vector3 camPos = vcam.transform.position;
        Vector3 dirToPlayer = player.position - camPos;
        float distance = dirToPlayer.magnitude;

        // 1. Skjut en tjock sfärstråle för att hitta HINDER/BUSSAR automatiskt
        bool hitObstacle = Physics.SphereCast(camPos, sphereRadius, dirToPlayer.normalized, out RaycastHit hit, distance, obstacleMask);

        if (hitObstacle)
        {
            // Spara det objekt vi står bakom automatiskt (buss, bil, vägg m.m.)
            currentObstacle = hit.transform;
            isCameraElevated = true;
        }
        else if (isCameraElevated && currentObstacle != null)
        {
            // 2. Om strålen inte träffar längre, kolla om vi fortfarande är inom marginalzonen för hindret
            float distX = Mathf.Abs(player.position.x - currentObstacle.position.x);
            float distZ = Mathf.Abs(player.position.z - currentObstacle.position.z);

            // Kameran sänks BARA om spelaren har lämnat BÅDE X- och Z-marginalen kring objektet
            if (distX > exitMarginX && distZ > exitMarginZ)
            {
                isCameraElevated = false;
                currentObstacle = null;
            }
        }

        // Mål-inställningar
        Vector3 targetOffset = initialFollowOffset;
        float targetPitch = initialPitch;

        if (isCameraElevated)
        {
            targetOffset.y += heightOffset;
            targetPitch += pitchOffset;
        }

        // Mjuk övergång för höjd och vinkel utan skak
        transposer.m_FollowOffset = Vector3.Lerp(transposer.m_FollowOffset, targetOffset, Time.deltaTime * smoothSpeed);

        Vector3 currentRot = vcam.transform.localEulerAngles;
        float newPitch = Mathf.LerpAngle(currentRot.x, targetPitch, Time.deltaTime * smoothSpeed);
        vcam.transform.localEulerAngles = new Vector3(newPitch, currentRot.y, currentRot.z);
    }
}