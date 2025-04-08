using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SetFonts : MonoBehaviour
{
    public TMP_FontAsset newFont;

    [ContextMenu("Replace All Fonts")]
    public void ReplaceAllFontsInScene()
    {
        if (newFont == null)
        {
            Debug.LogError("Yeni font atanmadý!");
            return;
        }

        int count = 0;

        // Tüm UI TextMeshPro'lar (aktif ve pasif)
        TextMeshProUGUI[] uiTexts = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
        foreach (var tmp in uiTexts)
        {
            if (tmp.gameObject.scene.isLoaded) // Sadece sahnede yüklü olanlar
            {
                tmp.font = newFont;
                count++;
            }
        }

        // Tüm 3D TextMeshPro'lar (aktif ve pasif)
        TextMeshPro[] worldTexts = Resources.FindObjectsOfTypeAll<TextMeshPro>();
        foreach (var tmp in worldTexts)
        {
            if (tmp.gameObject.scene.isLoaded)
            {
                tmp.font = newFont;
                count++;
            }
        }

        Debug.Log($"{count} adet TextMeshPro bileþenine yeni font atandý.");
    }
}
