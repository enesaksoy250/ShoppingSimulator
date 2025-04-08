using System;
using System.IO;
using System.Text;
using UnityEngine;

public class EncodingConverter : MonoBehaviour
{
    // CSV dosyasýnýn Resources içindeki giriþ ve çýkýþ yollarý
    public string inputFileName = "ShoppingSimulatorLanguage(Sayfa1)"; // Resources içinde dosya adý (uzantý yok)
    public string outputFileName = "ShoppingSimulatorLanguage_utf8"; // Çýkýþ dosyasý (uzantý yok)

    // Start metodunda dosya encoding dönüþümünü yapacaðýz
    void Start()
    {
        // Dosyayý Resources'tan oku ve dönüþüm iþlemi yap
        ConvertFileEncoding(inputFileName, outputFileName);
    }

    // Dosya encoding dönüþümünü gerçekleþtiren fonksiyon
    void ConvertFileEncoding(string inputFileName, string outputFileName)
    {
        try
        {
            // Resources'tan dosyayý oku (dosya ismini belirtirken uzantý kullanma)
            TextAsset csvFile = Resources.Load<TextAsset>(inputFileName);

            if (csvFile != null)
            {
                // CSV içeriðini Windows-1254 encoding ile oku
                string content = csvFile.text;

                // Resources'a kaydedemeyiz ama çýkýþ dosyasýný oyun klasöründe kaydedebiliriz
                string outputPath = Path.Combine(Application.dataPath, "Resources", outputFileName + ".csv");

                // UTF-8 encoding ile çýkýþ dosyasýna yaz
                File.WriteAllText(outputPath, content, Encoding.UTF8);

                Debug.Log($"Dosya baþarýyla {outputPath} olarak UTF-8 formatýnda kaydedildi.");
            }
            else
            {
                Debug.LogError("CSV dosyasý Resources içinde bulunamadý.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Hata: {ex.Message}");
        }
    }
}
