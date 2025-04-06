using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanguageControl : MonoBehaviour
{
    public static string CheckLanguage(string turkishMessage,string englishMessage)
    {
        string language = PlayerPrefs.GetString("Language","Turkish");

        if(language == "Turkish")
        {
            return turkishMessage;
        }

        else
        {
            return englishMessage;
        }

    }
}
