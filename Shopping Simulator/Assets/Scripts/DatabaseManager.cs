using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Extensions;
using Firebase.Database;
using TMPro;
using System;

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
        User user = new User(username, 0, 0,date);
        string json = JsonUtility.ToJson(user);

        databaseReference.Child("users").Child(userId).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
        {

            if (task.IsCompleted)
            {
                print("Kayýt baþarýlý");
                PlayerPrefs.SetInt("Login",1);
                PanelManager.instance.ChangePanelVisibility("SavePanel", false);
            }

            else
            {
                print("Kayýt baþarýsýz!");
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


   
}
