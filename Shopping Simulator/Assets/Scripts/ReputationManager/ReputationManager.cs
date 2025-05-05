using CryingSnow.CheckoutFrenzy;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReputationManager : MonoBehaviour
{
    public static ReputationManager instance;

    public float reputation;
    public int totalCustomers;
    public int satisfiedCustomers;

    public Action<float> OnReputationChanged;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            LoadReputation();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        ReputationDisplay.Instance.UpdateFill(reputation);
    }

    public void RegisterCustomerFeedback(bool isSatisfied)
    {
        totalCustomers++;

        if (isSatisfied)
            satisfiedCustomers++;

        reputation = (float)satisfiedCustomers / totalCustomers * 100f; // Örn. yüzde üzerinden hesap

        SaveReputation();

        OnReputationChanged?.Invoke(reputation);
    }

    private void SaveReputation()
    {
        ReputationData data = new ReputationData
        {
            reputation = reputation,
            totalCustomers = totalCustomers,
            satisfiedCustomers = satisfiedCustomers
        };

        SaveSystem.SaveData(data, "reputationData");
    }

    private void LoadReputation()
    {
        ReputationData data = SaveSystem.LoadData<ReputationData>("reputationData");

        if (data != null)
        {
            reputation = data.reputation;                
            totalCustomers = data.totalCustomers;      
            satisfiedCustomers = data.satisfiedCustomers;
        }
        else
        {
            reputation = 50f;
            totalCustomers = 100;
            satisfiedCustomers = 50;
        }
    }
}
