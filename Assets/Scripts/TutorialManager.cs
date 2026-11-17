using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    [Header("Tutorial UI")]
    public GameObject tutorialPanel;
    public TMP_Text tutorialTitle;
    public TMP_Text tutorialDescription;
    public TMP_Text pageIndicator;

    [Header("Buttons")]
    public Button nextButton;
    public Button startButton;

    [Tooltip("Opsional: tombol kembali ke halaman sebelumnya")]
    public Button previousButton;

    [Tooltip("Opsional: tombol kembali ke Main Menu (dibuat otomatis kalau kosong)")]
    public Button backToMenuButton;

    [Header("Gambar Tiap Halaman (Opsional)")]
    [Tooltip("Isi dengan objek ilustrasi per halaman. Boleh kosong atau kurang dari jumlah halaman.")]
    public GameObject[] pageVisuals;

    [Header("Swipe Animation")]
    public RectTransform tutorialFinger;
    public float animationDistance = 100f;
    public float animationDuration = 0.8f;

    [Header("Game Reference")]
    public BallController ballController;
    public GameManager gameManager;
    public MainMenuUI mainMenuUI;

    [Header("Input Settings")]
    public CanvasGroup tutorialCanvasGroup;

    private int currentPage;
    private bool styled;
    private Coroutine fingerAnimation;
    private Vector2 fingerStartPosition;
    private bool fingerPositionSaved;

    private string[] titles;
    private string[] descriptions;

    // =================================================
    // UNITY
    // =================================================

    private void Awake()
    {
        BuildPages();
    }

    private void OnEnable()
    {
        RegisterButtons();
    }

    private void Start()
    {
        FindReferences();
        BuildPages();
        RegisterButtons();

        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);

        if (ballController != null)
            ballController.SetCanThrowBall(false);

        Debug.Log("TutorialManager siap.");
    }

    private void OnDisable()
    {
        UnregisterButtons();
        StopFinger();
    }

    private void OnDestroy()
    {
        StopFinger();
    }

    // =================================================
    // SETUP
    // =================================================

    private void BuildPages()
    {
        float duration = 60f;

        if (gameManager != null)
            duration = gameManager.gameDuration;

        titles = new[]
        {
            "CARA MELEMPAR BOLA",
            "FUNGSI TIMER",
            "SISTEM SKOR",
            "SCAN GAMBAR INFORMATIKA"
        };

        descriptions = new[]
        {
            "Sentuh bola di bagian bawah layar, lalu geser jari ke ATAS (swipe up) untuk melempar bola menuju ring. Semakin panjang geseran, semakin kuat lemparannya.",

            "Timer ada di bagian atas layar dan menghitung mundur dari " +
            Mathf.RoundToInt(duration) +
            " detik. Timer mulai berjalan saat bola pertama kali disentuh. Kalau waktu habis, permainan selesai.",

            "SKOR ada di layar dan bertambah saat bola masuk ring.\n" +
            "Lemparan dekat = 1 poin\n" +
            "Lemparan menengah = 2 poin\n" +
            "Lemparan jauh = 3 poin\n" +
            "Makin jauh kamu dari ring, makin besar poinnya!",

            "Arahkan kamera ke gambar INFORMATIKA sampai ring basket muncul. Setelah ring muncul, sentuh bola dan swipe ke atas untuk menembak. Selamat bermain!"
        };
    }

    private void FindReferences()
    {
        if (mainMenuUI == null)
            mainMenuUI = FindAnyObjectByType<MainMenuUI>();

        if (ballController == null)
            ballController = FindAnyObjectByType<BallController>();

        if (gameManager == null)
            gameManager = FindAnyObjectByType<GameManager>();

        if (tutorialPanel == null)
            tutorialPanel = GameObject.Find("TutorialPanel");
    }

    private void RegisterButtons()
    {
        UnregisterButtons();

        if (nextButton != null)
            nextButton.onClick.AddListener(NextPage);

        if (startButton != null)
            startButton.onClick.AddListener(StartGameplay);

        if (previousButton != null)
            previousButton.onClick.AddListener(PreviousPage);

        if (backToMenuButton != null)
            backToMenuButton.onClick.AddListener(OnBackPressed);  
    }

    private void UnregisterButtons()
    {
        if (nextButton != null)
            nextButton.onClick.RemoveListener(NextPage);

        if (startButton != null)
            startButton.onClick.RemoveListener(StartGameplay);

        if (previousButton != null)
            previousButton.onClick.RemoveListener(PreviousPage);

        if (backToMenuButton != null)
            backToMenuButton.onClick.RemoveListener(OnBackPressed); 
    }

    private bool CheckReferences()
    {
        bool valid = true;

        if (tutorialPanel == null) { Debug.LogError("TutorialPanel belum dihubungkan!"); valid = false; }
        if (tutorialTitle == null) { Debug.LogError("TutorialTitle belum dihubungkan!"); valid = false; }
        if (tutorialDescription == null) { Debug.LogError("TutorialDescription belum dihubungkan!"); valid = false; }
        if (pageIndicator == null) { Debug.LogError("PageIndicator belum dihubungkan!"); valid = false; }
        if (nextButton == null) { Debug.LogError("NextButton belum dihubungkan!"); valid = false; }
        if (startButton == null) { Debug.LogError("StartButton belum dihubungkan!"); valid = false; }

        return valid;
    }
    
    private void UpdateBackButtonLabel()
    {
        if (backToMenuButton == null)
            return;

        TMP_Text label =
            backToMenuButton.GetComponentInChildren<TMP_Text>(true);

        if (label != null)
            label.text = currentPage == 0 ? "MENU" : "KEMBALI";
    }
    // =================================================
    // BUKA TUTORIAL (dipanggil tombol TUTORIAL)
    // =================================================

    public bool OpenTutorial()
    {
        FindReferences();
        BuildPages();

        if (!CheckReferences())
            return false;

        ApplyTutorialStyle();
        RegisterButtons();

        StopFinger();

        // Selama tutorial, bola tidak boleh dilempar
        if (ballController != null)
            ballController.SetCanThrowBall(false);

        SetTutorialGroupVisible(true);

        tutorialPanel.SetActive(true);

        if (mainMenuUI != null)
            mainMenuUI.ShowTutorialPanel();

        ShowPage(0);

        Debug.Log("Tutorial dibuka.");
        return true;
    }

    // =================================================
    // HALAMAN
    // =================================================

    private void ShowPage(int page)
    {
        if (page < 0 || page >= titles.Length)
            return;

        currentPage = page;

        tutorialTitle.text = titles[page];
        tutorialDescription.text = descriptions[page];
        pageIndicator.text = "Halaman " + (page + 1) + " / " + titles.Length;

        bool firstPage = page == 0;
        bool lastPage = page == titles.Length - 1;

        nextButton.gameObject.SetActive(!lastPage);
        startButton.gameObject.SetActive(lastPage);

        if (previousButton != null)
            previousButton.gameObject.SetActive(!firstPage);

        // Ilustrasi per halaman
        if (pageVisuals != null)
        {
            for (int i = 0; i < pageVisuals.Length; i++)
            {
                if (pageVisuals[i] != null)
                    pageVisuals[i].SetActive(i == page);
            }
        }

        // Animasi jari hanya di halaman cara melempar
        StopFinger();

        if (tutorialFinger != null)
        {
            tutorialFinger.gameObject.SetActive(firstPage);

            if (firstPage)
                fingerAnimation = StartCoroutine(AnimateFinger());
        }

        Canvas.ForceUpdateCanvases();
    }

    
    public void NextPage()
    {
        if (currentPage >= 0 && currentPage < titles.Length - 1)
        {
            ShowPage(currentPage + 1);
        }
    }

    public void PreviousPage()
    {
        if (currentPage > 0 && currentPage < titles.Length)
        {
            ShowPage(currentPage - 1);
        }
    }

    public void OnBackPressed()
    {
        if (currentPage > 0)
        {
            PreviousPage();
        }
        else
        {
            BackToMenu();
        }
    }


    // =================================================
    // MULAI GAME (dipanggil PLAY GAME dan tombol START di tutorial)
    // =================================================

    public void StartGameplay()
    {
        FindReferences();

        StopFinger();

        if (tutorialFinger != null)
            tutorialFinger.gameObject.SetActive(false);

        SetTutorialGroupVisible(false);

        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);

        if (ballController != null)
            ballController.SetCanThrowBall(true);
        else
            Debug.LogError("BallController tidak ditemukan!");

        if (gameManager != null)
            gameManager.ResumeGame();

        // Menampilkan timer, skor, dan tombol HOME
        if (mainMenuUI != null)
            mainMenuUI.ShowGameplayHUD();
        else
            Debug.LogError("MainMenuUI tidak ditemukan!");

        Debug.Log("Gameplay dimulai. Timer mulai saat bola pertama disentuh.");
    }

    public void BackToMenu()
    {
        StopFinger();
        SetTutorialGroupVisible(false);

        if (ballController != null)
            ballController.SetCanThrowBall(false);

        if (mainMenuUI != null)
            mainMenuUI.ShowMainMenu();
    }

    // =================================================
    // TAMPILAN TUTORIAL (dibuat lewat kode)
    // =================================================

    private void ApplyTutorialStyle()
    {
        if (styled || tutorialPanel == null)
            return;

        styled = true;

        Color orange = new Color32(239, 112, 35, 255);
        Color dark = new Color32(55, 45, 38, 255);
        Color cream = new Color32(255, 248, 238, 255);

        Sprite rounded =
            mainMenuUI != null ? mainMenuUI.GetRoundedSprite() : null;

        // Panel penuh layar dengan latar gelap transparan
        Stretch(tutorialPanel.GetComponent<RectTransform>());

        Image panelImage = tutorialPanel.GetComponent<Image>();

        if (panelImage == null)
            panelImage = tutorialPanel.AddComponent<Image>();

        panelImage.sprite = null;
        panelImage.type = Image.Type.Simple;
        panelImage.color = new Color(0f, 0f, 0f, 0.65f);
        panelImage.raycastTarget = true;

        // Kartu di tengah
        GameObject card = new GameObject(
            "TutorialCard",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );

        card.transform.SetParent(tutorialPanel.transform, false);
        card.transform.SetAsFirstSibling();
        SetBox(card.GetComponent<RectTransform>(), 0.07f, 0.20f, 0.93f, 0.78f);

        Image cardImage = card.GetComponent<Image>();
        cardImage.sprite = rounded;
        cardImage.type = rounded != null
            ? Image.Type.Sliced
            : Image.Type.Simple;
        cardImage.color = cream;
        cardImage.raycastTarget = false;

        // Teks
        StyleText(tutorialTitle, 0.12f, 0.66f, 0.88f, 0.75f, orange, 54, true);
        StyleText(tutorialDescription, 0.12f, 0.43f, 0.88f, 0.65f, dark, 34, false);
        StyleText(pageIndicator, 0.12f, 0.285f, 0.88f, 0.325f, dark, 24, false);

        // Jari animasi (di bawah teks)
        if (tutorialFinger != null)
        {
            tutorialFinger.anchorMin = new Vector2(0.5f, 0.36f);
            tutorialFinger.anchorMax = new Vector2(0.5f, 0.36f);
            tutorialFinger.anchoredPosition = Vector2.zero;
            fingerPositionSaved = false;
        }

        // Tombol kembali (dibuat otomatis kalau belum ada)
        if (backToMenuButton == null)
            backToMenuButton = CreateButton("KEMBALI");

        StyleButton(backToMenuButton, 0.12f, 0.215f, 0.46f, 0.275f,
            Color.white, orange, rounded);

        // Tombol NEXT dan START di posisi yang sama
        StyleButton(nextButton, 0.54f, 0.215f, 0.88f, 0.275f,
            orange, Color.white, rounded);

        StyleButton(startButton, 0.54f, 0.215f, 0.88f, 0.275f,
            orange, Color.white, rounded);

        if (previousButton != null)
            StyleButton(previousButton, 0.12f, 0.285f, 0.30f, 0.325f,
                Color.white, orange, rounded);
    }

    private Button CreateButton(string label)
    {
        GameObject obj = new GameObject(
            "Button_" + label,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button)
        );

        obj.transform.SetParent(tutorialPanel.transform, false);

        GameObject textObj = new GameObject(
            "Label",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI)
        );

        textObj.transform.SetParent(obj.transform, false);

        TextMeshProUGUI text = textObj.GetComponent<TextMeshProUGUI>();
        text.text = label;

        return obj.GetComponent<Button>();
    }

    private void StyleButton(
        Button button, float x0, float y0, float x1, float y1,
        Color bg, Color fg, Sprite sprite)
    {
        if (button == null)
            return;

        SetBox(button.GetComponent<RectTransform>(), x0, y0, x1, y1);

        Image image = button.GetComponent<Image>();

        if (image != null)
        {
            image.sprite = sprite;
            image.type = sprite != null
                ? Image.Type.Sliced
                : Image.Type.Simple;
            image.color = bg;
            image.raycastTarget = true;
            button.targetGraphic = image;
        }

        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 1f, 1f, 0.92f);
        colors.pressedColor = new Color(0.82f, 0.82f, 0.82f, 1f);
        button.colors = colors;

        TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);

        if (label != null)
        {
            Stretch(label.rectTransform);
            label.color = fg;
            label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.Center;
            label.enableAutoSizing = true;
            label.fontSizeMin = 14;
            label.fontSizeMax = 34;
            label.raycastTarget = false;
        }
    }

    private void StyleText(
        TMP_Text text, float x0, float y0, float x1, float y1,
        Color color, float maxSize, bool bold)
    {
        if (text == null)
            return;

        SetBox(text.rectTransform, x0, y0, x1, y1);

        text.color = color;
        text.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.enableAutoSizing = true;
        text.fontSizeMin = 14;
        text.fontSizeMax = maxSize;
        text.raycastTarget = false;
    }

    private void SetBox(RectTransform rect, float x0, float y0, float x1, float y1)
    {
        rect.anchorMin = new Vector2(x0, y0);
        rect.anchorMax = new Vector2(x1, y1);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    // =================================================
    // CANVAS GROUP
    // =================================================

    // CanvasGroup hanya disentuh kalau berada di TutorialPanel.
    // Kalau ada di Canvas utama, alpha 0 menyembunyikan SEMUA UI.
    private void SetTutorialGroupVisible(bool visible)
    {
        if (tutorialCanvasGroup == null || tutorialPanel == null)
            return;

        if (tutorialCanvasGroup.gameObject != tutorialPanel)
        {
            Debug.LogWarning("tutorialCanvasGroup bukan di TutorialPanel, diabaikan.");
            return;
        }

        tutorialCanvasGroup.alpha = visible ? 1f : 0f;
        tutorialCanvasGroup.interactable = visible;
        tutorialCanvasGroup.blocksRaycasts = visible;
    }

    // =================================================
    // ANIMASI JARI
    // =================================================

    private void StopFinger()
    {
        if (fingerAnimation != null)
        {
            StopCoroutine(fingerAnimation);
            fingerAnimation = null;
        }

        if (tutorialFinger != null && fingerPositionSaved)
            tutorialFinger.anchoredPosition = fingerStartPosition;
    }

    private IEnumerator AnimateFinger()
    {
        if (tutorialFinger == null)
            yield break;

        if (!fingerPositionSaved)
        {
            fingerStartPosition = tutorialFinger.anchoredPosition;
            fingerPositionSaved = true;
        }

        Vector2 start = fingerStartPosition;
        Vector2 end = start + Vector2.up * animationDistance;

        while (true)
        {
            float elapsed = 0f;

            while (elapsed < animationDuration)
            {
                elapsed += Time.unscaledDeltaTime;

                tutorialFinger.anchoredPosition = Vector2.Lerp(
                    start, end, Mathf.Clamp01(elapsed / animationDuration));

                yield return null;
            }

            yield return new WaitForSecondsRealtime(0.2f);

            tutorialFinger.anchoredPosition = start;

            yield return new WaitForSecondsRealtime(0.2f);
        }
    }
}