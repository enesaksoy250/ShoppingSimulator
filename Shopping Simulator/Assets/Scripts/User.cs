using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class User : MonoBehaviour
{

    public string username;
    public int watchedAdCount;
    public int gameTime;
    public string date;

    public User(string username,int watchedAdCount,int gameTime,string date)
    {
        this.username = username;
        this.watchedAdCount = watchedAdCount;
        this.gameTime = gameTime;
        this.date = date;
    }


}
