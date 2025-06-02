using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficLightManager : MonoBehaviour
{
    [Header("Kavşaktaki üç ışık")]
    [SerializeField] private List<TrafficLight> lights;

    [Header("Yeşil ışık süresi (saniye)")]
    [SerializeField] private float greenDuration = 10f;
    [Header("Sarı ışık süresi (saniye)")]
    [SerializeField] private float yellowDuration = 3f;

    // Faz: 0 = Green, 1 = Yellow
    private int phase = 0;
    private int currentIndex = 0;
    private float timer = 0f;

    void Start()
    {
        if (lights == null || lights.Count == 0)
        {
            Debug.LogError("TrafficLightManager: ışık listesi boş!");
            enabled = false;
            return;
        }

        // Hepsini kırmızı yap
        foreach (var light in lights)
            light.SetState(TrafficLight.LightState.Red);

        // İlk ışığı yeşile koy
        lights[currentIndex].SetState(TrafficLight.LightState.Green);
        phase = 0;
        timer = 0f;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (phase == 0)
        {
            // GREEN aşaması
            if (timer < greenDuration) return;

            // Green süresi bitti → Yellow
            lights[currentIndex].SetState(TrafficLight.LightState.Yellow);
            phase = 1;
            timer = 0f;
        }
        else if (phase == 1)
        {
            // YELLOW aşaması
            if (timer < yellowDuration) return;

            // Yellow bitti → Red yap ve sonraki ışığa geç
            lights[currentIndex].SetState(TrafficLight.LightState.Red);

            currentIndex = (currentIndex + 1) % lights.Count;

            lights[currentIndex].SetState(TrafficLight.LightState.Green);
            phase = 0;
            timer = 0f;
        }
    }
}
