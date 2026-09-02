using UnityEngine;

public class ShatterPiece : MonoBehaviour
{
    public float destroyDelay = 3f;

    void Start()
    {
        // Förstör biten efter 3 sekunder
        Destroy(gameObject, destroyDelay);
    }
}