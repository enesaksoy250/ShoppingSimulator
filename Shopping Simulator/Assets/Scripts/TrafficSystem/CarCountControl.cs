using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarCountControl : MonoBehaviour
{
    public static CarCountControl Instance;

    public int numberOfCars=0;

    [SerializeField] private int maxCars;

    private void Awake()
    {
        Instance = this;
    }

    public bool CarSpawn()
    {
        return numberOfCars < maxCars;
    }

    public void IncreaseCarNumber()
    {
        numberOfCars++;
    }

    public void DecreaseCarNumber()
    {
        numberOfCars--;
    }

}
