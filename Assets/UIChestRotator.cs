using UnityEngine;

public class UIChestRotator : MonoBehaviour
{
    [Header("Inställningar")]
    public float rotationSpeed = 100f; // Snurrhastighet i UI-rutan

    void Update()
    {
        // Snurrar 3D-kistan konstant runt sin egen Y-axel
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);
    }
}