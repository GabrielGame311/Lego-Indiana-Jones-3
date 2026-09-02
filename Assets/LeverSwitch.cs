using UnityEngine;
using System.Collections;

public class LeverSwitch : MonoBehaviour
{
    [Header("Inställningar")]
    public float interactRadius = 2.5f;     // Hur nära spelaren måste stå
    public KeyCode interactKey = KeyCode.E;  // Knapp för att dra i spaken
    public bool isActivated = false;         // Om spaken redan är dragen

    [Header("Referenser & Komponenter")]
    public Transform leverHandle;            // Dra in själva handtaget/spak-objektet som ska rotera
    public Vector3 pulledRotation = new Vector3(45f, 0f, 0f); // Målvinkel när spaken dragits ner
    public float pullSpeed = 3f;             // Hur snabbt spaken fälls ner

    [Header("Ljud & Effekter")]
    public AudioClip leverPullSound;
    public GameObject connectedMechanism;   // Objektet du vill aktivera (t.ex. en Dörr)

    private Vector3 initialRotation;

    void Start()
    {
        if (leverHandle != null)
        {
            initialRotation = leverHandle.localEulerAngles;
        }
    }

    void Update()
    {
        if (isActivated) return;

        CheckPlayerInteraction();
    }

    private void CheckPlayerInteraction()
    {
        PlayerHealth[] players = FindObjectsOfType<PlayerHealth>();

        foreach (PlayerHealth player in players)
        {
            if (player != null && player.isControlledByPlayer)
            {
                float distance = Vector3.Distance(transform.position, player.transform.position);

                // Om spelaren är nära och trycker på E
                if (distance <= interactRadius && Input.GetKeyDown(interactKey))
                {
                    PullLever();
                    break;
                }
            }
        }
    }

    public void PullLever()
    {
        isActivated = true;

        // Spela ljud
        if (leverPullSound != null)
        {
            GameObject.FindObjectOfType<AudioSource>().PlayOneShot(leverPullSound);
        }

        // Starta rörelsen för spaken
        StartCoroutine(AnimateLever());

        // Aktivera den kopplade mekanismen (t.ex. öppna dörren)
        ActivateConnectedMechanism();
    }

    private IEnumerator AnimateLever()
    {
        if (leverHandle == null) yield break;

        Quaternion startRot = leverHandle.localRotation;
        Quaternion targetRot = Quaternion.Euler(pulledRotation);
        float elapsed = 0f;

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * pullSpeed;
            leverHandle.localRotation = Quaternion.Slerp(startRot, targetRot, elapsed);
            yield return null;
        }

        leverHandle.localRotation = targetRot;
    }

    private void ActivateConnectedMechanism()
    {
        if (connectedMechanism != null)
        {
            // Om mekanismen har ett skript med en Activate()-metod eller aktiverar sin GameObject
            connectedMechanism.SendMessage("Activate", SendMessageOptions.DontRequireReceiver);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visar interaktionsradien i Scene-vyn (gul cirkel)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}