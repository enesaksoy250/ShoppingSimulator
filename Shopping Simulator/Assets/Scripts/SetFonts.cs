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
            Debug.LogError("Yeni font atanmad�!");
            return;
        }

        int count = 0;

        // T�m UI TextMeshPro'lar (aktif ve pasif)
        TextMeshProUGUI[] uiTexts = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
        foreach (var tmp in uiTexts)
        {
            if (tmp.gameObject.scene.isLoaded) // Sadece sahnede y�kl� olanlar
            {
                tmp.font = newFont;
                count++;
            }
        }

        // T�m 3D TextMeshPro'lar (aktif ve pasif)
        TextMeshPro[] worldTexts = Resources.FindObjectsOfTypeAll<TextMeshPro>();
        foreach (var tmp in worldTexts)
        {
            if (tmp.gameObject.scene.isLoaded)
            {
                tmp.font = newFont;
                count++;
            }
        }

        Debug.Log($"{count} adet TextMeshPro bile�enine yeni font atand�.");
    }
}
