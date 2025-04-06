using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stopwatch : MonoBehaviour
{
    // Start is called before the first frame update
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
            yield return new WaitForSeconds(300);
            AdManager.instance.ShowInterstitialAd();
        }
    }

}
