using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GoogleLoginButton : MonoBehaviour
{
    Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }
    void Start()
    {
        button.onClick.AddListener(Login);
    }

    private void Login()
    {
        LoginWithGoogle.instance.LoginWithGoogleAndCheckDatabase();
    }
        
}
