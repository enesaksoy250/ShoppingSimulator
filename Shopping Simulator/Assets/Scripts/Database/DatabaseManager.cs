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

public class DatabaseManager : MonoBehaviour
{

    public static DatabaseManager Instance;

    DatabaseReference databaseReference;

    private string userId;

    [SerializeField] TMP_InputField usernameInput;
    [SerializeField] TMP_InputField storeNameInput;

    private void Awake()
    {
        if(Instance == null)
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
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                print("Firebase'e baðlandý!");
                databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
                userId = SystemInfo.deviceUniqueIdentifier;
                //DatabaseStatistic databaseStatistic = new DatabaseStatistic(databaseReference, userId);
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
            }
        });
    }

    public void Save()
    {
        string username = usernameInput.text;
        string storeName = storeNameInput.text;

        if (username == "")
            return;

        string date = DateTime.Now.ToString("dd-MM-yyyy");
        PlayerPrefs.SetString("StoreName",storeName.ToUpper());
        User user = new User(username, 0, 0,date,false,0,1);
        string json = JsonUtility.ToJson(user);

        databaseReference.Child("users").Child(userId).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
        {

            if (task.IsCompleted)
            {
                print("Kayýt baþarýlý");
                PlayerPrefs.SetInt("Login",1);
                PanelManager.instance.ChangePanelVisibility("WaitingPanel", false);
                PanelManager.instance.ChangePanelVisibility("SavePanel", false);
            }

            else
            {
                print("Kayýt baþarýsýz!");
                PanelManager.instance.ChangePanelVisibility("WaitingPanel", false);
                PanelManager.instance.ChangePanelVisibility("ErrorPanel", true);
            }

        });


    }
   
    public void IncreaseFirebaseInfo(string statName,int incrementValue)
    {
        databaseReference.Child("users").Child(userId).Child(statName).GetValueAsync().ContinueWith(task => 
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

    public void UpdateFirebaseInfo<T>(string statName,T newValue)
    {
        databaseReference.Child("users").Child(userId).Child(statName).SetValueAsync(newValue).ContinueWith(task => 
        {

            if (task.IsCompleted)
            {
                print(statName+" firebase'e kaydedildi!");
            }
            else
            {
                Debug.LogError(statName+" firebase'e kaydedilemedi!");
            }
        
        });
    }

    public void GetBestPlayers2(Action onComplete)
    {
        databaseReference.Child("users").OrderByChild("level").LimitToLast(50).GetValueAsync().ContinueWithOnMainThread(task => {

            if (task.IsCompleted)
            {
                DataSnapshot dataSnapshot = task.Result;

                if (dataSnapshot.Exists)
                {
                    List<(string username, int level)> users = new();

                    foreach (var user in dataSnapshot.Children)
                    {
                        string username = user.Child("username")?.Value.ToString();
                        int level = 0;
                        int.TryParse(user.Child("level")?.Value.ToString(), out level);
                        users.Add((username, level));
                    }

                    var sortedList = users.OrderByDescending(v => v.level).ToList();

                    GameObject userPanel = UIRepository.Instance.UserPanel;
                    Transform parentTransform = UIRepository.Instance.ParentTransform;

                    GameObject header = Instantiate(userPanel, parentTransform);
                    header.GetComponent<Image>().color = new Color32(255, 255, 255, 200);
                    TextMeshProUGUI[] headerText = header.GetComponentsInChildren<TextMeshProUGUI>();
                    string language = PlayerPrefs.GetString("Language");
                    string headerKey1 = language == "English" ? "USERNAME" : "KULLANICI ADI";
                    string headerKey2 = language == "English" ? "LEVEL" : "SEVÝYE";
                    headerText[0].text = "";
                    headerText[1].text = headerKey1;
                    headerText[1].color = Color.black;
                    headerText[2].text = headerKey2;
                    headerText[2].color = Color.black;

                    int index = 1;
                    foreach (var user in sortedList)
                    {
                        GameObject topPlayer = Instantiate(userPanel, parentTransform);
                        TextMeshProUGUI[] topPlayerText = topPlayer.GetComponentsInChildren<TextMeshProUGUI>();
                        topPlayerText[0].text = index.ToString(); // sýralý index
                        topPlayerText[1].text = user.username;
                        topPlayerText[2].text = user.level.ToString();
                        index++;
                    }

                    onComplete?.Invoke();
                }


            }
            else
            {
                Debug.LogError("En iyi oyuncular çekilemedi!");
                PanelManager.instance.ClosePanel("WaitingPanel");
            }
        
        });
    }

    public void GetBestPlayers(Action onComplete)
    {
        string currentUserId = userId; // Kullanýcý ID'si burada saklandýðýný varsayýyoruz
        string currentUsername = "";
        int currentLevel = 0;
        int currentRank = 0;
        bool userInTopList = false;

        databaseReference.Child("users").OrderByChild("level").LimitToLast(50).GetValueAsync().ContinueWithOnMainThread(task =>
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
                        string username = user.Child("username")?.Value?.ToString();
                        int.TryParse(user.Child("level")?.Value?.ToString(), out int level);
                        users.Add((userId, username, level));
                    }

                    var sortedList = users.OrderByDescending(v => v.level).ToList();

                    GameObject userPanel = UIRepository.Instance.UserPanel;
                    Transform parentTransform = UIRepository.Instance.ParentTransform;

                    GameObject header = Instantiate(userPanel, parentTransform);
                    header.GetComponent<Image>().color = new Color32(255, 255, 255, 200);
                    TextMeshProUGUI[] headerText = header.GetComponentsInChildren<TextMeshProUGUI>();
                    string language = LanguageManager.GetLanguage();
                    string headerKey1 = language == "English" ? "USERNAME" : "KULLANICI ADI";
                    string headerKey2 = language == "English" ? "LEVEL" : "SEVÝYE";
                    headerText[0].text = "";
                    headerText[1].text = headerKey1;
                    headerText[1].color = Color.black;
                    headerText[2].text = headerKey2;
                    headerText[2].color = Color.black;

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
                            topPlayer.GetComponent<Image>().color = new Color32(255, 255, 100, 200); // sarýmsý panel
                            userInTopList = true;
                        }

                        index++;
                    }

                    if (!userInTopList)
                    {
                        // Tüm kullanýcýlarý çekip kendi sýralamaný bul
                        databaseReference.Child("users").OrderByChild("level").GetValueAsync().ContinueWithOnMainThread(allTask =>
                        {
                            if (allTask.IsCompleted)
                            {
                                var allSnapshot = allTask.Result;
                                List<(string userId, string username, int level)> allUsers = new();

                                foreach (var user in allSnapshot.Children)
                                {
                                    string userId = user.Key;
                                    string username = user.Child("username")?.Value?.ToString();
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

                                // Listeye kullanýcýyý ekle
                                GameObject userSelfPanel = Instantiate(userPanel, parentTransform);
                                TextMeshProUGUI[] selfText = userSelfPanel.GetComponentsInChildren<TextMeshProUGUI>();
                                selfText[0].text = currentRank.ToString();
                                selfText[1].text = currentUsername;
                                selfText[2].text = currentLevel.ToString();
                                userSelfPanel.GetComponent<Image>().color = new Color32(200, 255, 200, 200); // açýk yeþil
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


    //Tüm kullanýcýlara yeni bir alan ekler
    private void AddNewFieldToAllUsers<T>(string fieldName,T value)
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
                Debug.LogError("Kullanýcýlar çekilemedi veya veri yok.");
            }
        });
    }

    private async void SetLevelRandom()
    {
        DataSnapshot dataSnapshot = await databaseReference.Child("users").GetValueAsync();

        if (dataSnapshot.Exists)
        {
            int gameTime = 0;

            foreach(DataSnapshot userSnapshot in dataSnapshot.Children)
            {
                int.TryParse(userSnapshot.Child("gameTime").Value.ToString(),out gameTime);
                string id = userSnapshot.Key;

                if(gameTime == 0)
                {
                    int random = UnityEngine.Random.Range(10, 50);
                    await databaseReference.Child("users").Child(id).Child("level").SetValueAsync(random);
                }

            }
        }
    }

    //Eski boþ hesaplarý silme
    private void CleanOldUsers()
    {
        DateTime cutoffDate = new DateTime(2024, 12, 31);

        FirebaseDatabase.DefaultInstance.GetReference("users").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Veri çekilirken hata oluþtu.");
                return;
            }

            if (task.IsCompleted)
            {
                DataSnapshot dataSnapshot = task.Result;

                if (!dataSnapshot.Exists)
                {
                    Debug.Log("Kullanýcý verisi bulunamadý.");
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
                            updates[$"users/{userId}"] = null; // kullanýcýyý sil
                        }
                    }
                }

                if (updates.Count > 0)
                {
                    FirebaseDatabase.DefaultInstance.RootReference.UpdateChildrenAsync(updates).ContinueWith(updateTask =>
                    {
                        if (updateTask.IsCompleted)
                        {
                            Debug.Log($"{updates.Count} kullanýcý silindi.");
                        }
                        else
                        {
                            Debug.LogError("Kullanýcýlar silinirken hata oluþtu.");
                        }
                    });
                }
                else
                {
                    Debug.Log("Silinecek kullanýcý yok.");
                }
            }
        });
    }


}
