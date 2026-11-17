
using UnityEngine;
using UnityEngine.Networking;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.AuthenticationModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class PlayFabManager : MonoBehaviour
{
    private const string CUSTOM_ID_KEY = "ARBasketball_CustomID";
    private const string STATISTIC_NAME = "BasketballScore";
    private const string LEADERBOARD_NAME = "ARBasketBallLeaderboard";

    public static PlayFabManager Instance { get; private set; }

    public bool IsLoggedIn { get; private set; }
    public bool IsReady { get; private set; }

    private string customID;
    private string entityToken;
    private string entityID;
    private string entityType;

    private bool isSubmitting;

    // =========================================
    // DATA LEADERBOARD
    // =========================================

    [Serializable]
    public class OnlinePlayerData
    {
        public string playerName;
        public int score;
        public int rank;
    }

    [Serializable]
    private class StatisticUpdateData
    {
        public string Name;
        public string[] Scores;
    }

    [Serializable]
    private class UpdateStatisticsRequestData
    {
        public EntityData Entity;
        public StatisticUpdateData[] Statistics;
    }

    [Serializable]
    private class EntityData
    {
        public string Id;
        public string Type;
    }

    [Serializable]
    private class LeaderboardRequestData
    {
        public string LeaderboardName;
        public int StartingPosition;
        public int PageSize;
    }

    [Serializable]
    private class LeaderboardEntryData
    {
        public int Rank;
        public string DisplayName;
        public string[] Scores;
    }

    [Serializable]
    private class LeaderboardResponseData
    {
        public int Version;
        public LeaderboardEntryData[] Rankings;
    }

    [Serializable]
    private class LeaderboardResponseWrapper
    {
        public int code;
        public string status;
        public LeaderboardResponseData data;
    }

    // =========================================
    // INITIALIZATION
    // =========================================

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Login();
    }

    // =========================================
    // LOGIN
    // =========================================

    public void Login()
    {
        IsLoggedIn = false;
        IsReady = false;

        if (PlayerPrefs.HasKey(CUSTOM_ID_KEY))
        {
            customID = PlayerPrefs.GetString(CUSTOM_ID_KEY);
        }
        else
        {
            customID = Guid.NewGuid().ToString("N");

            PlayerPrefs.SetString(CUSTOM_ID_KEY, customID);
            PlayerPrefs.Save();
        }

        Debug.Log("Memulai login PlayFab...");

        LoginWithCustomIDRequest request =
            new LoginWithCustomIDRequest
            {
                CustomId = customID,
                CreateAccount = true
            };

        PlayFabClientAPI.LoginWithCustomID(
            request,
            OnLoginSuccess,
            OnLoginFailure
        );
    }

    private void OnLoginSuccess(LoginResult result)
    {
        IsLoggedIn = true;

        Debug.Log("================================");
        Debug.Log("PLAYFAB LOGIN BERHASIL");
        Debug.Log("PlayFab ID: " + result.PlayFabId);
        Debug.Log("================================");

        GetEntityToken();
    }

    private void OnLoginFailure(PlayFabError error)
    {
        IsLoggedIn = false;
        IsReady = false;

        Debug.LogError(
            "PLAYFAB LOGIN GAGAL: " +
            error.GenerateErrorReport()
        );
    }

    // =========================================
    // ENTITY TOKEN
    // =========================================

    private void GetEntityToken()
    {
        PlayFabAuthenticationAPI.GetEntityToken(
            new GetEntityTokenRequest(),
            OnEntityTokenSuccess,
            OnEntityTokenFailure
        );
    }

    private void OnEntityTokenSuccess(
        GetEntityTokenResponse result)
    {
        if (result == null ||
            result.Entity == null ||
            string.IsNullOrEmpty(result.EntityToken))
        {
            Debug.LogError("Entity Token tidak valid!");
            return;
        }

        entityToken = result.EntityToken;
        entityID = result.Entity.Id;
        entityType = result.Entity.Type;

        IsReady = true;

        Debug.Log("================================");
        Debug.Log("PLAYFAB SIAP DIGUNAKAN!");
        Debug.Log("Entity ID: " + entityID);
        Debug.Log("Entity Type: " + entityType);
        Debug.Log("================================");

        GetOnlineLeaderboard(null);
    }

    private void OnEntityTokenFailure(PlayFabError error)
    {
        IsReady = false;

        Debug.LogError(
            "GAGAL MENDAPATKAN ENTITY TOKEN: " +
            error.GenerateErrorReport()
        );
    }

    // =========================================
    // SUBMIT SCORE
    // =========================================

    public void SubmitScore(
        string playerName,
        int score,
        Action onSuccess,
        Action<string> onFailure)
    {
        if (isSubmitting)
        {
            onFailure?.Invoke("Pengiriman skor sedang berlangsung.");
            return;
        }

        if (!IsReady || string.IsNullOrEmpty(entityToken))
        {
            onFailure?.Invoke("PlayFab belum siap.");
            return;
        }

        if (score < 0)
        {
            onFailure?.Invoke("Skor tidak valid.");
            return;
        }

        if (string.IsNullOrWhiteSpace(playerName))
            playerName = "Player";

        playerName = playerName.Trim();

        isSubmitting = true;

        Debug.Log("================================");
        Debug.Log("MENGIRIM SKOR KE PLAYFAB");
        Debug.Log("Nama: " + playerName);
        Debug.Log("Skor: " + score);
        Debug.Log("================================");

        UpdateDisplayName(
            playerName,
            score,
            onSuccess,
            onFailure
        );
    }

    // =========================================
    // UPDATE DISPLAY NAME
    // =========================================

    private void UpdateDisplayName(
        string playerName,
        int score,
        Action onSuccess,
        Action<string> onFailure)
    {
        UpdateUserTitleDisplayNameRequest request =
            new UpdateUserTitleDisplayNameRequest
            {
                DisplayName = playerName
            };

        PlayFabClientAPI.UpdateUserTitleDisplayName(
            request,

            result =>
            {
                Debug.Log(
                    "Nama pemain diperbarui: " +
                    result.DisplayName
                );

                StartCoroutine(
                    UpdateStatisticCoroutine(
                        score,
                        onSuccess,
                        onFailure
                    )
                );
            },

            error =>
            {
                Debug.LogWarning(
                    "Nama gagal diperbarui, tetap mencoba mengirim skor: " +
                    error.ErrorMessage
                );

                StartCoroutine(
                    UpdateStatisticCoroutine(
                        score,
                        onSuccess,
                        onFailure
                    )
                );
            }
        );
    }

    // =========================================
    // UPDATE STATISTIC VIA REST API
    // =========================================

    private IEnumerator UpdateStatisticCoroutine(
        int score,
        Action onSuccess,
        Action<string> onFailure)
    {
        string url =
            "https://" + PlayFabSettings.TitleId +
            ".playfabapi.com/Statistic/UpdateStatistics";

        UpdateStatisticsRequestData body =
            new UpdateStatisticsRequestData
            {
                Entity = new EntityData
                {
                    Id = entityID,
                    Type = entityType
                },

                Statistics = new StatisticUpdateData[]
                {
                    new StatisticUpdateData
                    {
                        Name = STATISTIC_NAME,
                        Scores = new string[]
                        {
                            score.ToString()
                        }
                    }
                }
            };

        string json = JsonUtility.ToJson(body);

        using (UnityWebRequest request =
            new UnityWebRequest(url, "POST"))
        {
            byte[] data = Encoding.UTF8.GetBytes(json);

            request.uploadHandler = new UploadHandlerRaw(data);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader(
                "Content-Type",
                "application/json"
            );

            request.SetRequestHeader(
                "X-EntityToken",
                entityToken
            );

            Debug.Log("Mengirim UpdateStatistics...");
            Debug.Log("Request: " + json);

            yield return request.SendWebRequest();

            string response =
                request.downloadHandler.text;

            if (request.result != UnityWebRequest.Result.Success)
            {
                isSubmitting = false;

                string error =
                    "UpdateStatistics gagal. HTTP: " +
                    request.responseCode +
                    " | " + request.error +
                    " | " + response;

                Debug.LogError(error);

                onFailure?.Invoke(error);
                yield break;
            }

            Debug.Log("================================");
            Debug.Log("STATISTIC BERHASIL DIPERBARUI!");
            Debug.Log("HTTP: " + request.responseCode);
            Debug.Log("Respons: " + response);
            Debug.Log("================================");

            isSubmitting = false;

            onSuccess?.Invoke();
        }
    }

    // =========================================
    // GET ONLINE LEADERBOARD
    // =========================================

    public void GetOnlineLeaderboard(
        Action<List<OnlinePlayerData>> onSuccess,
        Action<string> onFailure = null)
    {
        if (!IsReady || string.IsNullOrEmpty(entityToken))
        {
            onFailure?.Invoke("PlayFab belum siap.");
            return;
        }

        StartCoroutine(
            GetLeaderboardCoroutine(onSuccess, onFailure)
        );
    }

    private IEnumerator GetLeaderboardCoroutine(
        Action<List<OnlinePlayerData>> onSuccess,
        Action<string> onFailure)
    {
        string url =
            "https://" + PlayFabSettings.TitleId +
            ".playfabapi.com/Leaderboard/GetLeaderboard";

        LeaderboardRequestData body =
            new LeaderboardRequestData
            {
                LeaderboardName = LEADERBOARD_NAME,
                StartingPosition = 1,
                PageSize = 10
            };

        string json = JsonUtility.ToJson(body);

        using (UnityWebRequest request =
            new UnityWebRequest(url, "POST"))
        {
            byte[] data = Encoding.UTF8.GetBytes(json);

            request.uploadHandler = new UploadHandlerRaw(data);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader(
                "Content-Type",
                "application/json"
            );

            request.SetRequestHeader(
                "X-EntityToken",
                entityToken
            );

            Debug.Log("Mengambil leaderboard dari PlayFab...");

            yield return request.SendWebRequest();

            string response =
                request.downloadHandler.text;

            if (request.result != UnityWebRequest.Result.Success)
            {
                string error =
                    "GetLeaderboard gagal. HTTP: " +
                    request.responseCode +
                    " | " + request.error +
                    " | " + response;

                Debug.LogError(error);
                onFailure?.Invoke(error);
                yield break;
            }

            Debug.Log("Respons leaderboard: " + response);

            LeaderboardResponseWrapper result =
                JsonUtility.FromJson<LeaderboardResponseWrapper>(
                    response
                );

            if (result == null ||
                result.data == null)
            {
                string error =
                    "Respons leaderboard tidak dapat dibaca: " +
                    response;

                Debug.LogError(error);
                onFailure?.Invoke(error);
                yield break;
            }

            List<OnlinePlayerData> players =
                new List<OnlinePlayerData>();

            if (result.data.Rankings != null)
            {
                foreach (LeaderboardEntryData entry
                    in result.data.Rankings)
                {
                    int playerScore = 0;

                    if (entry.Scores != null &&
                        entry.Scores.Length > 0)
                    {
                        int.TryParse(
                            entry.Scores[0],
                            out playerScore
                        );
                    }

                    players.Add(new OnlinePlayerData
                    {
                        playerName =
                            string.IsNullOrEmpty(entry.DisplayName)
                            ? "Player"
                            : entry.DisplayName,

                        score = playerScore,
                        rank = entry.Rank
                    });
                }
            }

            Debug.Log(
                "Leaderboard berhasil diambil. Jumlah pemain: " +
                players.Count
            );

            onSuccess?.Invoke(players);
        }
    }
}
