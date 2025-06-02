using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class User : MonoBehaviour
{

    public string nickname;
    public int watchedAdCount;
    public int gameTime;
    public string date;
    public bool removeAd;
    public int adLoadingErrorCount;
    public int level;

    public User(string nickname,int watchedAdCount,int gameTime,string date,bool removeAd,int adLoadingErrorCount,int level)
    {
        this.nickname = nickname;
        this.watchedAdCount = watchedAdCount;
        this.gameTime = gameTime;
        this.date = date;
        this.removeAd = removeAd;
        this.adLoadingErrorCount = adLoadingErrorCount;
        this.level = level;
    }


}
