using CryingSnow.CheckoutFrenzy;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReputationManager : MonoBehaviour
{
    public static ReputationManager instance;

    public float reputation => reputationData.reputation;


    //public int totalCustomers;
    //public int satisfiedCustomers;

    private ReputationData reputationData;

    public Action<float,bool> OnReputationChanged;

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
        ReputationDisplay.Instance.UpdateFill(reputationData.reputation, true);
    }

    public void RegisterCustomerFeedback(bool isSatisfied)
    {
     
        reputationData.totalCustomers++;

        if (isSatisfied)
            reputationData.satisfiedCustomers++;

        reputationData.reputation = (float)reputationData.satisfiedCustomers / reputationData.totalCustomers * 100f; // Örn. yüzde üzerinden hesap

        //SaveReputation();

        OnReputationChanged?.Invoke(reputationData.reputation,false);
    }

    public void SaveReputation()
    {       
        SaveSystem.SaveData(reputationData, "reputationData");
    }

    private void LoadReputation()
    {
         reputationData = SaveSystem.LoadData<ReputationData>("reputationData");

        if (reputationData == null)
        {
            reputationData = new ReputationData();
            reputationData.Inititalize();
        }
      
    }



   
}
