using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements;


public class UnityAds : MonoBehaviour
{
    public string placementId = "BannerAd";
    public bool testMode = true;

#if UNITY_IOS
    public string gameId = "3163061";
#elif UNITY_ANDROID
    public string gameId = "3163060";
#endif

    void Start()
    {
        Advertisement.Initialize(gameId, testMode);
        StartCoroutine(ShowBannerWhenReady());
    }

    IEnumerator ShowBannerWhenReady()
    {
        while (!(Advertisement.IsReady(placementId) || Advertisement.Banner.isLoaded))
        {
            yield return new WaitForSeconds(0.5f);
        }
        Advertisement.Banner.Show(placementId);
    }

    private void OnDestroy()
    {
        Advertisement.Banner.Hide();
    }
}
