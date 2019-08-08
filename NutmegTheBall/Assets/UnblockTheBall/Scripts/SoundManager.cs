using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour {
	public static SoundManager instance = null;
	public AudioSource moveTileSound, star1Sound, star2Sound, star3Sound;
	public AudioSource pathCompletedSound, hintSound, scoreSound, goalSound, music;

	void Awake() {
		
		if (instance == null) {
			instance = this;
		} else if (instance != this) {
			Destroy (gameObject);
		}
		DontDestroyOnLoad (gameObject);

	}
		

	public void PlaySound(AudioSource source) {
		if (GameManager.soundEnabled)
		source.Play ();
	}
}
