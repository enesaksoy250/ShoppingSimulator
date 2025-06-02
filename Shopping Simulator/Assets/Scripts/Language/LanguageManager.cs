using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;


public class LanguageManager : MonoBehaviour
{
    public static LanguageManager instance;
    private Dictionary<string, string> localizedText;
    public string currentLanguage = "English"; // Varsayılan dil
    public int selectedIndex; 

    private readonly static List<string> avaliableLanguages = new () { "English","Türkçe", "Deutsch", "Español", "Italiano", "Français", "Português"};

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

    private void Start()
    {
        selectedIndex = GetCurrentLanguageIndex();
    }

    public void LoadLocalizedText(string language)
    {
        localizedText = new Dictionary<string, string>();
        LoadTextFileFromResources(language);
    }

    private void LoadTextFileFromResources(string language)
    {
        TextAsset textFile = Resources.Load<TextAsset>("ShoppingSimulatorLanguage"); // .txt uzantısı olmadan

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

    public  string GetLocalizedValue(string key)
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
        currentLanguage = PlayerPrefs.GetString("Language", "English");

        if (currentLanguage == "Turkish")
            currentLanguage = "Türkçe";

    }

    public static string GetLanguage()
    {
        string language = PlayerPrefs.GetString("Language", "English");

        if (language == "Turkish")
            language = "Türkçe";

        return language;
    }

    public static int GetCurrentLanguageIndex()
    {
       return avaliableLanguages.IndexOf(GetLanguage());
    }

    public static void SetLanguagePanel()
    {
        UIRepository.Instance.LanguageText.text = avaliableLanguages[GetCurrentLanguageIndex()];
    }

    public string GetSelectedLanguage()
    {
        return avaliableLanguages[selectedIndex];
    }
    public static int GetNumberOfLanguage()
    {
        return avaliableLanguages.Count;
    }
}
