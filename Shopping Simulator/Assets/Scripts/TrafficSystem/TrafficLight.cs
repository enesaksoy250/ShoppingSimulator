using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficLight : MonoBehaviour
{
    public enum LightState { Red, Yellow, Green }
    public LightState currentState = LightState.Red;
    //public float GreenDuration = 10f;
    //public float RedDuration = 10f;

    public bool IsGreen => currentState == LightState.Green;

    private float _timer;

    void Update()
    {
       /* _timer += Time.deltaTime;
        if (currentState == LightState.Red && _timer >= RedDuration)
        {
            currentState = LightState.Green;
            _timer = 0f;
        }
        else if (currentState == LightState.Green && _timer >= GreenDuration)
        {
            currentState = LightState.Red;
            _timer = 0f;
        } */
    }

    public void SetState(LightState newState)
    {
        currentState = newState;
        ApplyState(newState);
    }

    private void ApplyState(LightState state)
    {
        // Iþýk objelerinizde emission veya material deðiþikliði varsa burada kontrol edin:
        bool isGreen = state == LightState.Green;
       /* if (greenLightRenderer != null)
            greenLightRenderer.material.SetColor(EmissionColor, isGreen ? Color.green : Color.black);
        if (redLightRenderer != null)
            redLightRenderer.material.SetColor(EmissionColor, isGreen ? Color.black : Color.red); */
    }

}
