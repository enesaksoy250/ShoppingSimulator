using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OpenLink : MonoBehaviour
{

    Button button;
    [SerializeField] string link;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(delegate { OpenLinks(link); });
    }

   
    private void OpenLinks(string url)
    {
        Application.OpenURL($"{url}");
    }
}
