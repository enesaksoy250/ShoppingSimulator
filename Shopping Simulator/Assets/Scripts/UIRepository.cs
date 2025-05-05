using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIRepository : MonoBehaviour
{

    public static UIRepository Instance;

    [SerializeField] private GameObject userPanel;
    [SerializeField] private Transform parentTransform;
    [SerializeField] private GameObject loadingImage;
 
    public GameObject UserPanel => userPanel;
    public Transform ParentTransform => parentTransform;
    public GameObject LoadingImage => loadingImage;

    private void Awake()
    {
        Instance = this;
    }

   
}
