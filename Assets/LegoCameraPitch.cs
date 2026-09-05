using UnityEngine;
using Cinemachine;

public class LegoCameraPitch : MonoBehaviour
{
    [Header("Referenser")]
    public CinemachineVirtualCamera vcam;
    public Transform player;

    [Header("Inställningar för vinkel")]
    public float defaultPitch = 15f;    // Normal vinkel
    public float maxPitch = 35f;        // Vinkel när spelaren är nära kameran
    public float zThreshold = -5f;      // Avstånd längs Z-axeln där vinklingen startar
    public float smoothSpeed = 3f;      // Hur mjukt kameran vinklas


    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        vcam = GetComponent<CinemachineVirtualCamera>();
    }

    void Update()
    {
        if (player == null || vcam == null) return;

        // Räkna ut hur nära kameran spelaren är längs Z-axeln
        float zDistance = player.position.z - transform.position.z;

        // Om spelaren går nära skärmen/kameran, vinkla upp kameran
        float targetPitch = defaultPitch;
        if (zDistance > zThreshold)
        {
            float t = Mathf.InverseLerp(zThreshold, zThreshold + 5f, zDistance);
            targetPitch = Mathf.Lerp(defaultPitch, maxPitch, t);
        }

        // Mjuk övergång till den nya vinkeln
        Vector3 currentRot = vcam.transform.localEulerAngles;
        float newPitch = Mathf.LerpAngle(currentRot.x, targetPitch, Time.deltaTime * smoothSpeed);
        vcam.transform.localEulerAngles = new Vector3(newPitch, currentRot.y, currentRot.z);
    }
}