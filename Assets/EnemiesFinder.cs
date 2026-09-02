using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemiesFinder : MonoBehaviour
{
    public float detectionRange = 15f;
    public AudioClip alertSound;
    public AudioClip normalMusic;
    public float timeWithoutEnemy = 10f;
    public float fadeDuration = 2f;

    private GameObject[] enemies;
    private Transform player;
    public AudioSource audioSource;
    private bool enemyWasInRange;
    private Coroutine returnToMusicCoroutine;
    private Coroutine fadeInCoroutine;
    private float normalMusicVolume;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        FindPlayer();

        if (audioSource != null)
        {
            normalMusicVolume = audioSource.volume;
        }

        if (audioSource != null && normalMusic != null)
        {
            audioSource.clip = normalMusic;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    void Update()
    {
        if (player == null || !player.CompareTag("Player"))
        {
            FindPlayer();
        }

        if (player == null)
        {
            return;
        }

        enemies = GameObject.FindGameObjectsWithTag("Enemie");
        bool enemyInRange = false;

        foreach (GameObject enemy in enemies)
        {
            if (enemy != null && Vector3.Distance(player.position, enemy.transform.position) <= detectionRange)
            {
                enemyInRange = true;
                break;
            }
        }

        if (enemyInRange && !enemyWasInRange && audioSource != null && alertSound != null)
        {
            if (returnToMusicCoroutine != null)
            {
                StopCoroutine(returnToMusicCoroutine);
                returnToMusicCoroutine = null;
            }

            if (fadeInCoroutine != null)
            {
                StopCoroutine(fadeInCoroutine);
                fadeInCoroutine = null;
            }

            audioSource.clip = alertSound;
            audioSource.loop = true;
            audioSource.volume = 0f;
            audioSource.Play();
            fadeInCoroutine = StartCoroutine(FadeInAlertMusic());
        }
        else if (!enemyInRange && enemyWasInRange && audioSource != null && returnToMusicCoroutine == null)
        {
            if (fadeInCoroutine != null)
            {
                StopCoroutine(fadeInCoroutine);
                fadeInCoroutine = null;
            }

            returnToMusicCoroutine = StartCoroutine(FadeToNormalMusic());
        }

        enemyWasInRange = enemyInRange;

    
    }

    private IEnumerator FadeInAlertMusic()
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, normalMusicVolume, elapsed / fadeDuration);
            yield return null;
        }

        audioSource.volume = normalMusicVolume;
        fadeInCoroutine = null;
    }

    private IEnumerator FadeToNormalMusic()
    {
        yield return new WaitForSeconds(timeWithoutEnemy);

        if (audioSource == null || normalMusic == null)
        {
            returnToMusicCoroutine = null;
            yield break;
        }

        float startingVolume = audioSource.volume;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startingVolume, 0f, elapsed / fadeDuration);
            yield return null;
        }

        audioSource.Stop();
        audioSource.clip = normalMusic;
        audioSource.loop = true;
        audioSource.volume = normalMusicVolume;
        audioSource.Play();
        returnToMusicCoroutine = null;
    }

    private void FindPlayer()
    {
        GameObject activePlayer = GameObject.FindGameObjectWithTag("Player");
        if (activePlayer != null)
        {
            player = activePlayer.transform;
        }
    }
}
