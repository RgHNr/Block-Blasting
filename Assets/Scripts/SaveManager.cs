using UnityEngine;
using System;
using System.IO;
using Firebase.Extensions;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using UnityEditor;
using Firebase.RemoteConfig;
using System.Collections.Generic;


public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    public PlayerData data;
    FirebaseApp app;
    FirebaseAuth Auth;
    DatabaseReference databaseReference;
    FirebaseRemoteConfig remoteConfig;
    string userID;
    bool isFirebaseReady;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
        data = LoadData();
        if (data == null) data = new();
        InitFirebase();
    }
    void Start()
    {
        if (data != null) UIManager.Instance.UpdateCoins(data.gold);
        
       
    }

    void LoadDatafromCloud()
    {

    }

    void AnonymousLogin()
    {
        Auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task => {

            if (task.IsCanceled)
            {
                Debug.LogError("SignInAnonymouslyAsync was canceled");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInAnonymouslyAsync encountered an Error: "+task.Exception);
                return;
            }

           
            Debug.LogFormat(" User signed in successfuly: ({1})" ,Auth.CurrentUser.UserId);

            SetupRemoteConfig();
            LoadDataFromCloud();
        
        });
    }


    void SetupRemoteConfig()
    {
        Dictionary<string, object> defaults = new();
        defaults.Add("daily_reward_amount", 100);
        FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(defaults).ContinueWithOnMainThread(task => {

            FetchingRemoteConfig();
        
        
        });
    }

    void LoadDataFromCloud() {

        string userID = Auth.CurrentUser.UserId;

        databaseReference.Child("users").Child(userID).GetValueAsync().ContinueWithOnMainThread(task => {

            if (task.IsFaulted)
            {
                LoadData();
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    string json = snapshot.GetRawJsonValue();
                    data = JsonUtility.FromJson<PlayerData>(json);
                    Debug.Log("Loaded data from the Cloud");
                    UIManager.Instance.UpdateCoins(data.gold);
                }
                else
                {
                    LoadData();
                }
            }
        
        
        });
    }

    void FetchingRemoteConfig()
    {
        Debug.Log("Fetching remote config");

        FirebaseRemoteConfig.DefaultInstance.FetchAsync(System.TimeSpan.Zero).ContinueWithOnMainThread(task => {

            if (task.IsCompleted)
            {
                Debug.Log("Fetching completed successully");

                FirebaseRemoteConfig.DefaultInstance.ActivateAsync().ContinueWithOnMainThread(activateTask =>
                {
                    long reward = FirebaseRemoteConfig.DefaultInstance.GetValue("daily_reward_amount").LongValue;
                    Debug.Log("Your daily reward amount is: " + reward);
                });
            }
            else
            {
                Debug.LogError("fetch error encountered");
            }
        
        
        });
    }

    void InitFirebase()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                app = Firebase.FirebaseApp.DefaultInstance;
                Auth = FirebaseAuth.DefaultInstance;
                isFirebaseReady = true;
                AnonymousLogin();
                Debug.Log("Firebase is ready to use.");
                databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
                Debug.Log("Database Reference initialized");
                // Set a flag here to indicate whether Firebase is ready to use by your app.
            }
            else
            {
                isFirebaseReady = false;
                UnityEngine.Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void UploadDataToCloud(string data)
    {
        if (!isFirebaseReady || FirebaseAuth.DefaultInstance.CurrentUser == null) return;

        Debug.LogWarning("Uploading the cloud");
        databaseReference.Child("users").Child(userID).SetRawJsonValueAsync(data).ContinueWithOnMainThread(task => {

            if (task.IsFaulted) Debug.LogError("Cloud save failed: " + task.Exception);
            else
            {
                Debug.Log("Cloud save successful!");
            }
        });
    }
    

    public void AddCoins()
    {
        data.gold += 500;
        Save(data);
        UIManager.Instance.UpdateCoins(data.gold);
        
    }

    void Save( PlayerData playerData)
    {
        string data = JsonUtility.ToJson(playerData);

        System.IO.File.WriteAllText(Application.persistentDataPath + "/save.json",data);

        if (isFirebaseReady)
        {
            UploadDataToCloud(data);
        }
    }

    PlayerData LoadData()
    {
        if(!File.Exists(Application.persistentDataPath + "/save.json"))
        {
            return null;
        }
        PlayerData savedData =JsonUtility.FromJson<PlayerData>( System.IO.File.ReadAllText(Application.persistentDataPath + "/save.json"));
        return savedData;
    }
}
