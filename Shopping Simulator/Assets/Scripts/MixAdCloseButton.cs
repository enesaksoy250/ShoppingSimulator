using CryingSnow.CheckoutFrenzy;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MixAdCloseButton : MonoBehaviour
{
    Button closeButton;

    private void Awake()
    {
        closeButton = GetComponent<Button>();
    }

    void Start()
    {
        closeButton.onClick.AddListener(CloseAdPanel);
    }

    private void CloseAdPanel()
    {
        DataManager.Instance.StopCoroutine(DataManager.Instance.startAdProcessCoroutine);
        DataManager.Instance.startAdProcessCoroutine = null;
        GamePanelManager.instance.ClosePanel("MixAdPanel");
    }

}
