using System;
using System.IO;
using System.Text;
using UnityEngine;

public class EncodingConverter : MonoBehaviour
{
    // CSV dosyas�n�n Resources i�indeki giri� ve ��k�� yollar�
    public string inputFileName = "ShoppingSimulatorLanguage(Sayfa1)"; // Resources i�inde dosya ad� (uzant� yok)
    public string outputFileName = "ShoppingSimulatorLanguage_utf8"; // ��k�� dosyas� (uzant� yok)

    // Start metodunda dosya encoding d�n���m�n� yapaca��z
    void Start()
    {
        // Dosyay� Resources'tan oku ve d�n���m i�lemi yap
        ConvertFileEncoding(inputFileName, outputFileName);
    }

    // Dosya encoding d�n���m�n� ger�ekle�tiren fonksiyon
    void ConvertFileEncoding(string inputFileName, string outputFileName)
    {
        try
        {
            // Resources'tan dosyay� oku (dosya ismini belirtirken uzant� kullanma)
            TextAsset csvFile = Resources.Load<TextAsset>(inputFileName);

            if (csvFile != null)
            {
                // CSV i�eri�ini Windows-1254 encoding ile oku
                string content = csvFile.text;

                // Resources'a kaydedemeyiz ama ��k�� dosyas�n� oyun klas�r�nde kaydedebiliriz
                string outputPath = Path.Combine(Application.dataPath, "Resources", outputFileName + ".csv");

                // UTF-8 encoding ile ��k�� dosyas�na yaz
                File.WriteAllText(outputPath, content, Encoding.UTF8);

                Debug.Log($"Dosya ba�ar�yla {outputPath} olarak UTF-8 format�nda kaydedildi.");
            }
            else
            {
                Debug.LogError("CSV dosyas� Resources i�inde bulunamad�.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Hata: {ex.Message}");
        }
    }
}
