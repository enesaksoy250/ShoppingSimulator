using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingImage : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 180f; // Derece/sn

    void Update()
    {
        transform.Rotate(Vector3.forward, -rotationSpeed * Time.deltaTime);
    }
}
