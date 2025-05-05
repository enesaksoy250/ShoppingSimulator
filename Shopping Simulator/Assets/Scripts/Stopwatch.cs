using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stopwatch : MonoBehaviour
{
    
    public static Stopwatch Instance;


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
        StartCoroutine(UpdateGameTime());
        StartCoroutine(ShowAd());
    }

    IEnumerator UpdateGameTime()
    {
        while (true)
        {
            yield return new WaitForSeconds(60);
            DatabaseManager.Instance.IncreaseFirebaseInfo("gameTime", 1);           
        }
    }

    IEnumerator ShowAd()
    {
        while (true)
        {
            yield return new WaitForSeconds(120);
            AdManager.instance.ShowInterstitialAd();
        }
    }

}
