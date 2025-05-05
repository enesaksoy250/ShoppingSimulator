using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class LanguageManager : MonoBehaviour
{
    public static LanguageManager instance;
    private Dictionary<string, string> localizedText;
    public string currentLanguage = "English"; // Varsayýlan dil

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SetLanguage();
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
        LoadTextFileFromResources(language);
    }

    private void LoadTextFileFromResources(string language)
    {
        TextAsset textFile = Resources.Load<TextAsset>("ShoppingSimulatorLanguage"); // .txt uzantýsý olmadan

        if (textFile != null)
        {
            ProcessTextData(textFile.text, language);
        }
        else
        {
            Debug.LogError("Localization text file not found in Resources!");
        }
    }

    private void ProcessTextData(string fileContent, string language)
    {
        string[] lines = fileContent.Split('\n');
        if (lines.Length == 0) return;

        string[] headers = lines[0].Trim().Split(';');

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
            Debug.LogError($"Language '{language}' not found in text file headers.");
            return;
        }

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] row = lines[i].Trim().Split(';');
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

    public void SetLanguage()
    {
        currentLanguage = PlayerPrefs.GetString("Language", "Turkish");
    }

    public static string GetLanguage()
    {
        return PlayerPrefs.GetString("Language", "English");
    }


}
