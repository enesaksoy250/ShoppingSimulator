using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Extensions;
using Firebase.Database;
using TMPro;
using System;
using System.Linq;
using UnityEngine.UI;
using CryingSnow.CheckoutFrenzy;
using System.Data;
using Unity.VisualScripting;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Globalization;


public class SerializableDictionary
{
    public string googleName;
    public string nickname;
    public string date;
    public int gameTime;
    public int level;
    public int watchedAdCount;
    public int adLoadingErrorCount;
    public bool removeAd;

    public SerializableDictionary(Dictionary<string, object> data)
    {
        googleName = data.ContainsKey("googleName") ? data["googleName"].ToString() : "null";
        nickname = data.ContainsKey("nickname") ? data["nickname"].ToString() : "null";
        date = data.ContainsKey("date") ? data["date"].ToString() : "null";
        gameTime = data.ContainsKey("gameTime") ? System.Convert.ToInt32(data["gameTime"]) : 0;
        level = data.ContainsKey("level") ? System.Convert.ToInt32(data["level"]) : 1;
        watchedAdCount = data.ContainsKey("watchedAdCount") ? System.Convert.ToInt32(data["watchedAdCount"]) : 0;
        adLoadingErrorCount = data.ContainsKey("adLoadingErrorCount") ? System.Convert.ToInt32(data["adLoadingErrorCount"]) : 0;
        removeAd = data.ContainsKey("removeAd") ? System.Convert.ToBoolean(data["removeAd"]) : false;
    }
}

public class DatabaseManager : MonoBehaviour
{

    public static DatabaseManager Instance;

    DatabaseReference databaseReference;

    private string userId;
    private string registerType;
    public string UserId => userId;

    public bool firebaseInitialized { get; private set; }

    //[SerializeField] TMP_InputField usernameInput;
    //[SerializeField] TMP_InputField storeNameInput;

    public event Action<GameData> OnGameDataLoaded;
    public event Action OnLoadFailed;

    

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


    void Start()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                print("Firebase'e bağlandı!");
                databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
                firebaseInitialized = true;

              
                if (!PlayerPrefs.HasKey("Login"))
                {
                    StartCoroutine(SaveUser());
                }
                  
                // DatabaseStatistic databaseStatistic = new DatabaseStatistic(databaseReference, userId);
            }
            else
            {
                firebaseInitialized = false;
                UnityEngine.Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
            }
        });
    }



    IEnumerator SaveUser()
    {
        while (true)
        {
            if(LoginWithGoogle.instance != null && LoginWithGoogle.instance.authInitialized)
            {
                    if (!PlayerPrefs.HasKey("Login"))
                    {
                        userId = SystemInfo.deviceUniqueIdentifier;
                        registerType = "users";
                    }

                    Save();

                    yield break;
                
            }

            yield return new WaitForSeconds(1);

        }
    }

    public void InitializeDatabase(string uid = null, string registerType = null)
    {
        if (string.IsNullOrEmpty(uid))
        {
            userId = SystemInfo.deviceUniqueIdentifier;
            this.registerType = "users";
        }

        else
        {
            userId = uid;
            this.registerType = registerType;
        }
    }


    private void Save2()
    {

        int userNumber = UnityEngine.Random.Range(0, 1000000);
        string nickname = "guest" + userNumber;
        string storeName = "SHOP MASTER";
      
        string date = DateTime.Now.ToString("dd-MM-yyyy");
        PlayerPrefs.SetString("StoreName", storeName.ToUpper());
        User user = new User(nickname, 0, 0, date, false, 0, 1);
        string json = JsonUtility.ToJson(user);

        databaseReference.Child(registerType).Child(userId).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
        {

            if (task.IsCompleted)
            {
                print("Kayıt başarılı");
                PlayerPrefs.SetInt("Login", 1);
                PlayerPrefs.SetString("UserId", userId);
                SetLeaderboard(nickname,1);
                PanelManager.instance.ChangePanelVisibility("WaitingPanel", false);
                PanelManager.instance.ChangePanelVisibility("SavePanel", false);
            }

            else
            {
                print("Kayıt başarısız!");
                PanelManager.instance.ChangePanelVisibility("WaitingPanel", false);
                PanelManager.instance.ChangePanelVisibility("ErrorPanel", true);
            }

        });


    }

    private void Save()
    {
        if (registerType == "google")
            return;

        int userNumber = UnityEngine.Random.Range(0, 1000000);
        string nickname = "guest" + userNumber;
        string storeName = "SHOP MASTER";

        string date = DateTime.Now.ToString("dd-MM-yyyy");
        PlayerPrefs.SetString("StoreName", storeName.ToUpper());
        User user = new User(nickname, 0, 0, date, false, 0, 1);
        string json = JsonUtility.ToJson(user);

        databaseReference.Child(registerType).Child(userId).GetValueAsync().ContinueWithOnMainThread(task =>
        {

            if (task.IsCompleted)
            {

                DataSnapshot dataSnapshot = task.Result;

                if (!dataSnapshot.Exists)
                {
                    databaseReference.Child(registerType).Child(userId).SetRawJsonValueAsync(json).ContinueWithOnMainThread(t =>
                    {

                        if (t.IsCompleted)
                        {
                            print("Kayıt başarılı");
                            PlayerPrefs.SetInt("Login", 1);
                            PlayerPrefs.SetInt("SecondLogin", 1);
                            PlayerPrefs.SetString("UserId", userId);
                            SetLeaderboard(nickname, 1);
                            //PanelManager.instance.ChangePanelVisibility("WaitingPanel", false);
                            //PanelManager.instance.ChangePanelVisibility("SavePanel", false);
                        }

                        else
                        {
                            print("Kayıt başarısız!");
                            //PanelManager.instance.ChangePanelVisibility("WaitingPanel", false);
                            //PanelManager.instance.ChangePanelVisibility("ErrorPanel", true);
                        }

                    });
                }
                else { PlayerPrefs.SetInt("Login", 1); PlayerPrefs.SetString("UserId", userId); }
            }          

        });


    }

    public void IncreaseFirebaseInfo(string statName, int incrementValue)
    {
        databaseReference.Child(registerType).Child(userId).Child(statName).GetValueAsync().ContinueWith(task =>
        {

            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                if (snapshot.Exists)
                {
                    var value = (long)snapshot.Value;
                    int newValue = (int)value + incrementValue;
                    UpdateFirebaseInfo(statName, newValue);
                }

            }

            else
            {
                Debug.LogError("Veri çekilemedi");
            }


        });
    }

    public void UpdateFirebaseInfo<T>(string statName, T newValue,Action onComplete=null)
    {
        databaseReference.Child(registerType).Child(userId).Child(statName).SetValueAsync(newValue).ContinueWithOnMainThread(task =>
        {

            if (task.IsCompleted)
            {
                print(statName + " firebase'e kaydedildi!");
                onComplete?.Invoke();
            }
            else
            {
                Debug.LogError(statName + " firebase'e kaydedilemedi!");
            }

        });
    }

    public void SaveGameDataToFirebase(GameData gameDataToSave, Action onComplete = null)
    {
        string json = JsonConvert.SerializeObject(gameDataToSave);

        databaseReference.Child("google").Child(userId).Child("GameData").SetRawJsonValueAsync(json).ContinueWithOnMainThread(saveTask =>
        {
            if (saveTask.IsCompleted)
            {
                Debug.Log("Game data saved to Firebase.");
                onComplete?.Invoke();
            }
            else
            {
                Debug.LogError("Failed to save game data: " + saveTask.Exception);
            }
        });

    }

    public void LoadGameDataFromFirebase(Action onComplete = null)
    {
        databaseReference.Child(registerType).Child(userId).Child("GameData").GetValueAsync().ContinueWithOnMainThread(task =>
        {

            if (task.IsCompleted)
            {
                DataSnapshot dataSnapshot = task.Result;

                if (dataSnapshot.Exists)
                {
                    string json = dataSnapshot.GetRawJsonValue();

                    try
                    {
                        GameData loadedData = JsonConvert.DeserializeObject<GameData>(json);
                        OnGameDataLoaded?.Invoke(loadedData);
                        onComplete?.Invoke();
                    }

                    catch (Exception ex)
                    {
                        OnGameDataLoaded?.Invoke(null);

                        if (ex.InnerException != null)
                        {
                            Debug.LogError("Inner Exception Type: " + ex.InnerException.GetType().Name);
                            Debug.LogError("Inner Exception Message: " + ex.InnerException.Message);
                            Debug.LogError("Inner Exception Stack Trace: " + ex.InnerException.StackTrace);
                            GamePanelManager.instance.LoadPanel("ErrorPanel");
                        }

                        Debug.LogError("Full Exception Stack Trace: " + ex.StackTrace);

                    }

                }

                else
                {
                    GamePanelManager.instance.LoadPanel("ErrorPanel");
                }
            }

            else
            {
                Debug.Log("GameData çekilirken hata oluştu");
                GamePanelManager.instance.LoadPanel("ErrorPanel");
            }

        });
    }

    public void GetOldInfoFromFirebase(string username,Action<Dictionary<string, object>> onComplete)
    {

        string userID = PlayerPrefs.GetString("UserId");

        databaseReference.Child("users").Child(userID).GetValueAsync().ContinueWithOnMainThread(task =>
        {

            if (task.IsCompleted)
            {
                DataSnapshot dataSnapshot = task.Result;

                if (dataSnapshot.Exists)
                {
       
                    string nickname = dataSnapshot.Child("nickname").Value?.ToString() ?? "not found";
                    string date = dataSnapshot.Child("date").Value?.ToString() ?? DateTime.Now.ToString("dd-MM-yyyy");

                    int gameTime = 0;
                    int level = 1;
                    int watchedAdCount = 0;
                    int adLoadingErrorCount = 0;
                    bool removeAd = false;

                    int.TryParse(dataSnapshot.Child("gameTime").Value?.ToString(), out gameTime);
                    int.TryParse(dataSnapshot.Child("level").Value?.ToString(), out level);
                    int.TryParse(dataSnapshot.Child("watchedAdCount").Value?.ToString(), out watchedAdCount);
                    int.TryParse(dataSnapshot.Child("adLoadingErrorCount").Value?.ToString(), out adLoadingErrorCount);
                    bool.TryParse(dataSnapshot.Child("removeAd").Value?.ToString(), out removeAd);

                    Dictionary<string, object> oldUser = new Dictionary<string, object>
                    {
                       { "googleName", username },
                       { "nickname", nickname },
                       { "date", date },
                       { "gameTime", gameTime },
                       { "level", level },
                       { "watchedAdCount", watchedAdCount },
                       { "adLoadingErrorCount", adLoadingErrorCount },
                       { "removeAd", removeAd }
                    };
               
                    onComplete?.Invoke(oldUser);
                }
                else
                {
                    //GamePanelManager.instance.LoadPanel("InfoPanel", "GetInfoFromFirebase snapshot not exists");
                    onComplete?.Invoke(null);
                }
            }

            else
            {
                GamePanelManager.instance.LoadPanel("ErrorPanel");
                onComplete?.Invoke(null);
            }


        });


    }

    public void SetNewInfoFromFirebase(Dictionary<string, object> userData)
    {
        string json = JsonUtility.ToJson(new SerializableDictionary(userData));

        databaseReference.Child(registerType).Child(userId).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                // Başarılı
            }
            else
            {
                Debug.LogError("SetInfoFromFirebase task fail!");
            }
        });
    }

    public void DeleteOldAccount()
    {
        
        string id = PlayerPrefs.GetString("UserId","");

        if (string.IsNullOrEmpty(id))
        {
            return;
        }

        databaseReference.Child("users").Child(id).GetValueAsync().ContinueWith(getTask =>
        {
            if (getTask.IsCompleted && getTask.Result.Exists)
            {
                // Kullanıcı verisi varsa sil
                databaseReference.Child("users").Child(id).RemoveValueAsync().ContinueWith(removeTask =>
                {
                    if (removeTask.IsCompleted)
                    {
                        Debug.Log("Kullanıcı başarıyla silindi.");
                    }
                    else
                    {
                        Debug.LogError("Silme işlemi sırasında hata oluştu: " + removeTask.Exception);
                    }
                });
            }
            else
            {
                Debug.LogWarning("Belirtilen ID'ye sahip kullanıcı verisi bulunamadı, silme işlemi yapılmadı.");
            }
        });
    }


    public void GetInfoFromFirebase(string field,Action<string> onComplete)
    {
        
        databaseReference.Child(registerType).Child(userId).GetValueAsync().ContinueWith(task => {

            if (task.IsCompleted)
            {
                DataSnapshot dataSnapshot = task.Result;

                if (dataSnapshot.Exists)
                {
                    var value = dataSnapshot.Child(field).Value.ToString();
                    onComplete.Invoke(value);
                }

            }
                    
        });
    }


    public void GetBestPlayers(Action onComplete)
    {
        string currentUserId = userId;
        string currentUsername = "";
        int currentLevel = 0;
        int currentRank = 0;
        bool userInTopList = false;

        databaseReference.Child("leaderboard").OrderByChild("level").LimitToLast(20).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot dataSnapshot = task.Result;

                if (dataSnapshot.Exists)
                {
                    List<(string userId, string username, int level)> users = new();

                    foreach (var user in dataSnapshot.Children)
                    {
                        string userId = user.Key;
                        string username = user.Child("nickname")?.Value?.ToString();
                        int.TryParse(user.Child("level")?.Value?.ToString(), out int level);
                        users.Add((userId, username, level));
                    }

                    var sortedList = users.OrderByDescending(v => v.level).ToList();

                    GameObject headerPanel = UIRepository.Instance.HeaderPanel;
                    Transform parentTransform = UIRepository.Instance.ParentTransform;

                    GameObject header = Instantiate(headerPanel, parentTransform);
                    header.GetComponent<Image>().color = new Color32(255, 255, 255, 200);
                    TextMeshProUGUI[] headerText = header.GetComponentsInChildren<TextMeshProUGUI>();
                    string language = LanguageManager.GetLanguage();
                    
                    string headerKey1 = LanguageManager.instance.GetLocalizedValue("LeaderboardUsernameText");
                    string headerKey2 = LanguageManager.instance.GetLocalizedValue("LeaderboardLevelText");
                    headerText[0].text = "";
                    headerText[1].text = headerKey1;
                    headerText[2].text = headerKey2;
                    headerText[1].color = Color.black;
                    headerText[2].color = Color.black;

                    GameObject userPanel = UIRepository.Instance.UserPanel;

                    int index = 1;
                    foreach (var user in sortedList)
                    {
                        GameObject topPlayer = Instantiate(userPanel, parentTransform);
                        TextMeshProUGUI[] topPlayerText = topPlayer.GetComponentsInChildren<TextMeshProUGUI>();

                        topPlayerText[0].text = index.ToString();
                        topPlayerText[1].text = user.username;
                        topPlayerText[2].text = user.level.ToString();

                        if (user.userId == currentUserId)
                        {
                            topPlayer.GetComponent<Image>().color = new Color32(255, 255, 100, 200); // sarımsı panel
                            userInTopList = true;
                        }

                        index++;
                    }

                    if (!userInTopList)
                    {
                        databaseReference.Child("leaderboard").OrderByChild("level").GetValueAsync().ContinueWithOnMainThread(allTask =>
                        {
                            if (allTask.IsCompleted)
                            {
                                var allSnapshot = allTask.Result;
                                List<(string userId, string username, int level)> allUsers = new();

                                foreach (var user in allSnapshot.Children)
                                {
                                    string userId = user.Key;
                                    string username = user.Child("nickname")?.Value?.ToString();
                                    int.TryParse(user.Child("level")?.Value?.ToString(), out int level);
                                    allUsers.Add((userId, username, level));
                                }

                                var orderedAll = allUsers.OrderByDescending(v => v.level).ToList();
                                for (int i = 0; i < orderedAll.Count; i++)
                                {
                                    if (orderedAll[i].userId == currentUserId)
                                    {
                                        currentUsername = orderedAll[i].username;
                                        currentLevel = orderedAll[i].level;
                                        currentRank = i + 1;
                                        break;
                                    }
                                }

                                GameObject userSelfPanel = Instantiate(userPanel, parentTransform);
                                TextMeshProUGUI[] selfText = userSelfPanel.GetComponentsInChildren<TextMeshProUGUI>();
                                selfText[0].text = currentRank.ToString();
                                selfText[1].text = currentUsername;
                                selfText[2].text = currentLevel.ToString();
                                userSelfPanel.GetComponent<Image>().color = new Color32(200, 255, 200, 200); // açık yeşil
                            }

                            onComplete?.Invoke();
                        });
                    }
                    else
                    {
                        onComplete?.Invoke();
                    }
                }
            }
            else
            {
                Debug.LogError("En iyi oyuncular çekilemedi!");
                PanelManager.instance.ClosePanel("WaitingPanel");
            }
        });
    }

    private void SetLeaderboard(string nickname,int level)
    {

        Dictionary<string, object> userData = new Dictionary<string, object>()
        {
             { "nickname", nickname },
             { "level", level }
        };

        databaseReference.Child("leaderboard").Child(userId).SetValueAsync(userData).ContinueWith(task => {

            if (task.IsCompleted)
            {
                Debug.Log("Leaderboard kaydı oluşturuldu!");
            }

            else
            {
                Debug.LogError("Leaderboard kaydı oluşturulurken hata oluştu:"+task.Exception);
            }
                
        });

    }

    public void UpdateLeaderboardName(string nickname,Action onComplete=null)
    {
        databaseReference.Child("leaderboard").Child(userId).Child("nickname").SetValueAsync(nickname).ContinueWithOnMainThread(task => {

            if (task.IsCompleted)
            {
                onComplete?.Invoke();
            }
            else
            {

            }
        });
    }
    public void IncreaseLevel()
    {
       
        databaseReference.Child("leaderboard").Child(userId).Child("level").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                int currentLevel = int.Parse(task.Result.Value.ToString());
                int newLevel = currentLevel + 1;

                databaseReference.Child("leaderboard").Child(userId).Child("level").SetValueAsync(newLevel).ContinueWithOnMainThread(updateTask =>
                {
                    if (updateTask.IsCompleted)
                        Debug.Log("Seviye artırıldı: " + newLevel);
                    else
                        Debug.LogError("Seviye artırılamadı: " + updateTask.Exception);
                });
            }
        });
    }

    public void TransferLeaderboardData()
    {

        string oldUserId = PlayerPrefs.GetString("UserId");


        if (string.IsNullOrEmpty(oldUserId))
        {
            return;
        }

        var leaderboardRef = databaseReference.Child("leaderboard");

        leaderboardRef.Child(oldUserId).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                var data = task.Result.Value as Dictionary<string, object>;

                leaderboardRef.Child(userId).GetValueAsync().ContinueWithOnMainThread(task => { 
                
                   if(task.IsCompleted)
                    {

                        if (!task.Result.Exists)
                        {
                            leaderboardRef.Child(userId).SetValueAsync(data).ContinueWithOnMainThread(setTask =>
                            {
                                if (setTask.IsCompleted)
                                {
                                    Debug.Log("Veriler yeni kullanıcıya aktarıldı.");
                                    DeleteOldRecord(leaderboardRef, oldUserId);
                                  
                                }
                                else
                                {
                                    Debug.LogError("Yeni kullanıcıya veri aktarılamadı: " + setTask.Exception);
                                    DeleteOldRecord(leaderboardRef, oldUserId);
                                }
                            });
                        }

                    }
                             

                });              

            }
            else
            {
                Debug.LogWarning("Taşınacak veri bulunamadı.");
            }
        });
    }

    private void DeleteOldRecord(DatabaseReference leaderboardRef,string oldUserId)
    {
        leaderboardRef.Child(oldUserId)
             .RemoveValueAsync()
             .ContinueWithOnMainThread(removeOld =>
             {
                 if (removeOld.IsCompleted)
                 {
                     Debug.Log("Eski kullanıcı kaydı silindi.");                  
                 }
                 else
                 {
                     Debug.LogError("Eski kayıt silinemedi: " + removeOld.Exception);
                 }
             });
    }


    //Diğer fonksiyonlar
  
    private void AddNewFieldToAllUsers<T>(string fieldName, T value)
    {

        databaseReference.Child("users").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                DataSnapshot snapshot = task.Result;

                foreach (DataSnapshot userSnapshot in snapshot.Children)
                {
                    string userId = userSnapshot.Key;


                    if (!userSnapshot.HasChild(fieldName))
                    {
                        databaseReference.Child("users").Child(userId).Child(fieldName).SetValueAsync(value).ContinueWithOnMainThread(setTask =>
                        {
                            if (setTask.IsCompleted)
                            {
                                Debug.Log($"{fieldName} eklendi: {userId}");
                            }
                            else
                            {
                                Debug.LogWarning($"{fieldName} eklenemedi: {userId} - {setTask.Exception}");
                            }
                        });
                    }
                    else
                    {
                        Debug.Log($"Zaten var: {userId}");
                    }
                }
            }
            else
            {
                Debug.LogError("Kullanıcılar çekilemedi veya veri yok.");
            }
        });
    }

    private async void SetLevelRandom()
    {
        DataSnapshot dataSnapshot = await databaseReference.Child("users").GetValueAsync();

        if (dataSnapshot.Exists)
        {
            int gameTime = 0;

            foreach (DataSnapshot userSnapshot in dataSnapshot.Children)
            {
                int.TryParse(userSnapshot.Child("gameTime").Value.ToString(), out gameTime);
                string id = userSnapshot.Key;

                if (gameTime == 0)
                {
                    int random = UnityEngine.Random.Range(10, 50);
                    await databaseReference.Child("users").Child(id).Child("level").SetValueAsync(random);
                }

            }
        }
    }

    private void CleanOldUsers()
    {
        DateTime cutoffDate = new DateTime(2024, 12, 31);

        FirebaseDatabase.DefaultInstance.GetReference("users").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Veri çekilirken hata oluştu.");
                return;
            }

            if (task.IsCompleted)
            {
                DataSnapshot dataSnapshot = task.Result;

                if (!dataSnapshot.Exists)
                {
                    Debug.Log("Kullanıcı verisi bulunamadı.");
                    return;
                }

                Dictionary<string, object> updates = new Dictionary<string, object>();

                foreach (var userSnapshot in dataSnapshot.Children)
                {
                    string userId = userSnapshot.Key;

                    string dateStr = userSnapshot.Child("date")?.Value?.ToString();
                    int.TryParse(userSnapshot.Child("gameTime")?.Value?.ToString(), out int gameTime);

                    if (DateTime.TryParse(dateStr, out DateTime userDate))
                    {
                        if (userDate < cutoffDate && gameTime < 10)
                        {
                            updates[$"users/{userId}"] = null; // kullanıcıyı sil
                        }
                    }
                }

                if (updates.Count > 0)
                {
                    FirebaseDatabase.DefaultInstance.RootReference.UpdateChildrenAsync(updates).ContinueWith(updateTask =>
                    {
                        if (updateTask.IsCompleted)
                        {
                            Debug.Log($"{updates.Count} kullanıcı silindi.");
                        }
                        else
                        {
                            Debug.LogError("Kullanıcılar silinirken hata oluştu.");
                        }
                    });
                }
                else
                {
                    Debug.Log("Silinecek kullanıcı yok.");
                }
            }
        });
    }

    private void RenameUsernameToNickname()
    {
        
        databaseReference.Child("users").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                DataSnapshot snapshot = task.Result;

                foreach (var user in snapshot.Children)
                {
                    string userId = user.Key;

                    if (!user.HasChild("username"))
                    {
                        continue;
                    }

                    // "username" var mı?
                    var usernameValue = user.Child("username").Value?.ToString();

                    if (!string.IsNullOrEmpty(usernameValue))
                    {
                        Dictionary<string, object> updates = new Dictionary<string, object>
                    {
                        { "nickname", usernameValue },
                        { "username", null } // Bu alan silinir
                    };

                        databaseReference.Child("users").Child(userId).UpdateChildrenAsync(updates).ContinueWith(updateTask =>
                        {
                            if (updateTask.IsCompletedSuccessfully)
                            {
                                Debug.Log($"'{userId}' kullanıcısında username → nickname olarak değiştirildi.");
                            }
                            else
                            {
                                Debug.LogError($"Hata: {userId} için güncelleme başarısız.");
                            }
                        });
                    }
                }
            }
            else
            {
                Debug.LogError("Kullanıcı verileri çekilemedi: " + task.Exception);
            }
        });
    }

    private void FillEmptyFields()
    {
        databaseReference.Child("users").GetValueAsync().ContinueWith(task => { 
        
          
            if(task.IsCompletedSuccessfully)
            {

                DataSnapshot dataSnapshot = task.Result;

                if (dataSnapshot.Exists)
                {

                    foreach (DataSnapshot userSnapshot in dataSnapshot.Children)
                    {
                        string userId = userSnapshot.Key;
                        Dictionary<string, object> updates = new Dictionary<string, object>();

                        if (!userSnapshot.HasChild("nickname"))
                        {
                            int random = UnityEngine.Random.Range(100, 999999);
                            string nickname = "guest" + random;
                            updates["nickname"] = nickname;
                        }

                        if (!userSnapshot.HasChild("gameTime"))
                        {
                            updates["gameTime"] = 0;
                        }

                        if (!userSnapshot.HasChild("level"))
                        {
                            updates["level"] = 1;
                        }

                        if (!userSnapshot.HasChild("watchedAdCount"))
                        {
                            updates["watchedAdCount"] = 0;
                        }

                        if (!userSnapshot.HasChild("date"))
                        {
                            string date = DateTime.Now.ToString("dd-MM-yyyy");
                            updates["date"] = date;
                        }

                        if (!userSnapshot.HasChild("adLoadingErrorCount"))
                        {
                            updates["adLoadingErrorCount"] = 0;
                        }

                        if (!userSnapshot.HasChild("removeAd"))
                        {
                            updates["removeAd"] = false;
                        }

                        if (updates.Count > 0)
                        {
                            databaseReference.Child("users").Child(userId).UpdateChildrenAsync(updates).ContinueWithOnMainThread(updateTask =>
                            {
                                if (updateTask.IsCompletedSuccessfully)
                                {
                                    Debug.Log($"'{userId}' kullanıcısı için eksik alanlar güncellendi.");
                                }
                                else
                                {
                                    Debug.LogError($"Hata: {userId} güncellenemedi → {updateTask.Exception}");
                                }
                            });
                        }
                        else
                        {
                            Debug.Log($"'{userId}' kullanıcısında tüm alanlar zaten mevcut.");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("Hiç kullanıcı bulunamadı.");
                }

            }

        
        
        
        
        });
    }

    private void SetLeaderboardUsers()
    {

        databaseReference.Child("users").GetValueAsync().ContinueWithOnMainThread(task => {

            if (task.IsCompleted)
            {
                DataSnapshot dataSnapshot = task.Result;

                if (dataSnapshot.Exists)
                {
         
                    foreach (var item in dataSnapshot.Children)
                    {
                        string nickname = item.Child("nickname").Value?.ToString();
                        int gameTime = int.Parse(item.Child("gameTime").Value?.ToString());
                        string date = item.Child("date").Value?.ToString();
                        int level =int.Parse(item.Child("level").Value?.ToString());

                        if (gameTime != 0)
                        {
                            string id = item.Key;

                            int random = UnityEngine.Random.Range(5, 50);

                            Dictionary<string, object> data = new Dictionary<string, object>()
                            {
                                {"nickname",nickname },
                                {"level",level}
                            };
                            //data["nickname"] = nickname;
                            //data["level"] = level;

                            databaseReference.Child("leaderboard").Child(id).SetValueAsync(data).ContinueWith(task =>
                            {

                                if (task.IsCompletedSuccessfully)
                                {
                                    Debug.Log($"{id} id değerine sahip {nickname} leaderboarda set edildi! ");
                                }

                                else if(task.IsFaulted || task.IsCanceled)
                                {
                                        Debug.LogError($"{id} değerine sahip {nickname} leaderboarda set edilemedi!");
                                }

                            });

                        }

                    }
                }

                else
                {
                    Debug.LogError("Veri yok");
                }
            }
            else
            {
                Debug.LogError("Veriler çekilemedi");
            }
        
        });
       

       
    }

    private void UpdateLeaderboard()
    {
        databaseReference.Child("leaderboard").GetValueAsync().ContinueWithOnMainThread(task => {


            if (task.IsCompletedSuccessfully)
            {
                DataSnapshot dataSnapshot = task.Result;

                if (dataSnapshot.Exists)
                {
                    foreach(var user in dataSnapshot.Children)
                    {
                        int level = int.Parse(user.Child("level").Value?.ToString());

                        if(level == 1)
                        {
                            string key = user.Key;

                            databaseReference.Child("leaderboard").Child(key).RemoveValueAsync();
                        }

                    }
                }

            }
           
        
        
        });
    }

    private void FixLeaderboard()
    {
        databaseReference.Child("leaderboard").Child("dyH0oBCaTuUSP4VTV2tuuTjkI1e2").GetValueAsync().ContinueWith(task => { 
        
            if(task.IsCompletedSuccessfully && task.Result.Exists)
            {

                DataSnapshot dataSnapshot = task.Result;

                foreach (var child in dataSnapshot.Children)
                {
                    string userId = child.Key;
                    object userData = child.Value;

                    databaseReference.Child("leaderboard").Child(userId).SetValueAsync(userData)
                        .ContinueWithOnMainThread(copyTask =>
                        {
                            if (copyTask.IsCompleted)
                                Debug.Log($"Kullanıcı {userId} başarıyla taşındı.");
                            else
                                Debug.LogError($"Kullanıcı {userId} taşınamadı: {copyTask.Exception}");
                        });
                }

            }
        
        
        
        });
    }


}
