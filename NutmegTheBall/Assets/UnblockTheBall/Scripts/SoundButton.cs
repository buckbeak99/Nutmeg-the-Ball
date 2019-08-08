using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SoundButton : MonoBehaviour {
	public Sprite soundButtonEnabled, soundButtonDisabled; 
	private string soundSetting = "SoundSettings";

	void Start() {
		if (!PlayerPrefs.HasKey (soundSetting)) {
			PlayerPrefs.SetInt (soundSetting, 1);
			GameManager.soundEnabled = true;
		}
		if (PlayerPrefs.GetInt(soundSetting)==1) 
			GetComponent<Image> ().sprite = soundButtonEnabled;
		 else 
			GetComponent<Image> ().sprite = soundButtonDisabled;
	}

	public void ToggleSoundButton() {
		GameManager.soundEnabled = !GameManager.soundEnabled;
		if (GameManager.soundEnabled) {
			PlayerPrefs.SetInt (soundSetting, 1);
			SoundManager.instance.PlaySound (SoundManager.instance.music);
		}
		if (!GameManager.soundEnabled) {
			PlayerPrefs.SetInt (soundSetting, 0);
			SoundManager.instance.music.Stop ();
		}
		LoadSoundButtonImage ();
	}

	void LoadSoundButtonImage() {
		if (GameManager.soundEnabled) 
			GetComponent<Image> ().sprite = soundButtonEnabled;
		 else 
			GetComponent<Image> ().sprite = soundButtonDisabled;
	}
}
