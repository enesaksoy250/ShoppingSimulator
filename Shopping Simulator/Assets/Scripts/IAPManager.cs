using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing;
using System;
using CryingSnow.CheckoutFrenzy;

public class IAPManager : MonoBehaviour, IStoreListener
{
    IStoreController controller;

    public string[] product;

 

    private void Start()
    {
        IAPStart();
    }

    public void IAPStart()
    {
        var module = StandardPurchasingModule.Instance();
        ConfigurationBuilder builder = ConfigurationBuilder.Instance(module);
        foreach (string item in product)
        {
            builder.AddProduct(item, ProductType.Consumable);
        }
        UnityPurchasing.Initialize(this, builder);
    }


    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.Log("OnInitializeFailed: " + error);
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.Log("OnInitializeFailed: " + error + " message = " + message);
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
    {
        if (string.Equals(purchaseEvent.purchasedProduct.definition.id, product[0], StringComparison.Ordinal))
        {
            PlayerPrefs.SetInt("RemoveAd", 1);
            GamePanelManager.Instance.LoadPanel("PaymentCompletedPanel");
            DatabaseManager.Instance.UpdateFirebaseInfo("removeAd", true);
            return PurchaseProcessingResult.Complete;
        }
        if (string.Equals(purchaseEvent.purchasedProduct.definition.id, product[1], StringComparison.Ordinal))
        {
            DataManager.Instance.PlayerMoney += 5000;
            GamePanelManager.Instance.LoadPanel("PaymentCompletedPanel");
            return PurchaseProcessingResult.Complete;
        }
        if (string.Equals(purchaseEvent.purchasedProduct.definition.id, product[2], StringComparison.Ordinal))
        {
            DataManager.Instance.PlayerMoney += 10000;
            GamePanelManager.Instance.LoadPanel("PaymentCompletedPanel");
            return PurchaseProcessingResult.Complete;
        }
        if (string.Equals(purchaseEvent.purchasedProduct.definition.id, product[3], StringComparison.Ordinal))
        {
            DataManager.Instance.PlayerMoney += 25000;
            GamePanelManager.Instance.LoadPanel("PaymentCompletedPanel");
            return PurchaseProcessingResult.Complete;
        }
        if (string.Equals(purchaseEvent.purchasedProduct.definition.id, product[4], StringComparison.Ordinal))
        {
            DataManager.Instance.PlayerMoney += 50000;
            GamePanelManager.Instance.LoadPanel("PaymentCompletedPanel");
            return PurchaseProcessingResult.Complete;
        }
       
        else
        {
            return PurchaseProcessingResult.Pending; 
        }
    }

    public void OnPurchaseFailed(UnityEngine.Purchasing.Product product, PurchaseFailureReason failureReason)
    {
        Debug.Log("OnPurchaseFailed: " + failureReason);
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        this.controller = controller;
    }

    public void IAPButton(string id)
    {
       UnityEngine.Purchasing.Product product = controller.products.WithID(id);
        if (product != null && product.availableToPurchase)
        {
            controller.InitiatePurchase(product);
            Debug.Log("Buying: " + id);
        }
        else
        {
            Debug.Log("Not Buying: " + id);
        }
    }

   
}
