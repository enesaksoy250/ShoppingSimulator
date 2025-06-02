using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReputationDisplay : MonoBehaviour
{

    public static ReputationDisplay Instance;

    [SerializeField] private Image reputationFill;

    public float transitionSpeed = 2f;

    private float targetFill;
    private float currentFill;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {  
        ReputationManager.instance.OnReputationChanged += UpdateFill;
    }

    private void OnDestroy()
    {
        ReputationManager.instance.OnReputationChanged -= UpdateFill;
    }

    public void UpdateFill(float reputation,bool isInitial)
    {
        targetFill = Mathf.Clamp01(reputation / 100f);
        StopAllCoroutines();
        StartCoroutine(SmoothFill(isInitial));
    }

    private IEnumerator SmoothFill(bool isInitial)
    {

        float oldFill = currentFill;
        float direction = targetFill - oldFill;

        if (!isInitial)
        {

            Color originalColor = reputationFill.color;
            Color flashColor = direction > 0 ? Color.green : Color.red;
            float flashDuration = 1;
            float elapsed = 0f;

            while (elapsed < flashDuration)
            {
                float t = elapsed / flashDuration;
                reputationFill.color = Color.Lerp(originalColor, flashColor, t);
                elapsed += Time.deltaTime;
                yield return null;
            }

            reputationFill.color = flashColor;

            elapsed = 0f;
            while (elapsed < flashDuration)
            {
                float t = elapsed / flashDuration;
                reputationFill.color = Color.Lerp(flashColor, originalColor, t);
                elapsed += Time.deltaTime;
                yield return null;
            }

            reputationFill.color = originalColor;

        }


        while (Mathf.Abs(currentFill - targetFill) > 0.01f)
        {
            currentFill = Mathf.Lerp(currentFill, targetFill, Time.deltaTime * transitionSpeed);
            reputationFill.fillAmount = currentFill;


            Color fillColor = Color.Lerp(Color.red, Color.yellow, currentFill * 2f);
            if (currentFill > 0.5f)
                fillColor = Color.Lerp(Color.yellow, Color.green, (currentFill - 0.5f) * 2f);

            reputationFill.color = fillColor;

            yield return null;
        }

           currentFill = targetFill;
           reputationFill.fillAmount = currentFill;
    }

}
