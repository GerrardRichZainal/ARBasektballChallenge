
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform content;
    public GameObject leaderboardRowPrefab;
    public TMP_Text emptyMessageText;

    [Header("Game Manager")]
    public GameManager gameManager;

    [Header("Settings")]
    public int maxPlayers = 10;

    private bool isLoading;

    private void Start()
    {
        if (gameManager == null)
            gameManager = FindAnyObjectByType<GameManager>();
        ApplyStyle();
        ShowEmptyMessage("Leaderboard akan dimuat...");
        
        Debug.Log("LeaderboardManager siap.");
    }

    // =========================================
    // SUBMIT SCORE
    // =========================================

    public void AddPlayer(string playerName, int playerScore)
    {
        if (PlayFabManager.Instance == null)
        {
            HandleSubmitFailure("PlayFabManager tidak ditemukan.");
            return;
        }

        Debug.Log("Mengirim skor ke PlayFab...");

        PlayFabManager.Instance.SubmitScore(
            playerName,
            playerScore,
            HandleSubmitSuccess,
            HandleSubmitFailure
        );
    }

    private void HandleSubmitSuccess()
    {
        Debug.Log("SKOR BERHASIL DIKIRIM KE PLAYFAB!");

        if (gameManager != null)
        {
            gameManager.OnScoreSubmittedSuccessfully();
        }
        else
        {
            Debug.LogWarning(
                "GameManager tidak ditemukan. Memuat leaderboard langsung."
            );

            RefreshLeaderboard();
        }
    }

    private void HandleSubmitFailure(string error)
    {
        Debug.LogError("SUBMIT SKOR GAGAL: " + error);

        if (gameManager != null)
            gameManager.OnScoreSubmissionFailed();
    }

    // =========================================
    // REFRESH LEADERBOARD
    // =========================================

    public void RefreshLeaderboard()
    {
        Debug.Log("=== REFRESH LEADERBOARD ===");

        if (isLoading)
        {
            Debug.Log("Leaderboard masih dalam proses loading.");
            return;
        }

        if (PlayFabManager.Instance == null)
        {
            ShowEmptyMessage("PlayFab tidak tersedia.");
            return;
        }

        if (!PlayFabManager.Instance.IsReady)
        {
            ShowEmptyMessage("Menghubungkan ke PlayFab...");
            return;
        }

        isLoading = true;

        ShowEmptyMessage("Memuat leaderboard...");

        Debug.Log("Mengambil leaderboard dari PlayFab...");

        PlayFabManager.Instance.GetOnlineLeaderboard(
            DisplayOnlineLeaderboard,
            HandleLeaderboardFailure
        );
    }

    public void DisplayLeaderboard()
    {
        RefreshLeaderboard();
    }

    private bool styled;

    private static readonly Color Orange = new Color32(239, 112, 35, 255);
    private static readonly Color Dark = new Color32(55, 45, 38, 255);
    private static readonly Color Cream = new Color32(255, 248, 238, 255);

    private void ApplyStyle()
    {
        if (styled || content == null)
            return;

        ScrollRect scroll = content.GetComponentInParent<ScrollRect>(true);

        GameObject panel = gameManager != null
            ? gameManager.leaderboardPanel
            : null;

        if (panel == null && scroll != null)
            panel = scroll.transform.parent.gameObject;

        if (panel == null || scroll == null)
            return;

        styled = true;

        MainMenuUI menu = FindAnyObjectByType<MainMenuUI>();
        Sprite rounded = menu != null ? menu.GetRoundedSprite() : null;

        // Panel penuh layar, latar gelap
        Stretch(panel.GetComponent<RectTransform>());

        Image panelImage = panel.GetComponent<Image>();

        if (panelImage == null)
            panelImage = panel.AddComponent<Image>();

        panelImage.sprite = null;
        panelImage.type = Image.Type.Simple;
        panelImage.color = new Color(0f, 0f, 0f, 0.7f);
        panelImage.raycastTarget = true;

        // Kartu krem
        GameObject card = new GameObject(
            "LeaderboardCard",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );

        card.transform.SetParent(panel.transform, false);
        card.transform.SetAsFirstSibling();
        SetBox(card.GetComponent<RectTransform>(), 0.06f, 0.08f, 0.94f, 0.92f);

        Image cardImage = card.GetComponent<Image>();
        cardImage.sprite = rounded;
        cardImage.type = rounded != null ? Image.Type.Sliced : Image.Type.Simple;
        cardImage.color = Cream;
        cardImage.raycastTarget = false;

        // Judul
        Transform titleTransform = panel.transform.Find("LeaderboardTitle");

        if (titleTransform != null)
        {
            TMP_Text title = titleTransform.GetComponent<TMP_Text>();

            SetBox(titleTransform.GetComponent<RectTransform>(),
                0.10f, 0.80f, 0.90f, 0.89f);

            if (title != null)
            {
                title.text = "LEADERBOARD";
                title.color = Orange;
                title.fontStyle = FontStyles.Bold;
                title.alignment = TextAlignmentOptions.Center;
                title.enableAutoSizing = true;
                title.fontSizeMin = 20;
                title.fontSizeMax = 54;
                title.raycastTarget = false;
            }
        }

        // Daftar skor
        SetBox(scroll.GetComponent<RectTransform>(), 0.10f, 0.20f, 0.90f, 0.78f);

        Image scrollImage = scroll.GetComponent<Image>();

        if (scrollImage != null)
        {
            scrollImage.sprite = null;
            scrollImage.color = new Color(0f, 0f, 0f, 0.04f);
        }

        if (scroll.viewport != null)
            Stretch(scroll.viewport);

        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;

        if (scroll.horizontalScrollbar != null)
        {
            scroll.horizontalScrollbar.gameObject.SetActive(false);
            scroll.horizontalScrollbar = null;
        }

        if (scroll.verticalScrollbar != null)
        {
            scroll.verticalScrollbar.gameObject.SetActive(false);
            scroll.verticalScrollbar = null;
        }

        // Content: baris tersusun otomatis dari atas ke bawah
        RectTransform contentRect = content.GetComponent<RectTransform>();

        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.offsetMin = new Vector2(0f, contentRect.offsetMin.y);
        contentRect.offsetMax = new Vector2(0f, 0f);

        VerticalLayoutGroup layout =
            content.GetComponent<VerticalLayoutGroup>();

        if (layout == null)
            layout = content.gameObject.AddComponent<VerticalLayoutGroup>();

        layout.spacing = 10;
        layout.padding = new RectOffset(8, 8, 8, 8);
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter =
            content.GetComponent<ContentSizeFitter>();

        if (fitter == null)
            fitter = content.gameObject.AddComponent<ContentSizeFitter>();

        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Tombol tutup
        Transform closeTransform =
            panel.transform.Find("CloseLeaderboardButton");

        if (closeTransform != null)
        {
            SetBox(closeTransform.GetComponent<RectTransform>(),
                0.25f, 0.10f, 0.75f, 0.17f);

            Image closeImage = closeTransform.GetComponent<Image>();

            if (closeImage != null)
            {
                closeImage.sprite = rounded;
                closeImage.type = rounded != null
                    ? Image.Type.Sliced
                    : Image.Type.Simple;
                closeImage.color = Orange;
            }

            Button closeButton = closeTransform.GetComponent<Button>();

            if (closeButton != null)
            {
                ColorBlock colors = closeButton.colors;
                colors.normalColor = Color.white;
                colors.highlightedColor = new Color(1f, 1f, 1f, 0.92f);
                colors.pressedColor = new Color(0.82f, 0.82f, 0.82f, 1f);
                closeButton.colors = colors;
            }

            TMP_Text closeLabel =
                closeTransform.GetComponentInChildren<TMP_Text>(true);

            if (closeLabel != null)
            {
                Stretch(closeLabel.rectTransform);
                closeLabel.text = "TUTUP";
                closeLabel.color = Color.white;
                closeLabel.fontStyle = FontStyles.Bold;
                closeLabel.alignment = TextAlignmentOptions.Center;
                closeLabel.enableAutoSizing = true;
                closeLabel.fontSizeMin = 14;
                closeLabel.fontSizeMax = 34;
                closeLabel.raycastTarget = false;
            }
        }

        // Teks "memuat..." (dibuat otomatis kalau slot kosong)
        if (emptyMessageText == null)
        {
            GameObject msg = new GameObject(
                "EmptyMessage",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(TextMeshProUGUI)
            );

            msg.transform.SetParent(panel.transform, false);
            SetBox(msg.GetComponent<RectTransform>(), 0.12f, 0.45f, 0.88f, 0.55f);

            TextMeshProUGUI text = msg.GetComponent<TextMeshProUGUI>();
            text.color = Dark;
            text.alignment = TextAlignmentOptions.Center;
            text.enableAutoSizing = true;
            text.fontSizeMin = 14;
            text.fontSizeMax = 32;
            text.raycastTarget = false;

            emptyMessageText = text;
        }
    }

    private void StyleRow(GameObject row, int index)
    {
        LayoutElement element = row.GetComponent<LayoutElement>();

        if (element == null)
            element = row.AddComponent<LayoutElement>();

        element.minHeight = 90f;
        element.preferredHeight = 90f;

        Image bg = row.GetComponent<Image>();

        if (bg == null)
            bg = row.AddComponent<Image>();

        bg.sprite = null;
        bg.raycastTarget = false;

        // Juara 1 emas, 2 perak, 3 perunggu, sisanya putih
        if (index == 0) bg.color = new Color32(255, 226, 150, 255);
        else if (index == 1) bg.color = new Color32(228, 228, 228, 255);
        else if (index == 2) bg.color = new Color32(240, 200, 160, 255);
        else bg.color = Color.white;

        foreach (TMP_Text text in row.GetComponentsInChildren<TMP_Text>(true))
        {
            text.enableAutoSizing = true;
            text.fontSizeMin = 14;
            text.fontSizeMax = 34;
            text.raycastTarget = false;

            if (text.name == "RankText")
            {
                text.color = Orange;
                text.fontStyle = FontStyles.Bold;
            }
            else
            {
                text.color = Dark;
                text.fontStyle = text.name == "PlayerScoreText"
                    ? FontStyles.Bold
                    : FontStyles.Normal;
            }
        }
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
    // =========================================
    // DISPLAY ONLINE LEADERBOARD
    // =========================================

    private void DisplayOnlineLeaderboard(
        List<PlayFabManager.OnlinePlayerData> players)
    {
        isLoading = false;

        Debug.Log("=== CALLBACK LEADERBOARD DITERIMA ===");

        Debug.Log(
            "Jumlah pemain: " +
            (players == null ? "NULL" : players.Count.ToString())
        );

        if (content == null)
        {
            Debug.LogError("Content belum dihubungkan!");
            ShowEmptyMessage("Content leaderboard belum diatur.");
            return;
        }

        if (leaderboardRowPrefab == null)
        {
            Debug.LogError("Leaderboard Row Prefab belum dihubungkan!");
            ShowEmptyMessage("Prefab leaderboard belum diatur.");
            return;
        }

        ClearRows();

        if (players == null || players.Count == 0)
        {
            ShowEmptyMessage(
                "Belum ada skor. Jadilah pemain pertama!"
            );

            return;
        }

        int total = Mathf.Min(players.Count, maxPlayers);

        if (emptyMessageText != null)
            emptyMessageText.gameObject.SetActive(false);

        for (int i = 0; i < total; i++)
        {
            PlayFabManager.OnlinePlayerData player = players[i];

            GameObject row = Instantiate(
                leaderboardRowPrefab,
                content
            );

            row.SetActive(true);
            StyleRow(row, i);

            TMP_Text[] textComponents =
                row.GetComponentsInChildren<TMP_Text>(true);

            bool rankFound = false;
            bool nameFound = false;
            bool scoreFound = false;

            foreach (TMP_Text textComponent in textComponents)
            {
                switch (textComponent.name)
                {
                    case "RankText":
                        textComponent.text =
                            "#" + (player.rank > 0 ? player.rank : i + 1);
                        rankFound = true;
                        break;

                    case "PlayerNameText":
                        textComponent.text = player.playerName;
                        nameFound = true;
                        break;

                    case "PlayerScoreText":
                        textComponent.text = player.score + " PTS";
                        scoreFound = true;
                        break;
                }
            }

            if (!rankFound || !nameFound || !scoreFound)
            {
                Debug.LogWarning(
                    "Periksa nama TMP pada prefab leaderboard. " +
                    "Rank: " + rankFound +
                    ", Name: " + nameFound +
                    ", Score: " + scoreFound
                );
            }

            Debug.Log(
                "Pemain ditampilkan: " +
                player.playerName +
                " | Skor: " + player.score
            );
        }

        Canvas.ForceUpdateCanvases();

        RectTransform rect =
            content.GetComponent<RectTransform>();

        if (rect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        }

        Debug.Log(
            "LEADERBOARD DITAMPILKAN: " + total + " pemain."
        );
    }

    // =========================================
    // LEADERBOARD FAILURE
    // =========================================

    private void HandleLeaderboardFailure(string error)
    {
        isLoading = false;

        Debug.LogError(
            "GAGAL MEMUAT LEADERBOARD: " + error
        );

        ShowEmptyMessage(
            "Gagal memuat leaderboard. Coba refresh."
        );
    }

    // =========================================
    // CLEAR ROWS
    // =========================================

    private void ClearRows()
    {
        if (content == null)
            return;

        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
    }

    // =========================================
    // EMPTY MESSAGE
    // =========================================

    private void ShowEmptyMessage(string message)
    {
        if (emptyMessageText == null)
            return;

        emptyMessageText.text = message;
        emptyMessageText.gameObject.SetActive(true);
    }
}
