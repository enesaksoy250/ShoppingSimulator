using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIRepository : MonoBehaviour
{

    public static UIRepository Instance;

    [SerializeField] private GameObject userPanel;
    [SerializeField] private GameObject headerPanel;
    [SerializeField] private Transform parentTransform;
    [SerializeField] private GameObject loadingImage;
    [SerializeField] private TextMeshProUGUI languageText;
    [SerializeField] private TextMeshProUGUI settingsLanguageText;
    [SerializeField] private TMP_InputField nicknameIF;
 
    public GameObject UserPanel => userPanel;
    public GameObject HeaderPanel => headerPanel;
    public Transform ParentTransform => parentTransform;
    public GameObject LoadingImage => loadingImage;
    public TextMeshProUGUI LanguageText => languageText;
    public TextMeshProUGUI SettingsLanguageText => settingsLanguageText;
    public TMP_InputField NicknameIF => nicknameIF;

    private void Awake()
    {
        Instance = this;
    }

   
}
