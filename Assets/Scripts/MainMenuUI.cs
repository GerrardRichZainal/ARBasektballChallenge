using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("CANVAS")]
    public Canvas targetCanvas;

    [Header("PANELS")]
    public GameObject tutorialPanel;
    public GameObject leaderboardPanel;
    public GameObject gameplayPanel;
    public GameObject gameOverPanel;

    [Header("GAME REFERENCES")]
    public TutorialManager tutorialManager;
    public GameManager gameManager;
    public LeaderboardManager leaderboardManager;

    [Header("GAMEPLAY HUD")]
    public GameObject timerTextObject;
    public GameObject scoreTextObject;

    [Tooltip("Ukuran font timer dan skor (sama besar). Ubah di sini.")]
    public float hudFontSize = 48f;

    [Header("MAIN MENU IMAGE")]
    public Sprite basketballSprite;

    [Header("BACKGROUND REMOVAL")]
    public bool removeBackground = true;

    [Range(0.01f, 0.5f)]
    public float backgroundTolerance = 0.16f;

    [Range(0.01f, 0.3f)]
    public float edgeSmoothness = 0.10f;

    [Header("COLORS")]
    public Color backgroundColor = new Color32(255, 248, 238, 255);
    public Color orangeColor = new Color32(239, 112, 35, 255);
    public Color darkColor = new Color32(55, 45, 38, 255);

    private GameObject mainMenuRoot;
    private GameObject homeButtonObject;

    private Sprite roundedSprite;
    private Sprite circleSprite;
    private Sprite transparentBasketballSprite;
    private Texture2D transparentBasketballTexture;

    private bool openedFromGameOver;

    // =================================================
    // HUD (timer kiri atas, skor kanan atas, ukuran sama)
    // =================================================

    private void AlignHud()
    {
        StyleHudText(timerTextObject, 0.05f, 0.91f, 0.50f, 0.98f,
            TextAlignmentOptions.MidlineLeft);

        StyleHudText(scoreTextObject, 0.50f, 0.91f, 0.95f, 0.98f,
            TextAlignmentOptions.MidlineRight);
    }

    private void StyleHudText(
        GameObject obj, float x0, float y0, float x1, float y1,
        TextAlignmentOptions align)
    {
        if (obj == null)
            return;

        RectTransform rect = obj.GetComponent<RectTransform>();

        if (rect != null)
        {
            SetBox(rect, x0, y0, x1, y1);

            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.localScale = Vector3.one;
        }

        TMP_Text text = obj.GetComponent<TMP_Text>();

        if (text == null)
            return;

        text.enableAutoSizing = false;
        text.fontSize = hudFontSize;
        text.alignment = align;
        text.fontStyle = FontStyles.Bold;
        text.color = Color.white;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Overflow;
        text.margin = Vector4.zero;
        text.raycastTarget = false;
    }

    // =================================================
    // START
    // =================================================

    private void Start()
    {
        EnsureEventSystem();
        FindReferences();

        if (targetCanvas == null)
        {
            Debug.LogError("MainMenuUI: Canvas tidak ditemukan.");
            enabled = false;
            return;
        }

        if (tutorialManager != null)
            tutorialManager.mainMenuUI = this;

        if (gameManager != null)
            gameManager.mainMenuUI = this;

        AlignHud();
        BuildMainMenu();
        BuildHomeButton();

        ShowMainMenu();

        Debug.Log("MainMenuUI berhasil dijalankan.");
    }

    private void EnsureEventSystem()
    {
        if (FindAnyObjectByType<EventSystem>() != null)
            return;

        new GameObject(
            "EventSystem",
            typeof(EventSystem),
            typeof(StandaloneInputModule)
        );

        Debug.LogWarning("EventSystem tidak ada, dibuat otomatis.");
    }

    private void FindReferences()
    {
        if (targetCanvas == null)
            targetCanvas = FindAnyObjectByType<Canvas>();

        if (tutorialManager == null)
            tutorialManager = FindAnyObjectByType<TutorialManager>();

        if (gameManager == null)
            gameManager = FindAnyObjectByType<GameManager>();

        if (leaderboardManager == null)
            leaderboardManager = FindAnyObjectByType<LeaderboardManager>();

        // Ambil dari referensi yang sudah ada (lebih aman
        // dibanding Find, karena Find tidak menemukan objek nonaktif)
        if (timerTextObject == null &&
            gameManager != null && gameManager.timerText != null)
            timerTextObject = gameManager.timerText.gameObject;

        if (scoreTextObject == null &&
            gameManager != null && gameManager.scoreText != null)
            scoreTextObject = gameManager.scoreText.gameObject;

        if (tutorialPanel == null && tutorialManager != null)
            tutorialPanel = tutorialManager.tutorialPanel;

        if (gameOverPanel == null && gameManager != null)
            gameOverPanel = gameManager.gameOverPanel;

        if (leaderboardPanel == null && gameManager != null)
            leaderboardPanel = gameManager.leaderboardPanel;

        if (tutorialPanel == null)
            tutorialPanel = GameObject.Find("TutorialPanel");

        if (leaderboardPanel == null)
            leaderboardPanel = GameObject.Find("LeaderBoardPanel");

        if (gameOverPanel == null)
            gameOverPanel = GameObject.Find("GameOverPanel");

        if (timerTextObject == null)
            Debug.LogWarning("MainMenuUI: TimerText tidak ditemukan. Isi manual di Inspector.");

        if (scoreTextObject == null)
            Debug.LogWarning("MainMenuUI: ScoreText tidak ditemukan. Isi manual di Inspector.");
    }

    private void SetActive(GameObject obj, bool value)
    {
        if (obj != null && obj.activeSelf != value)
            obj.SetActive(value);
    }

    // Satu fungsi untuk mengatur semua layar
    private void ShowOnly(
        GameObject main = null,
        GameObject tutorial = null,
        GameObject leaderboard = null,
        GameObject gameOver = null,
        bool gameplay = false)
    {
        SetActive(mainMenuRoot, main != null);
        SetActive(tutorialPanel, tutorial != null);
        SetActive(leaderboardPanel, leaderboard != null);
        SetActive(gameOverPanel, gameOver != null);

        SetActive(gameplayPanel, gameplay);
        SetActive(timerTextObject, gameplay);
        SetActive(scoreTextObject, gameplay);
        SetActive(homeButtonObject, gameplay);

        GameObject top = main ?? tutorial ?? leaderboard ?? gameOver;

        if (top != null)
            top.transform.SetAsLastSibling();

        if (gameplay && homeButtonObject != null)
            homeButtonObject.transform.SetAsLastSibling();

        Canvas.ForceUpdateCanvases();
    }

    // =================================================
    // MAIN MENU (layout berbasis persentase layar)
    // =================================================

    private void BuildMainMenu()
    {
        if (mainMenuRoot != null)
            Destroy(mainMenuRoot);

        mainMenuRoot = new GameObject(
            "MainMenuRoot",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );

        mainMenuRoot.transform.SetParent(targetCanvas.transform, false);

        Stretch(mainMenuRoot.GetComponent<RectTransform>());

        Image background = mainMenuRoot.GetComponent<Image>();
        background.color = backgroundColor;
        background.raycastTarget = true;

        Transform root = mainMenuRoot.transform;

        // Lingkaran dekorasi + logo di posisi yang SAMA
        CreateSquare(root, "DecorationCircle", 0.62f, 0.48f,
            GetCircleSprite(), new Color(1f, 0.55f, 0.2f, 0.12f));

        CreateBasketball(root, 0.62f, 0.68f);

        CreateText(root, "AR BASKETBALL",
            0.05f, 0.85f, 0.95f, 0.93f, darkColor, 54, FontStyles.Bold);

        CreateText(root, "CHALLENGE",
            0.05f, 0.79f, 0.95f, 0.85f, orangeColor, 43, FontStyles.Bold);

        CreateText(root, "UJI KEMAMPUAN SHOOTING-MU!",
            0.05f, 0.355f, 0.95f, 0.405f, darkColor, 26, FontStyles.Bold);

        CreateText(root, "Swipe up and shoot!",
            0.05f, 0.31f, 0.95f, 0.355f, darkColor, 23, FontStyles.Normal);

        CreateButton(root, "PLAY GAME",
            0.10f, 0.215f, 0.90f, 0.295f,
            orangeColor, Color.white, PlayGame);

        CreateButton(root, "TUTORIAL",
            0.10f, 0.125f, 0.90f, 0.195f,
            Color.white, orangeColor, OpenTutorial);

        CreateButton(root, "LEADERBOARD",
            0.10f, 0.035f, 0.90f, 0.105f,
            Color.white, orangeColor, OpenLeaderboard);

        CreateText(root, "INFORMATIKA AR BASKETBALL CHALLENGE",
            0.05f, 0.0f, 0.95f, 0.03f, darkColor, 18, FontStyles.Normal);
    }

    private void CreateBasketball(Transform parent, float centerY, float widthPercent)
    {
        Sprite sprite = null;

        if (basketballSprite != null)
        {
            sprite = removeBackground
                ? CreateTransparentSprite(basketballSprite)
                : basketballSprite;
        }
        else
        {
            Debug.LogWarning("Basketball Sprite belum dipasang.");
        }

        if (sprite == null)
            return;

        CreateSquare(parent, "BasketballImage", centerY, widthPercent,
            sprite, Color.white);
    }

    // Gambar persegi yang lebarnya = persen lebar layar,
    // tinggi otomatis sama dengan lebar (AspectRatioFitter)
    private void CreateSquare(
        Transform parent,
        string objName,
        float centerY,
        float widthPercent,
        Sprite sprite,
        Color color)
    {
        GameObject obj = new GameObject(
            objName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(AspectRatioFitter)
        );

        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.GetComponent<RectTransform>();

        float half = widthPercent / 2f;

        rect.anchorMin = new Vector2(0.5f - half, centerY);
        rect.anchorMax = new Vector2(0.5f + half, centerY);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        AspectRatioFitter fitter = obj.GetComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
        fitter.aspectRatio = 1f;

        Image image = obj.GetComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.preserveAspect = true;
        image.raycastTarget = false;
    }

    // =================================================
    // REMOVE IMAGE BACKGROUND
    // =================================================

    private Sprite CreateTransparentSprite(Sprite source)
    {
        if (source == null || source.texture == null)
            return source;

        try
        {
            Texture2D sourceTexture = source.texture;
            Rect r = source.textureRect;

            int x = Mathf.RoundToInt(r.x);
            int y = Mathf.RoundToInt(r.y);
            int width = Mathf.RoundToInt(r.width);
            int height = Mathf.RoundToInt(r.height);

            Color[] sourcePixels = sourceTexture.GetPixels(x, y, width, height);
            Color32[] pixels = new Color32[sourcePixels.Length];

            for (int i = 0; i < sourcePixels.Length; i++)
                pixels[i] = sourcePixels[i];

            Color32 key = GetCornerBackgroundColor(pixels, width, height);
            float smooth = Mathf.Max(0.001f, edgeSmoothness);

            for (int i = 0; i < pixels.Length; i++)
            {
                Color32 p = pixels[i];

                float dr = (p.r - key.r) / 255f;
                float dg = (p.g - key.g) / 255f;
                float db = (p.b - key.b) / 255f;

                float distance = Mathf.Sqrt(dr * dr + dg * dg + db * db);

                float alpha = Mathf.InverseLerp(
                    backgroundTolerance,
                    backgroundTolerance + smooth,
                    distance
                );

                p.a = (byte)Mathf.RoundToInt(p.a * alpha);
                pixels[i] = p;
            }

            transparentBasketballTexture = new Texture2D(
                width, height, TextureFormat.RGBA32, false);

            transparentBasketballTexture.SetPixels32(pixels);
            transparentBasketballTexture.Apply();

            transparentBasketballSprite = Sprite.Create(
                transparentBasketballTexture,
                new Rect(0, 0, width, height),
                new Vector2(0.5f, 0.5f),
                source.pixelsPerUnit,
                0,
                SpriteMeshType.FullRect
            );

            return transparentBasketballSprite;
        }
        catch (UnityException e)
        {
            Debug.LogWarning(
                "Background logo tidak dapat diproses. " +
                "Aktifkan Read/Write pada import gambar. " + e.Message);

            return source;
        }
    }

    private Color32 GetCornerBackgroundColor(Color32[] p, int w, int h)
    {
        Color32 a = p[0];
        Color32 b = p[w - 1];
        Color32 c = p[(h - 1) * w];
        Color32 d = p[h * w - 1];

        return new Color32(
            (byte)((a.r + b.r + c.r + d.r) / 4),
            (byte)((a.g + b.g + c.g + d.g) / 4),
            (byte)((a.b + b.b + c.b + d.b) / 4),
            255
        );
    }

    // =================================================
    // HOME BUTTON
    // =================================================

    private void BuildHomeButton()
    {
        if (homeButtonObject != null)
            Destroy(homeButtonObject);

        homeButtonObject = CreateButtonObject(
            targetCanvas.transform,
            "HOME",
            0.36f, 0.025f, 0.64f, 0.085f,
            darkColor, Color.white, ReturnHome
        );

        homeButtonObject.SetActive(false);
    }

    public void ReturnHome()
    {
        if (gameManager != null)
            gameManager.PauseGame();

        if (tutorialManager != null && tutorialManager.ballController != null)
            tutorialManager.ballController.SetCanThrowBall(false);

        ShowMainMenu();
    }

    // =================================================
    // BUTTON ACTIONS
    // =================================================

    public void PlayGame()
    {
        if (tutorialManager == null)
            tutorialManager = FindAnyObjectByType<TutorialManager>();

        if (tutorialManager == null)
        {
            Debug.LogError("TutorialManager tidak ditemukan.");
            return;
        }

        tutorialManager.mainMenuUI = this;

        // Langsung main: timer, skor, HOME muncul, bola bisa dilempar
        tutorialManager.StartGameplay();
    }

    public void OpenTutorial()
    {
        if (tutorialManager == null)
            tutorialManager = FindAnyObjectByType<TutorialManager>();

        if (tutorialManager == null)
        {
            Debug.LogError("TutorialManager tidak ditemukan.");
            return;
        }

        tutorialManager.mainMenuUI = this;

        if (!tutorialManager.OpenTutorial())
        {
            Debug.LogError("Tutorial gagal dibuka. Periksa referensi TutorialManager.");
            ShowMainMenu();
        }
    }

    // =================================================
    // SHOW SCREENS
    // =================================================

    public void ShowTutorialPanel()
    {
        ShowOnly(tutorial: tutorialPanel);
    }

    public void ShowGameplayHUD()
    {
        ShowOnly(gameplay: true);
        Debug.Log("Gameplay HUD ditampilkan.");
    }

    public void ResumeGameplay()
    {
        if (gameManager != null)
            gameManager.ResumeGame();

        if (tutorialManager != null && tutorialManager.ballController != null)
            tutorialManager.ballController.SetCanThrowBall(true);

        ShowGameplayHUD();
    }

    public void ShowMainMenu()
    {
        ShowOnly(main: mainMenuRoot);
    }

    public void ShowGameOverPanel()
    {
        ShowOnly(gameOver: gameOverPanel);
    }

    // =================================================
    // LEADERBOARD
    // =================================================

    public void OpenLeaderboard()
    {
        openedFromGameOver = false;
        ShowOnly(leaderboard: leaderboardPanel);

        if (leaderboardManager == null)
            leaderboardManager = FindAnyObjectByType<LeaderboardManager>();

        if (leaderboardManager != null)
            leaderboardManager.RefreshLeaderboard();
        else
            Debug.LogError("LeaderboardManager tidak ditemukan.");
    }

    public void OpenLeaderboardAfterGameOver()
    {
        openedFromGameOver = true;
        ShowOnly(leaderboard: leaderboardPanel);

        if (leaderboardManager != null)
            leaderboardManager.RefreshLeaderboard();
    }

    public void BackFromLeaderboard()
    {
        if (openedFromGameOver)
            ShowGameOverPanel();
        else
            ShowMainMenu();
    }

    // =================================================
    // UI HELPERS
    // =================================================

    private void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private void SetBox(RectTransform rect, float x0, float y0, float x1, float y1)
    {
        rect.anchorMin = new Vector2(x0, y0);
        rect.anchorMax = new Vector2(x1, y1);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private TextMeshProUGUI CreateText(
        Transform parent,
        string value,
        float x0, float y0, float x1, float y1,
        Color color,
        float maxFontSize,
        FontStyles style)
    {
        GameObject obj = new GameObject(
            "Text_" + value.Replace(" ", ""),
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI)
        );

        obj.transform.SetParent(parent, false);
        SetBox(obj.GetComponent<RectTransform>(), x0, y0, x1, y1);

        TextMeshProUGUI text = obj.GetComponent<TextMeshProUGUI>();
        text.text = value;
        text.color = color;
        text.fontStyle = style;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.enableAutoSizing = true;
        text.fontSizeMin = 8;
        text.fontSizeMax = maxFontSize;
        text.raycastTarget = false;

        return text;
    }

    private void CreateButton(
        Transform parent, string label,
        float x0, float y0, float x1, float y1,
        Color bg, Color fg, UnityAction action)
    {
        CreateButtonObject(parent, label, x0, y0, x1, y1, bg, fg, action);
    }

    private GameObject CreateButtonObject(
        Transform parent, string label,
        float x0, float y0, float x1, float y1,
        Color bg, Color fg, UnityAction action)
    {
        GameObject obj = new GameObject(
            "Button_" + label,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button)
        );

        obj.transform.SetParent(parent, false);
        SetBox(obj.GetComponent<RectTransform>(), x0, y0, x1, y1);

        Image image = obj.GetComponent<Image>();
        image.sprite = GetRoundedSprite();
        image.type = Image.Type.Sliced;
        image.color = bg;
        image.raycastTarget = true;

        Button button = obj.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);

        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 1f, 1f, 0.92f);
        colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
        button.colors = colors;

        TextMeshProUGUI text = CreateText(
            obj.transform, label, 0.05f, 0.1f, 0.95f, 0.9f,
            fg, 32, FontStyles.Bold);

        text.name = "Label";

        return obj;
    }

    private Sprite GetCircleSprite()
    {
        if (circleSprite != null)
            return circleSprite;

        const int size = 128;

        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - size / 2f;
                float dy = y - size / 2f;

                float alpha =
                    Mathf.Sqrt(dx * dx + dy * dy) <= size / 2f ? 1f : 0f;

                texture.SetPixel(x, y, new Color(1, 1, 1, alpha));
            }
        }

        texture.Apply();

        circleSprite = Sprite.Create(
            texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));

        return circleSprite;
    }

    public Sprite GetRoundedSprite()
    {
        if (roundedSprite != null)
            return roundedSprite;

        const int size = 128;
        const float radius = 18f;

        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float px = Mathf.Clamp(x, radius, size - radius);
                float py = Mathf.Clamp(y, radius, size - radius);

                float dx = x - px;
                float dy = y - py;

                float alpha =
                    Mathf.Sqrt(dx * dx + dy * dy) <= radius ? 1f : 0f;

                texture.SetPixel(x, y, new Color(1, 1, 1, alpha));
            }
        }

        texture.Apply();

        roundedSprite = Sprite.Create(
            texture,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f),
            100,
            0,
            SpriteMeshType.FullRect,
            new Vector4(40, 40, 40, 40)
        );

        return roundedSprite;
    }

    private void OnDestroy()
    {
        if (roundedSprite != null) Destroy(roundedSprite);
        if (circleSprite != null) Destroy(circleSprite);
        if (transparentBasketballSprite != null) Destroy(transparentBasketballSprite);
        if (transparentBasketballTexture != null) Destroy(transparentBasketballTexture);
    }
}