using UnityEngine;
using System.Collections;

public class LevelLoader : MonoBehaviour {

	public Color cameraColor;

	// Use this for initialization
	void Start () {
		Camera.main.backgroundColor = cameraColor;
		AdsController.instance.ShowInterstitialAds ();
		GameManager.instance.LoadLevel ();
		#if ADMOB
		AdsController.instance.ShowBanner ();
		#endif
	}

	void Update() {
		GameManager.instance.UpdateLevel ();
	}

}
