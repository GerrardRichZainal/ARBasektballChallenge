
using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip scoreSound;
    public AudioClip gameOverSound;

    [Header("Audio Settings")]
    public float scoreSoundDuration = 3f;

    private AudioSource audioSource;
    private Coroutine scoreCoroutine;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    public void PlayScoreSound()
    {
        if (scoreSound == null)
            return;

        if (scoreCoroutine != null)
            StopCoroutine(scoreCoroutine);

        scoreCoroutine = StartCoroutine(PlayScore());
    }

    IEnumerator PlayScore()
    {
        audioSource.Stop();
        audioSource.clip = scoreSound;
        audioSource.Play();

        yield return new WaitForSeconds(scoreSoundDuration);

        audioSource.Stop();
        scoreCoroutine = null;
    }

    public void PlayGameOverSound()
    {
        if (gameOverSound == null)
            return;

        if (scoreCoroutine != null)
        {
            StopCoroutine(scoreCoroutine);
            scoreCoroutine = null;
        }

        audioSource.Stop();
        audioSource.PlayOneShot(gameOverSound);
    }
}
