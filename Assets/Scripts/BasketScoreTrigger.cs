
using UnityEngine;

public class BasketScoreTrigger : MonoBehaviour
{
    [Header("Distance Settings")]
    [Tooltip("Jarak mulai mendapatkan 2 poin")]
    public float mediumDistance = 1.5f;

    [Tooltip("Jarak mulai mendapatkan 3 poin")]
    public float farDistance = 2f;

    [Header("Audio Settings")]
    public AudioManager audioManager;

    private bool ballDetected = false;
    private float throwDistance = 0f;

    // Menerima jarak kamera ke ring dalam meter
    public void SetThrowDistance(float distance)
    {
        throwDistance = distance;
        ballDetected = false;

        Debug.Log(
            "Jarak lemparan tersimpan: " +
            throwDistance.ToString("F2") + " meter"
        );
    }

    // Mendeteksi bola masuk ring
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Ball"))
            return;

        if (ballDetected)
            return;

        ballDetected = true;

        int points;

        // Dekat: 1 poin
        // Menengah: 2 poin
        // Jauh: 3 poin

        if (throwDistance >= farDistance)
        {
            points = 3;
        }
        else if (throwDistance >= mediumDistance)
        {
            points = 2;
        }
        else
        {
            points = 1;
        }

        GameManager gameManager =
            Object.FindAnyObjectByType<GameManager>();

        if (gameManager != null)
        {
            gameManager.AddScore(points);
        }
        else
        {
            Debug.LogError("GameManager tidak ditemukan!");
        }

        if (audioManager != null)
        {
            audioManager.PlayScoreSound();
        }

        Debug.Log(
            "================================"
        );

        Debug.Log(
            "BOLA MASUK!"
        );

        Debug.Log(
            "JARAK: " +
            throwDistance.ToString("F2") +
            " METER"
        );

        Debug.Log(
            "POIN DIDAPATKAN: +" + points
        );

        Debug.Log(
            "================================"
        );
    }

    // Reset untuk lemparan berikutnya
    public void ResetTrigger()
    {
        ballDetected = false;
        throwDistance = 0f;

        Debug.Log("Trigger scoring di-reset!");
    }
}
