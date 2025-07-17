using CryingSnow.CheckoutFrenzy;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using Google;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginWithGoogle : MonoBehaviour
{
    public static LoginWithGoogle instance;

    public string GoogleWebApi = "476256887766-o2qdgmp2210vd8dlu67q5bbhg4srrp9k.apps.googleusercontent.com";

    FirebaseAuth auth;

    FirebaseUser user;

    private bool isGoogleSignInInitialized = false;

    public bool authInitialized = false;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        StartCoroutine(CheckFirebaseInitialized());

        if(!PlayerPrefs.HasKey("GoogleLogin"))
           StartCoroutine(GoogleLoginControl());
    }

    private void SetAuth()
    {
        auth = FirebaseAuth.DefaultInstance;

        if (auth.CurrentUser != null)
        {
            print("Auth null değil");
            DatabaseManager.Instance.InitializeDatabase(auth.CurrentUser.UserId, "google");
        }

        else
        {
            print("Auth null");
            DatabaseManager.Instance.InitializeDatabase();
        }

        authInitialized = true;
    }

    public void LoginWithGoogleAndCheckDatabase()
    {
        if (!isGoogleSignInInitialized)
        {
            GoogleSignIn.Configuration = new GoogleSignInConfiguration
            {
                RequestIdToken = true,
                WebClientId = GoogleWebApi,
                RequestEmail = true,
                RequestProfile = true
            };
            isGoogleSignInInitialized = true;
        }

        GoogleSignIn.DefaultInstance.SignOut();


        GoogleSignIn.DefaultInstance.SignIn().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Google Sign-In failed.");
                GamePanelManager.instance.LoadPanel("ErrorPanel");
                return;
            }

            else if (task.IsCanceled)
            {
                return;
            }

            //GamePanelManager.instance.LoadPanel("LoadingPanel");

            GoogleSignInUser googleUser = task.Result;
            Credential credential = GoogleAuthProvider.GetCredential(googleUser.IdToken, null);

            string username = googleUser.DisplayName;

            auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(authTask =>
            {
                if (task.IsCanceled)
                {
                    Debug.Log("Google Sign-In was canceled by the user.");
                    //GamePanelManager.instance.ClosePanel("LoadingPanel");
                    return; 
                }
        
                if (task.IsFaulted)
                {
                    Debug.LogError($"Google Sign-In failed with error: {task.Exception}");
                    //GamePanelManager.instance.ClosePanel("LoadingPanel");
                    GamePanelManager.instance.LoadPanel("ErrorPanel");               
                    return; 
                }

                user = auth.CurrentUser;
                string userId = user.UserId;


                DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.RootReference;
                dbRef.Child("google").Child(userId).GetValueAsync().ContinueWithOnMainThread(dataTask =>
                {
                    if (dataTask.IsFaulted)
                    {
                        Debug.LogError("Database check failed.");
                        GamePanelManager.instance.LoadPanel("ErrorPanel");
                    }
                    else if (dataTask.Result.Exists)
                    {
                        // ✅ Kayıt var
                        Debug.Log("Kullanıcı kaydı bulundu.");      
                        DatabaseManager.Instance.InitializeDatabase(userId, "google");
                        DatabaseManager.Instance.GetInfoFromFirebase("removeAd", (removeAd) =>
                        {
                            if (!string.IsNullOrEmpty(removeAd) && bool.TryParse(removeAd, out bool result) && result)
                            {
                                PlayerPrefs.SetInt("RemoveAd", 1);
                                Stopwatch.Instance.StopShowAdCoroutine();
                            }
                        });
                        DatabaseManager.Instance.LoadGameDataFromFirebase(() => {
                               LoginCompleted();
                        });
       
                    }
                    else
                    {
                        // ❌ Kayıt yok → Panel aç
                        Debug.Log("Kayıt bulunamadı. Panel açılıyor...");                                     
                        DatabaseManager.Instance.InitializeDatabase();
                        LoadUserInfoAfterGoogleLogin(userId,username);

                    }
                });
            });
        });
    }


    private void LoadUserInfoAfterGoogleLogin(string userId,string username)
    {

        DatabaseManager.Instance.GetOldInfoFromFirebase(username,oldUser =>
        {
    
           
            if (oldUser != null)
            {
                DatabaseManager.Instance.InitializeDatabase(userId, "google");
                DatabaseManager.Instance.SetNewInfoFromFirebase(oldUser);    
                DatabaseManager.Instance.SaveGameDataToFirebase(DataManager.Instance.Data,() => { LoginCompleted(); });           
            }
            else
            {
                DatabaseManager.Instance.InitializeDatabase(userId, "google");
                int random = UnityEngine.Random.Range(1,999999);
                DateTime today = DateTime.Now;
                string date = today.ToString("dd-MM-yyyy");
               
                Dictionary<string, object> userData = new Dictionary<string, object>
                {
                   { "googleName", username },
                   { "nickname", "guest"+random },
                   { "gameTime", 0 },
                   { "level", 1 },
                   { "date", date },
                   { "removeAd", false },
                   { "adLoadingErrorCount", 0 },
                   { "watchedAdCount", 0 }
                };
              
                DatabaseManager.Instance.SetNewInfoFromFirebase(userData);
                DatabaseManager.Instance.SaveGameDataToFirebase(DataManager.Instance.Data, () => { LoginCompleted(); });             
            }          

        });

    }

  
    private IEnumerator ShowGoogleLoginPanel()
    {
        while (true)
        {
            int random = UnityEngine.Random.Range(15, 30);
            yield return new WaitForSeconds(random);

            if (!GamePanelManager.instance.IsThereOpenPanel())
            {
                GamePanelManager.instance.LoadPanel("GoogleLoginPanel");
                yield break;
            }

        }
    }

    private IEnumerator GoogleLoginControl()
    {

        while (true)
        {

            if(SceneManager.GetActiveScene().buildIndex == 1)
            {
                if(PlayerPrefs.GetInt("EntriesCount",0) > 0)
                {
                    StartCoroutine(ShowGoogleLoginPanel());
                    yield break;
                }

                else
                {
                    PlayerPrefs.SetInt("EntriesCount", 1);
                    yield break;
                }
            }
       
            yield return new WaitForSeconds(30);

        }
             
    }

    private IEnumerator CheckFirebaseInitialized()
    {
        while (true)
        {
            if (DatabaseManager.Instance.firebaseInitialized)
            {
                SetAuth();
                yield break;
            }

            yield return new WaitForSeconds(1);
        }
    }

    private void LoginCompleted()
    {
        PlayerPrefs.SetInt("GoogleLogin", 1);
        GamePanelManager.instance.LoadPanel("LoadingPanel");
        //GamePanelManager.instance.CloseAllPanel();
        string message = LanguageManager.instance.GetLocalizedValue("LoginSuccessText");    
        StatsDisplay.instance.UpdateLevelDisplay();
        DatabaseManager.Instance.DeleteOldAccount();
        DatabaseManager.Instance.TransferLeaderboardData();
        StartCoroutine(ReloadScene());                      
    }

    IEnumerator ReloadScene()
    {    
        yield return new WaitForSeconds(3);
        GamePanelManager.instance.CloseAllPanel();
        ReloadSceneAsync.instance.RestartGame();
    }

}
