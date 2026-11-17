
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Timer Settings")]
    public TMP_Text timerText;
    public float gameDuration = 60f;

    [Header("Score Settings")]
    public TMP_Text scoreText;

    [Header("Audio Settings")]
    public AudioManager audioManager;

    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public TMP_Text finalScoreText;
    public TMP_InputField nameInput;

    [Header("Game Over Buttons")]
    public GameObject submitButton;
    public GameObject restartButton;

    [Header("Leaderboard UI")]
    public GameObject leaderboardPanel;
    public LeaderboardManager leaderboardManager;

    [Header("Main Menu UI")]
    public MainMenuUI mainMenuUI;

    private float timeRemaining;
    private int currentScore;

    private bool gameStarted;
    private bool gameOver;
    private bool scoreSubmitted;
    private bool isSubmitting;
    private bool gamePaused;
    private bool gameOverSoundPlayed;

    private void Start()
    {
        timeRemaining = gameDuration;
        currentScore = 0;

        gameStarted = false;
        gameOver = false;
        scoreSubmitted = false;
        isSubmitting = false;
        gamePaused = false;
        gameOverSoundPlayed = false;

        if (mainMenuUI == null)
            mainMenuUI = FindAnyObjectByType<MainMenuUI>();

        if (leaderboardManager == null)
            leaderboardManager = FindAnyObjectByType<LeaderboardManager>();

        UpdateTimerDisplay();
        UpdateScoreDisplay();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (leaderboardPanel != null)
            leaderboardPanel.SetActive(false);

        Debug.Log("GameManager siap.");
    }

    private void Update()
    {
        if (!gameStarted || gameOver || gamePaused)
            return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining < 0f)
            timeRemaining = 0f;

        UpdateTimerDisplay();

        if (timeRemaining <= 1f && !gameOverSoundPlayed)
        {
            gameOverSoundPlayed = true;

            if (audioManager != null)
                audioManager.PlayGameOverSound();
        }

        if (timeRemaining <= 0f)
            EndGame();
    }

    // =================================================
    // PAUSE / RESUME
    // =================================================

    public void PauseGame()
    {
        if (gameOver)
            return;

        gamePaused = true;
        Debug.Log("Game dijeda dari tombol HOME.");
    }

    public void ResumeGame()
    {
        if (gameOver)
            return;

        gamePaused = false;
        Debug.Log("Game dilanjutkan.");
    }

    public bool IsPaused()
    {
        return gamePaused;
    }

    // =================================================
    // START TIMER
    // =================================================

    public void StartTimerOnFirstThrow()
    {
        if (gameStarted || gameOver || gamePaused)
            return;

        gameStarted = true;
        Debug.Log("GAME DIMULAI!");
    }

    // =================================================
    // SCORE
    // =================================================

    public void AddScore(int points)
    {
        if (gameOver || gamePaused || points <= 0)
            return;

        currentScore += points;
        UpdateScoreDisplay();

        Debug.Log("SKOR SEKARANG: " + currentScore);
    }

    private void UpdateTimerDisplay()
    {
        if (timerText == null)
            return;

        int seconds = Mathf.CeilToInt(timeRemaining);

        timerText.text = string.Format(
            "{0:00}:{1:00}",
            seconds / 60,
            seconds % 60
        );
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
            scoreText.text = "SCORE: " + currentScore;
    }

    // =================================================
    // GAME OVER
    // =================================================

    private void EndGame()
    {
        if (gameOver)
            return;

        gameOver = true;
        gameStarted = false;
        gamePaused = false;
        timeRemaining = 0f;

        UpdateTimerDisplay();

        if (finalScoreText != null)
            finalScoreText.text = "FINAL SCORE: " + currentScore;

        if (submitButton != null)
            submitButton.SetActive(true);

        if (restartButton != null)
            restartButton.SetActive(true);

        if (mainMenuUI != null)
            mainMenuUI.ShowGameOverPanel();
        else if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Debug.Log("GAME OVER! SKOR AKHIR: " + currentScore);
    }

    // =================================================
    // SUBMIT SCORE
    // =================================================

    public void SubmitPlayerScore()
    {
        if (!gameOver || scoreSubmitted || isSubmitting)
        {
            Debug.LogWarning("Submit tidak dapat dilakukan sekarang.");
            return;
        }

        if (nameInput == null ||
            string.IsNullOrWhiteSpace(nameInput.text))
        {
            Debug.LogWarning("Masukkan nama pemain terlebih dahulu!");
            return;
        }

        if (leaderboardManager == null)
        {
            Debug.LogError("LeaderboardManager belum dihubungkan!");
            return;
        }

        if (PlayFabManager.Instance == null ||
            !PlayFabManager.Instance.IsReady)
        {
            Debug.LogWarning("PlayFab belum siap. Coba lagi sebentar.");
            return;
        }

        string playerName = nameInput.text.Trim();

        isSubmitting = true;

        if (submitButton != null)
        {
            Button button = submitButton.GetComponent<Button>();

            if (button != null)
                button.interactable = false;
        }

        leaderboardManager.AddPlayer(playerName, currentScore);

        Debug.Log("Mengirim skor " + currentScore + " atas nama " + playerName);
    }

    public void OnScoreSubmittedSuccessfully()
    {
        isSubmitting = false;
        scoreSubmitted = true;

        if (submitButton != null)
            submitButton.SetActive(false);

        if (mainMenuUI != null)
        {
            mainMenuUI.OpenLeaderboardAfterGameOver();
        }
        else
        {
            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);

            if (leaderboardPanel != null)
                leaderboardPanel.SetActive(true);

            if (leaderboardManager != null)
                leaderboardManager.RefreshLeaderboard();
        }

        Debug.Log("SKOR BERHASIL DISIMPAN ONLINE!");
    }

    public void OnScoreSubmissionFailed()
    {
        isSubmitting = false;

        if (submitButton != null)
        {
            submitButton.SetActive(true);

            Button button = submitButton.GetComponent<Button>();

            if (button != null)
                button.interactable = true;
        }

        Debug.LogWarning("Submit gagal. Periksa koneksi dan coba lagi.");
    }

    // =================================================
    // LEADERBOARD
    // =================================================

    public void OpenLeaderboard()
    {
        if (mainMenuUI != null)
        {
            mainMenuUI.OpenLeaderboard();
            return;
        }

        if (leaderboardPanel != null)
            leaderboardPanel.SetActive(true);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (leaderboardManager != null)
            leaderboardManager.RefreshLeaderboard();
    }

    public void CloseLeaderboard()
    {
        if (mainMenuUI != null)
        {
            mainMenuUI.BackFromLeaderboard();
            return;
        }

        if (leaderboardPanel != null)
            leaderboardPanel.SetActive(false);

        if (gameOver && gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (scoreSubmitted && submitButton != null)
            submitButton.SetActive(false);

        if (restartButton != null)
            restartButton.SetActive(true);
    }

    // =================================================
    // RESTART
    // =================================================

    public void RestartGame()
    {
        Debug.Log("Memulai permainan baru...");

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    // =================================================
    // GETTERS
    // =================================================

    public int GetCurrentScore()
    {
        return currentScore;
    }

    public bool IsGameOver()
    {
        return gameOver;
    }

    public bool IsGameStarted()
    {
        return gameStarted;
    }

    public bool IsScoreSubmitted()
    {
        return scoreSubmitted;
    }
}
