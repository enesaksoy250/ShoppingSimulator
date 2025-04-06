using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanguageManager : MonoBehaviour
{
    public static LanguageManager instance;
    private Dictionary<string, string> localizedText;
    private string currentLanguage;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            GetLanguage();
            LoadLocalizedText(currentLanguage);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadLocalizedText(string language)
    {
        localizedText = new Dictionary<string, string>();
        LoadCSVFromResources(language);
    }

    private void LoadCSVFromResources(string language)
    {

        TextAsset csvFile = Resources.Load<TextAsset>("ShoppingSimulatorLanguage(Sayfa1)");

        if (csvFile != null)
        {
            ProcessCSVData(csvFile.text, language);
        }
        else
        {
            Debug.LogError("Localization CSV file not found in Resources!");
        }
    }

    private void ProcessCSVData(string csvData, string language)
    {
        string[] data = csvData.Split('\n');
        string[] headers = data[0].Split(';');

        int languageIndex = -1;
        for (int i = 0; i < headers.Length; i++)
        {
            if (headers[i].Trim() == language)
            {
                languageIndex = i;
                break;
            }
        }

        if (languageIndex == -1)
        {
            Debug.LogError($"Language '{language}' not found in CSV file");
            return;
        }

        for (int i = 1; i < data.Length; i++)
        {
            string[] row = data[i].Split(';');
            if (row.Length > languageIndex)
            {
                string key = row[0].Trim();
                string value = row[languageIndex].Trim();

                if (!localizedText.ContainsKey(key))
                {
                    localizedText.Add(key, value);
                }
            }
        }
    }

    public string GetLocalizedValue(string key)
    {
        if (localizedText.ContainsKey(key))
        {
            return localizedText[key];
        }
        else
        {
            Debug.LogWarning($"Localized text not found for key: {key}");
            return key;
        }
    }

    private void GetLanguage()
    {
       
        currentLanguage = PlayerPrefs.GetString("Language","Turkish");
    }


}