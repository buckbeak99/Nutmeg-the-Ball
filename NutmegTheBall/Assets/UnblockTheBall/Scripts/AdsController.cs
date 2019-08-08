using UnityEngine;
using System.Collections;
#if CHARTBOOST
using ChartboostSDK;
#endif
#if ADMOB
using GoogleMobileAds.Api;
#endif
public class AdsController : MonoBehaviour {
	public int adsCounter = 3;
	public static AdsController instance;
	private int counter = 0;
	private bool isShowing = false;
	private bool rewardsLimitMet=false;

	#if CHARTBOOST
	private bool hasInterstitial = false;
	private bool hasMoreApps = false;
	private bool hasRewardedVideo = false;
	private bool hasInPlay = false;
	private int frameCount = 0;

	private bool ageGate = false;
	private bool autocache = true;
	private bool activeAgeGate = false;
	private bool showInterstitial = true;
	private bool showMoreApps = true;
	private bool showRewardedVideo = true;
	private int BANNER_HEIGHT = 110;
	private int REQUIRED_HEIGHT = 650;
	private int ELEMENT_WIDTH = 190;
	private Rect scrollRect;
	private Rect scrollArea;
	private Vector3 guiScale;
	private float scale;
	#endif

	#if ADMOB
	private string AndroidBannerID = "ca-app-pub-3940256099942544/6300978111"; // an example banner id
	private string AndroidInterstitialID = "ca-app-pub-3940256099942544/1033173712";
	private string AndroidRewardedVideoID = "ca-app-pub-3940256099942544/5224354917";
	//private string IOSBannerID = "INSERT_IOS_BANNER_AD_UNIT_ID_HERE";
	//private string IOSInterstitialID = "INSERT_IOS_INTERSTITIAL_AD_UNIT_ID_HERE";
	//private string IOSRewardedVideoID = "INSERT_IOS_REWARDED_VIDEO_ID_HERE";
	private BannerView bannerView;
	private InterstitialAd interstitial;
	private AdRequest request;
	private RewardBasedVideoAd rewardBasedVideo;

    public bool IsShowing { get => IsShowing2; set => IsShowing2 = value; }
    public bool IsShowing1 { get => IsShowing2; set => IsShowing2 = value; }
    public bool IsShowing2 { get => isShowing; set => isShowing = value; }
#endif

    void Awake() {
		if (instance == null) {
			instance = this;
		} else if (instance != this) {
			Destroy (gameObject);
		}
		DontDestroyOnLoad (gameObject);

	}
	// Use this for initialization
	void Start () {
		if (PlayerPrefs.GetInt ("Rewards") >= GameManager.rewardsLimit) 
			rewardsLimitMet = true;
		#if CHARTBOOST
		SetupDelegates ();
		Chartboost.cacheInterstitial (CBLocation.Default);
		Chartboost.cacheRewardedVideo (CBLocation.Default);
		#endif
		#if ADMOB
		request = new AdRequest.Builder()//.AddTestDevice ("B3F3B04D908A2803DED88BE1DA2B2B7E")
		.Build();
		RequestBanner ();
		RequestInterstitial ();
		RequestRewardBasedVideo ();
		#endif
	}

	#if CHARTBOOST
	void SetupDelegates()
	{
		// Listen to all impression-related events
		Chartboost.didInitialize += didInitialize;
		Chartboost.didFailToLoadInterstitial += didFailToLoadInterstitial;
		Chartboost.didDismissInterstitial += didDismissInterstitial;
		Chartboost.didCloseInterstitial += didCloseInterstitial;
		Chartboost.didClickInterstitial += didClickInterstitial;
		Chartboost.didCacheInterstitial += didCacheInterstitial;
		Chartboost.shouldDisplayInterstitial += shouldDisplayInterstitial;
		Chartboost.didDisplayInterstitial += didDisplayInterstitial;
		Chartboost.didFailToLoadMoreApps += didFailToLoadMoreApps;
		Chartboost.didDismissMoreApps += didDismissMoreApps;
		Chartboost.didCloseMoreApps += didCloseMoreApps;
		Chartboost.didClickMoreApps += didClickMoreApps;
		Chartboost.didCacheMoreApps += didCacheMoreApps;
		Chartboost.shouldDisplayMoreApps += shouldDisplayMoreApps;
		Chartboost.didDisplayMoreApps += didDisplayMoreApps;
		Chartboost.didFailToRecordClick += didFailToRecordClick;
		Chartboost.didFailToLoadRewardedVideo += didFailToLoadRewardedVideo;
		Chartboost.didDismissRewardedVideo += didDismissRewardedVideo;
		Chartboost.didCloseRewardedVideo += didCloseRewardedVideo;
		Chartboost.didClickRewardedVideo += didClickRewardedVideo;
		Chartboost.didCacheRewardedVideo += didCacheRewardedVideo;
		Chartboost.shouldDisplayRewardedVideo += shouldDisplayRewardedVideo;
		Chartboost.didCompleteRewardedVideo += didCompleteRewardedVideo;
		Chartboost.didDisplayRewardedVideo += didDisplayRewardedVideo;
		Chartboost.didCacheInPlay += didCacheInPlay;
		Chartboost.didFailToLoadInPlay += didFailToLoadInPlay;
		Chartboost.didPauseClickForConfirmation += didPauseClickForConfirmation;
		Chartboost.willDisplayVideo += willDisplayVideo;
		#if UNITY_IPHONE
		Chartboost.didCompleteAppStoreSheetFlow += didCompleteAppStoreSheetFlow;
		#endif
	}

	void didInitialize(bool status) {
		Debug.Log(string.Format("didInitialize: {0}", status));
	}

	void didFailToLoadInterstitial(CBLocation location, CBImpressionError error) {
		Debug.Log(string.Format("didFailToLoadInterstitial: {0} at location {1}", error, location));
	}

	void didDismissInterstitial(CBLocation location) {
		Debug.Log("didDismissInterstitial: " + location);
	}

	void didCloseInterstitial(CBLocation location) {
		Debug.Log("didCloseInterstitial: " + location);
		Chartboost.cacheInterstitial (CBLocation.Default);
	}

	void didClickInterstitial(CBLocation location) {
		Debug.Log("didClickInterstitial: " + location);
	}

	void didCacheInterstitial(CBLocation location) {
		Debug.Log("didCacheInterstitial: " + location);
	}

	bool shouldDisplayInterstitial(CBLocation location) {
		// return true if you want to allow the interstitial to be displayed
		Debug.Log("shouldDisplayInterstitial @" + location + " : " + showInterstitial);
		return showInterstitial;
	}

	void didDisplayInterstitial(CBLocation location){
		Debug.Log("didDisplayInterstitial: " + location);
	}

	void didFailToLoadMoreApps(CBLocation location, CBImpressionError error) {
		Debug.Log(string.Format("didFailToLoadMoreApps: {0} at location: {1}", error, location));
	}

	void didDismissMoreApps(CBLocation location) {
		Debug.Log(string.Format("didDismissMoreApps at location: {0}", location));
	}

	void didCloseMoreApps(CBLocation location) {
		Debug.Log(string.Format("didCloseMoreApps at location: {0}", location));
	}

	void didClickMoreApps(CBLocation location) {
		Debug.Log(string.Format("didClickMoreApps at location: {0}", location));
	}

	void didCacheMoreApps(CBLocation location) {
		Debug.Log(string.Format("didCacheMoreApps at location: {0}", location));
	}

	bool shouldDisplayMoreApps(CBLocation location) {
		Debug.Log(string.Format("shouldDisplayMoreApps at location: {0}: {1}", location, showMoreApps));
		return showMoreApps;
	}

	void didDisplayMoreApps(CBLocation location){
		Debug.Log("didDisplayMoreApps: " + location);
	}

	void didFailToRecordClick(CBLocation location, CBClickError error) {
		Debug.Log(string.Format("didFailToRecordClick: {0} at location: {1}", error, location));
	}

	void didFailToLoadRewardedVideo(CBLocation location, CBImpressionError error) {
		Debug.Log(string.Format("didFailToLoadRewardedVideo: {0} at location {1}", error, location));
		if (GameManager.rewardsCount >= GameManager.rewardsLimit)
			rewardsLimitMet = true;
	}

	void didDismissRewardedVideo(CBLocation location) {
		Debug.Log("didDismissRewardedVideo: " + location);
	}

	void didCloseRewardedVideo(CBLocation location) {
		Debug.Log("didCloseRewardedVideo: " + location);
	}

	void didClickRewardedVideo(CBLocation location) {
		Debug.Log("didClickRewardedVideo: " + location);
	}

	void didCacheRewardedVideo(CBLocation location) {
		Debug.Log("didCacheRewardedVideo: " + location);
	}

	bool shouldDisplayRewardedVideo(CBLocation location) {
		Debug.Log("shouldDisplayRewardedVideo @" + location + " : " + showRewardedVideo);
		return showRewardedVideo;
	}

	void didCompleteRewardedVideo(CBLocation location, int reward) {
		Debug.Log(string.Format("didCompleteRewardedVideo: reward {0} at location {1}", reward, location));
		GameManager.instance.ShowCongratsDialog ();
		GameManager.hintCount += 3;
		PlayerPrefs.SetInt ("Hints",GameManager.hintCount);
		GameManager.instance.SetHintCountText ();
	}

	void didDisplayRewardedVideo(CBLocation location){
		Debug.Log("didDisplayRewardedVideo: " + location);
	}

	void didCacheInPlay(CBLocation location) {
		Debug.Log("didCacheInPlay called: "+location);
	}

	void didFailToLoadInPlay(CBLocation location, CBImpressionError error) {
		Debug.Log(string.Format("didFailToLoadInPlay: {0} at location: {1}", error, location));
	}

	void didPauseClickForConfirmation() {
		#if UNITY_IPHONE
		Debug.Log("didPauseClickForConfirmation called");
		activeAgeGate = true;
		#endif
	}

	void willDisplayVideo(CBLocation location) {
		Debug.Log("willDisplayVideo: " + location);
	}
		

		public void ShowRewardedVideo() {
		if (Chartboost.hasRewardedVideo(CBLocation.Default)) {
			if (rewardsLimitMet) {
				GameManager.rewardsCount = 0;
				rewardsLimitMet = false;
			}
			Chartboost.showRewardedVideo (CBLocation.Default);
			GameManager.rewardsCount += 1;
			Debug.Log ("rewardsCount = "+GameManager.rewardsCount);
			PlayerPrefs.SetInt ("Rewards",GameManager.rewardsCount);
		} else {
			if (rewardsLimitMet) {
			Debug.Log ("Rewards Limit met");
			GameManager.instance.ShowRewardLimitDialog ();
		}
		if (!rewardsLimitMet) GameManager.instance.ShowVideoUnavailableDialog ();
		}

			Chartboost.cacheRewardedVideo (CBLocation.Default);
	}

		public void ShowInterstitialAds(){
		counter++;
		if (counter >= adsCounter) {
		counter = 0;
		if (Chartboost.hasInterstitial (CBLocation.Default))
			Chartboost.showInterstitial (CBLocation.Default);
			else Chartboost.cacheInterstitial (CBLocation.Default);
				}
			
		}
	#endif

	#if ADMOB
	private void RequestRewardBasedVideo()
	{
	#if UNITY_ANDROID
	string adUnitId = AndroidRewardedVideoID;
	#elif UNITY_IPHONE
	string adUnitId = IOSRewardedVideoID;
	#else
	string adUnitId = "unexpected_platform";
	#endif

	rewardBasedVideo = RewardBasedVideoAd.Instance;
	rewardBasedVideo.OnAdLoaded += RewardedVideoOnLoaded;
	rewardBasedVideo.OnAdRewarded += RewardedVideoOnReward;
	rewardBasedVideo.OnAdClosed += RewardedVideoOnClosed;
	rewardBasedVideo.OnAdFailedToLoad += RewardedVideoOnFailedToLoad;
	rewardBasedVideo.LoadAd(request, adUnitId);
	}

	private void RewardedVideoOnLoaded(object sender, System.EventArgs args) {
	Debug.Log ("Video Ads Loaded");
	}

	private void RewardedVideoOnFailedToLoad(object sender,AdFailedToLoadEventArgs args) {
	Debug.Log ("Video Ads Failed To Load: "+args.Message);
	if (GameManager.rewardsCount>=GameManager.rewardsLimit)
	rewardsLimitMet = true;
	}

	private void RewardedVideoOnClosed(object sender, System.EventArgs args) {
	LoadRewardedVideoAds ();
	Debug.Log ("Video Ads Closed");
	}

	private void LoadRewardedVideoAds() {
	#if UNITY_ANDROID
	string adUnitId = AndroidRewardedVideoID;
	#elif UNITY_IPHONE
	string adUnitId = IOSRewardedVideoID;
	#else
	string adUnitId = "unexpected_platform";
	#endif
	rewardBasedVideo.LoadAd(request, adUnitId);
	}

	private void RewardedVideoOnReward(object sender, Reward args) {
	Debug.Log ("VideoAds: User has been rewarded");
	GameManager.hintCount += 3;
	PlayerPrefs.SetInt ("Hints",GameManager.hintCount);
	GameManager.instance.SetHintCountText ();
	GameManager.instance.ShowCongratsDialog ();
	}

	private void RequestBanner()
	{
	#if UNITY_ANDROID
	string adUnitId = AndroidBannerID;
	#elif UNITY_IPHONE
	string adUnitId = IOSBannerID;
	#else
	string adUnitId = "unexpected_platform";
	#endif

	// Create a 320x50 banner at the top of the screen.
	bannerView = new BannerView(adUnitId, AdSize.SmartBanner, AdPosition.Bottom);
	//
	bannerView.OnAdLoaded+=BannerOnLoaded;
	bannerView.OnAdClosed += BannerOnClosed;
	bannerView.LoadAd(request);
	}

	void BannerOnLoaded (object sender, System.EventArgs e)
	{
	//bannerView.Hide ();
	}

	void BannerOnClosed (object sender, System.EventArgs e)
	{
	bannerView.LoadAd (request);
	}

	private void RequestInterstitial()
	{
	#if UNITY_ANDROID
	string adUnitId = AndroidInterstitialID;
	#elif UNITY_IPHONE
	string adUnitId = IOSInterstitialID;
	#else
	string adUnitId = "unexpected_platform";
	#endif

	// Initialize an InterstitialAd.
	interstitial = new InterstitialAd(adUnitId);
	interstitial.OnAdClosed+=InterstitialOnClosed;
	interstitial.OnAdFailedToLoad += InterstitialOnFailedToLoad;
		interstitial.OnAdLoaded += InterstitialOnLoaded;
	interstitial.LoadAd(request);
	}

	void InterstitialOnClosed (object sender, System.EventArgs e)
	{
	interstitial.Destroy ();
	interstitial.LoadAd (request);
	}

	void InterstitialOnLoaded (object sender, System.EventArgs e)
	{
		Debug.Log ("Ads: Interstitial Loaded");
	}

	void InterstitialOnFailedToLoad (object sender, AdFailedToLoadEventArgs args)
	{
	Debug.Log ("Ads: Interstitials failed to load: "+args.Message);
	}

	public void HideAds(){
	if (instance != null) {
	instance.IsShowing = false;
	instance.HideBanner ();
	}
	}

	public void ShowRewardedVideo() {
	if (rewardBasedVideo.IsLoaded ()) {
	if (rewardsLimitMet) {
	GameManager.rewardsCount = 0;
	rewardsLimitMet = false;
	}
	rewardBasedVideo.Show ();
	GameManager.rewardsCount += 1;
	Debug.Log ("rewardsCount = "+GameManager.rewardsCount);
	PlayerPrefs.SetInt ("Rewards",GameManager.rewardsCount);
	} else {
	if (rewardsLimitMet) {
	Debug.Log ("Rewards Limit met");
	GameManager.instance.ShowRewardLimitDialog ();
	}
	if (!rewardsLimitMet) GameManager.instance.ShowVideoUnavailableDialog ();
	LoadRewardedVideoAds ();
	}
	}

	public void ShowInterstitialAds(){
	counter++;
	if (counter >= adsCounter) {
	counter = 0;
	if (interstitial.IsLoaded ()) {
	interstitial.Show ();
	}
	else interstitial.LoadAd(request);

	}
	}

	public void ShowBanner(){
	#if !UNITY_EDITOR
	bannerView.Show ();
	#endif
	}
	private void HideBanner(){
	bannerView.Hide ();
	}

	public void Destroy() {
	bannerView.Destroy();
	interstitial.Destroy();
	}

	#endif
		}
