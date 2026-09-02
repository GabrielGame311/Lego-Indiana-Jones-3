using UnityEngine;

public class TrainPlatform : MonoBehaviour
{

    private Vector3 lastPosition;
    private Quaternion lastRotation;
    private CharacterController playerController;
    private Transform playerTransform;

    void Start()
    {
        lastPosition = transform.position;
        lastRotation = transform.rotation;
    }

    void Update()
    {
        // Om spelaren står på tåget
        if (playerController != null && playerController.enabled)
        {
            // 1. Räkna ut hur mycket tåget har flyttats (Position Delta)
            Vector3 positionDelta = transform.position - lastPosition;

            // 2. Räkna ut hur mycket tåget har roterat i kurvor (Rotation Delta)
            Quaternion rotationDelta = transform.rotation * Quaternion.Inverse(lastRotation);

            // Om tåget svängde, beräkna den nya positionen för spelaren kring tågets mittpunkt
            Vector3 positionDiff = playerTransform.position - lastPosition;
            Vector3 rotatedPositionDiff = rotationDelta * positionDiff;
            Vector3 rotationDeltaPosition = rotatedPositionDiff - positionDiff;

            // 3. Flytta spelaren med CharacterController (kombinerar tågets rörelse + svängar)
            playerController.Move(positionDelta + rotationDeltaPosition);

            // Rota även spelaren om tåget svänger i en kurva
            playerTransform.rotation = rotationDelta * playerTransform.rotation;
        }

        // Spara tågets nuvarande position och rotation för nästa frame
        lastPosition = transform.position;
        lastRotation = transform.rotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerController = other.GetComponent<CharacterController>();
            playerTransform = other.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerController = null;
            playerTransform = null;
        }
    }
}