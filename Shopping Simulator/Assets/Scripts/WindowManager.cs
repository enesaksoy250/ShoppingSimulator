using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class WindowManager : MonoBehaviour, IPointerClickHandler
{

    [SerializeField] private RectTransform mainPanel;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (RectTransformUtility.RectangleContainsScreenPoint(mainPanel, eventData.position))
        {
            return;
        }

        gameObject.SetActive(false);

    }


}
