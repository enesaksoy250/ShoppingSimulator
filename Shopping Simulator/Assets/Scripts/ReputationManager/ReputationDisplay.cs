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

    public void UpdateFill(float reputation)
    {
        targetFill = Mathf.Clamp01(reputation / 100f);
        StopAllCoroutines();
        StartCoroutine(SmoothFill());
    }

    private IEnumerator SmoothFill()
    {
        while (Mathf.Abs(currentFill - targetFill) > 0.01f)
        {
            currentFill = Mathf.Lerp(currentFill, targetFill, Time.deltaTime * transitionSpeed);
            reputationFill.fillAmount = currentFill;

            // Renk geçiþi
            Color color = Color.Lerp(Color.red, Color.yellow, currentFill * 2);
            if (currentFill > 0.5f)
                color = Color.Lerp(Color.yellow, Color.green, (currentFill - 0.5f) * 2);
            reputationFill.color = color;

            yield return null;
        }

        currentFill = targetFill;
        reputationFill.fillAmount = currentFill;
    }
}
