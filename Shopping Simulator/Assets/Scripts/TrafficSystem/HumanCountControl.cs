using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanCountControl : MonoBehaviour
{
    public static HumanCountControl Instance;

    public int numberOfHuman;

    [SerializeField] private int maxHumanCount;

    private void Awake()
    {
        Instance = this;
    }

    public bool HumanSpawn()
    {
        return numberOfHuman < maxHumanCount;
    }

    public void IncreaseHumanCount()
    {
        numberOfHuman++;
    }

    public void DecreaseHumanCount()
    {
        numberOfHuman--;
    }
}
