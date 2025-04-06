using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UILanguageManager : MonoBehaviour
{

    TextMeshProUGUI UIText;


    private void Awake()
    {

        UIText = GetComponent<TextMeshProUGUI>();

    }

    private void Start()
    {

        UIText.text = LanguageManager.instance.GetLocalizedValue(gameObject.name);
    }

    public void UpdateText()
    {


        UIText.text = LanguageManager.instance.GetLocalizedValue(gameObject.name);

    }

}