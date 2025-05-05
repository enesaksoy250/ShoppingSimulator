using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowPointer : MonoBehaviour
{
    public RectTransform arrowRect;
    public Transform target;
    public float offset = 50f;

    public void SetTarget(Transform t)
    {
        target = t;
    }

    void Update()
    {
        if (target == null) return;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(target.position);
        arrowRect.position = screenPos + Vector3.up * offset;

    }
}
