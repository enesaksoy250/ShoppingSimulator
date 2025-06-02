using CryingSnow.CheckoutFrenzy;
using GoogleMobileAds.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AdManager : MonoBehaviour
{
   
    public static AdManager instance;

    private InterstitialAd _interstitialAd;
    //private const string adInterstitialUnitId= "ca-app-pub-3940256099942544/1033173712";
    private const string adInterstitialUnitId= "ca-app-pub-2684276866838299/5604426296";

    private RewardedAd _rewardedAd;
    //private const string adRewardUnitId = "ca-app-pub-3940256099942544/5224354917";
    private const string adRewardUnitId = "ca-app-pub-2684276866838299/9928908011";

    private RewardedInterstitialAd _rewardedInterstitialAd;
    //private const string adRewardedInterstitialUnitId= "ca-app-pub-3940256099942544/5354046379";
    private const string adRewardedInterstitialUnitId= "ca-app-pub-2684276866838299/2849557408";

    public event Action OnAdCoroutineFinished;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            MobileAds.RaiseAdEventsOnUnityMainThread = true;
        }

        else
        {
            Destroy(gameObject);    
        }
    }

    void Start()
    {
        MobileAds.Initialize(initStatus => { if (PlayerPrefs.GetInt("RemoveAd",0) != 1) { StartCoroutine(LoadAd()); } 
            LoadRewardedInterstitialAd(); });
    }

  

    private IEnumerator LoadAd()
    {
        yield return new WaitForSeconds(60);
        LoadInterstitialAd();
    }

    public void LoadInterstitialAd()
    {

        if (_interstitialAd != null)
        {
            _interstitialAd.Destroy();
            _interstitialAd = null;
        }

        var adRequest = new AdRequest();


        InterstitialAd.Load(adInterstitialUnitId, adRequest,
        (InterstitialAd ad, LoadAdError error) =>
        {

            if (error != null || ad == null)
            {
                Debug.LogError("interstitial ad failed to load an ad " +
                               "with error : " + error);
                if (CheckInternetConnection.GetInternetConnection())
                {
                    DatabaseManager.Instance.IncreaseFirebaseInfo("adLoadingErrorCount", 1);
                }
            
                return;
            }

            Debug.Log("Interstitial ad loaded with response : "
                      + ad.GetResponseInfo());

            _interstitialAd = ad;
            RegisterEventHandlers(_interstitialAd);

        });

       

    }

    public void ShowInterstitialAd()
    {
        if (_interstitialAd != null && _interstitialAd.CanShowAd())
        {
            Debug.Log("Showing interstitial ad.");
            _interstitialAd.Show();
        }
        else
        {
            Debug.LogError("Interstitial ad is not ready yet.");
            LoadInterstitialAd();
        }
    }

    private void RegisterEventHandlers(InterstitialAd interstitialAd)
    {

        interstitialAd.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("Interstitial ad paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };

        interstitialAd.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Interstitial ad recorded an impression.");
        };

        interstitialAd.OnAdClicked += () =>
        {
            Debug.Log("Interstitial ad was clicked.");
        };

        interstitialAd.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Interstitial ad full screen content opened.");
        };

        interstitialAd.OnAdFullScreenContentClosed += () =>
        {

            Debug.Log("Interstitial ad full screen content closed.");
            LoadInterstitialAd();
            DatabaseManager.Instance.IncreaseFirebaseInfo("watchedAdCount", 1);

            int totalAd = PlayerPrefs.GetInt("TotalAd",0);
            totalAd++;
          
            if (totalAd % 10 == 0)
            {
                GamePanelManager.instance.LoadPanel("RemoveAdPanel");
                PlayerPrefs.SetInt("TotalAd",totalAd);
            }

        };

        interstitialAd.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Interstitial ad failed to open full screen content " +
                           "with error : " + error);
            LoadInterstitialAd();
        };
    }

    public void LoadRewardedAd()
    {

        if (_rewardedAd != null)
        {
            _rewardedAd.Destroy();
            _rewardedAd = null;
        }

        Debug.Log("Loading the rewarded ad."); 

        var adRequest = new AdRequest();

        RewardedAd.Load(adRewardUnitId, adRequest,
            (RewardedAd ad, LoadAdError error) =>
            {

                if (error != null || ad == null)
                {
                    Debug.LogError("Rewarded ad failed to load an ad " +
                                   "with error : " + error);

                    GamePanelManager.instance.ClosePanel("LoadingPanel");
                    GamePanelManager.instance.LoadPanel("ErrorPanel");
                    //Invoke(nameof(CloseErrorAdPanel), 2);
                    return;
                }

                Debug.Log("Rewarded ad loaded with response : "
                          + ad.GetResponseInfo());

                _rewardedAd = ad;


                if (_rewardedAd != null)
                {
                    GamePanelManager.instance.ClosePanel("LoadingPanel");
                    ShowRewardedAd();
                }



            });
    }

    public void ShowRewardedAd()
    {

        const string rewardMsg = "Rewarded ad rewarded the user. Type: {0}, amount: {1}.";


        if (_rewardedAd != null && _rewardedAd.CanShowAd())
        {
            _rewardedAd.Show((Reward reward) =>
            {
                Debug.Log(String.Format(rewardMsg, reward.Type, reward.Amount));

            });

            RegisterReloadHandler(_rewardedAd);
        }

        else
        {

            GamePanelManager.instance.LoadPanel("ErrorPanel");


        }
    }

    private void RegisterReloadHandler(RewardedAd ad)
    {

        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Rewarded Ad full screen content closed.");

            StartCoroutine(HandleAdClosed());

        };

        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Rewarded ad failed to open full screen content " +
                           "with error : " + error);


        };
    }

    private IEnumerator HandleAdClosed()
    {
        yield return null;
        GivePlayerReward();
        DatabaseManager.Instance.IncreaseFirebaseInfo("watchedAdCount", 1);
    }

    private void GivePlayerReward()
    {
        GamePanelManager.instance.LoadPanel("RewardGivenPanel");
        DataManager.Instance.PlayerMoney += 100;
    }

    public void LoadRewardedInterstitialAd()
    {
        // Clean up the old ad before loading a new one.
        if (_rewardedInterstitialAd != null)
        {
            _rewardedInterstitialAd.Destroy();
            _rewardedInterstitialAd = null;
        }

        Debug.Log("Loading the rewarded interstitial ad.");

        // create our request used to load the ad.
        var adRequest = new AdRequest();
        adRequest.Keywords.Add("unity-admob-sample");

        // send the request to load the ad.
        RewardedInterstitialAd.Load(adRewardedInterstitialUnitId, adRequest,
            (RewardedInterstitialAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    Debug.LogError("rewarded interstitial ad failed to load an ad " +
                                   "with error : " + error);
                    return;
                }

                Debug.Log("Rewarded interstitial ad loaded with response : "
                          + ad.GetResponseInfo());

                _rewardedInterstitialAd = ad;
            });
    }


    public void ShowRewardedInterstitialAd()
    {
        const string rewardMsg =
            "Rewarded interstitial ad rewarded the user. Type: {0}, amount: {1}.";

        if (_rewardedInterstitialAd != null && _rewardedInterstitialAd.CanShowAd())
        {
            _rewardedInterstitialAd.Show((Reward reward) =>
            {
                // TODO: Reward the user.
                Debug.Log(String.Format(rewardMsg, reward.Type, reward.Amount));
            });

            RegisterEventHandlers(_rewardedInterstitialAd);
        }
    }

    private void RegisterEventHandlers(RewardedInterstitialAd ad)
    {
        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("Rewarded interstitial ad paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Rewarded interstitial ad recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () =>
        {
            Debug.Log("Rewarded interstitial ad was clicked.");
        };
        // Raised when an ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Rewarded interstitial ad full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Rewarded interstitial ad full screen content closed.");
            GamePanelManager.instance.ClosePanel("MixAdPanel");
            StartCoroutine(HandleAdClosed());
            LoadRewardedInterstitialAd();
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Rewarded interstitial ad failed to open " +
                           "full screen content with error : " + error);
            LoadRewardedInterstitialAd();
        };
    }

    public IEnumerator StartInterstitialRewardedAtProcess()
    {
        yield return new WaitForSeconds(3);
        GamePanelManager.instance.LoadPanel("MixAdPanel");
        TextMeshProUGUI adStartingtext = UIManager.Instance.MixAdLoadingText;
        string language = LanguageManager.GetLanguage();

        int time = 5;

        while(time > 0)
        {
            time--;
            //adStartingtext.text = language == "English" ? $"The video starts in {time} seconds..." : $"Video {time} saniye içinde baþlýyor...";        
            adStartingtext.text = LanguageManager.instance.GetLocalizedValue("VideoStartTimerText").Replace("{time}",time.ToString());        
            yield return new WaitForSeconds(1);
        }

        ShowRewardedInterstitialAd();

        OnAdCoroutineFinished?.Invoke();

    }

}
