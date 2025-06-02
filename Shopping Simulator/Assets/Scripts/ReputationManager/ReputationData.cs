using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ReputationData
{
    public float reputation;
    public int totalCustomers;
    public int satisfiedCustomers;

   
   public void Inititalize()
   {
        reputation = 50;
        totalCustomers = 100;
        satisfiedCustomers = 50;
   }
}
