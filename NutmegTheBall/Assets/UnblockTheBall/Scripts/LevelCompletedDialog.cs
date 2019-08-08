using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LevelCompletedDialog : MonoBehaviour {
	public Sprite star,emptyStar;
	private GameObject[] stars;
	private ParticleSystem[] emitters; 
	private float timeStep = 0.5f;
	private float timer;
	private int numStars,counter;
	private bool timerEnabled = false;

	void Start() {
		stars = new GameObject[3];
		stars [0] = GameObject.FindGameObjectWithTag ("ui-star-1");
		stars [1] = GameObject.FindGameObjectWithTag ("ui-star-2");
		stars [2] = GameObject.FindGameObjectWithTag ("ui-star-3");
		emitters = new ParticleSystem[stars.Length];
		for (int i = 0; i < stars.Length; i++) {
			emitters [i] = stars [i].GetComponent<ParticleSystem> ();
		}
	}

	public void Reset() {
		for (int i = 0; i < stars.Length; i++) {
			stars [i].GetComponent<Image> ().sprite = emptyStar;
		}
		counter = 0;
		numStars = 0;
	}

	void PlayStarEffect() {
		timer += Time.deltaTime;
		if (timer > timeStep) {
			if (stars [counter].GetComponent<Image> ().sprite != star) {
				stars [counter].GetComponent<Image> ().sprite = star;
				if (counter == 0)
					SoundManager.instance.PlaySound (SoundManager.instance.star1Sound);
				if (counter == 1)
					SoundManager.instance.PlaySound (SoundManager.instance.star2Sound);
				if (counter == 2)
					SoundManager.instance.PlaySound (SoundManager.instance.star3Sound);
			}
			emitters [counter].Play ();
			timer = 0;
			timerEnabled = false;
		}
	}

	public void ShowStars(int n) {
		if (n > 0) {
			numStars = n - 1;
			timerEnabled = true;
		}
	}

	void Update() {
		if (timerEnabled)
			PlayStarEffect ();
		else {
			if (counter < numStars) {
				counter += 1;
				timerEnabled = true;
			}
		}
	}
}
