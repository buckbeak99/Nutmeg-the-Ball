using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Hint : MonoBehaviour {
	
	public Sprite hintImage1,hintImage2,hintImage3;
	[HideInInspector]public bool show=false;
	[HideInInspector] public GameObject[] hintPath;
	private Transform[] hints;
	private float timeStep = 0.1f;
	private float timer;
	private int counter=1,numHints;
	private Color hintColor;

	public void ShowHint() {
		if (GameManager.hintCount >= 0) {
			SoundManager.instance.PlaySound (SoundManager.instance.hintSound);
			show = true;
			GameManager.hintCount -= 1;
			PlayerPrefs.SetInt ("Hints", GameManager.hintCount);
			GameManager.instance.SetHintCountText ();
		} else {
			GameManager.instance.ShowEarnHintsDialog ();
		}
	}

	public void Reset() {
		for (int i = 0; i < hints.Length; i++)
			if (hints[i].gameObject.GetComponent<SpriteRenderer>()!=null)
				hints [i].gameObject.GetComponent<SpriteRenderer> ().color = Color.clear;
		counter = 1;
		show = false;
		timer = 0;
	}

	void ShowHintPath() {
		showPath ();
	}

	private void showPath() {
		timer += Time.deltaTime;
		if (timer > timeStep) {
			hints [counter].gameObject.GetComponent<SpriteRenderer> ().color = hintColor;
			timer = 0;
			show = false;
		}
		if (timer == 0 && counter < numHints) {
			counter += 1;
			show = true;
		}
	}
		

	void Start() {
		show = false;
		hints = transform.GetComponentsInChildren<Transform>();
		for (int i = 0; i < hints.Length; i++)
			if (hints[i].gameObject.GetComponent<SpriteRenderer>()!=null)
			hints [i].gameObject.GetComponent<SpriteRenderer> ().color = Color.clear;
		numHints = hints.Length - 1;
		hintColor = new Color (1f,1f,1f,0.65f);
		Button hint_btn = GameObject.Find("Hint-Button").GetComponent<Button>();
		hint_btn.onClick.AddListener(ShowHint);
	}

	void Update() {
		if (show)
			ShowHintPath();
		if (Input.GetMouseButtonDown (0)) {
			if (counter == numHints)
				Reset ();
		}
	}
}
