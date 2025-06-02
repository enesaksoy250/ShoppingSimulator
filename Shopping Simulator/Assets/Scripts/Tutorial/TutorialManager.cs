using CryingSnow.CheckoutFrenzy;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{

   
    public Camera tutorialCamera;
    public Button nextButton;
    public GameObject infoPanel;
    public TextMeshProUGUI infoText;
    public List<TutorialStep> steps;
  
    private GameObject currentArrow;
    private int currentStepIndex = -1;

    TutorialStep step;

    void Start()
    {
        if (!PlayerPrefs.HasKey("TutorialEnd") && DataManager.Instance.Data.CurrentLevel == 1)
        {         
            tutorialCamera.enabled = true;
            infoPanel.SetActive(false);
            nextButton.onClick.AddListener(NextStep);
            ShowNextStep();
        }
    }

    private void Update()
    {
       //StartCoroutine(MoveCameraTo(step));
    }

  
    void ShowNextStep()
    {
        if (currentStepIndex >= 0)
        {
            infoPanel.SetActive(false);
            if (currentArrow) Destroy(currentArrow);
        }

        currentStepIndex++;
        if (currentStepIndex >= steps.Count)
        {
            EndTutorial();
            return;
        }

        step = steps[currentStepIndex];
        
        step.onStepEnter?.Invoke();
        StopAllCoroutines();
        StartCoroutine(MoveCameraTo(step));
      
     
        infoPanel.SetActive(true);
       
        Transform parent = infoPanel.transform;
        GameObject child = parent.GetChild(0).gameObject;
        child.GetComponent<RectTransform>().anchoredPosition = step.infoPanelPosition;
    
        infoText.text = step.Description;
        nextButton.interactable = true;
    }

    public void NextStep()
    {
        ShowNextStep();
    }


    IEnumerator MoveCameraTo(TutorialStep step)
    {
        Vector3 startPos = tutorialCamera.transform.position;
        Quaternion startRot = tutorialCamera.transform.rotation;
        Vector3 startEuler = startRot.eulerAngles;
        float startY = startEuler.y;

        Vector3 targetPos = step.targetTransform.position + step.cameraOffset;
        float targetY = Quaternion.LookRotation(step.targetTransform.position - targetPos, Vector3.up).eulerAngles.y;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime;
            float f = Mathf.SmoothStep(0f, 1f, t);
            tutorialCamera.transform.position = Vector3.Lerp(startPos, targetPos, f);
            float newY = Mathf.LerpAngle(startY, targetY, f);
            tutorialCamera.transform.rotation = Quaternion.Euler(startEuler.x, newY, startEuler.z);
            yield return null;
        }
    }

   
    void EndTutorial()
    {
        tutorialCamera.enabled = false;
        infoPanel.SetActive(false);
        if (currentArrow) Destroy(currentArrow);
        nextButton.gameObject.SetActive(false);
        PlayerPrefs.SetInt("TutorialEnd", 1);
        Debug.Log("Tutorial tamamlandý.");
        StartCoroutine(ShowGoogleLoginPanel());
    }

    private IEnumerator ShowGoogleLoginPanel()
    {
        yield return new WaitForSeconds(3);
        LoginWithGoogle.instance.LoginWithGoogleAndCheckDatabase();
    }

}




