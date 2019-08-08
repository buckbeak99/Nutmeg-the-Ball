using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SelectLevel : MonoBehaviour {
	public Button buttonPrefab;

	// Use this for initialization
	void Start () {
		FillGrid ();
	}

	void FillGrid() {
		GameObject panel = GameObject.Find ("Buttons-Panel");
		for (int i = 0; i < GameManager.currentMode_maxLevels; i++) {
			Button button = Instantiate (buttonPrefab);
			button.transform.SetParent (panel.transform,false);
			LevelButtonScript buttonScript = button.GetComponent<LevelButtonScript> ();
			GameObject buttonUnlocked = button.transform.Find ("LevelUnlocked").gameObject;
			GameObject lockImage = button.transform.Find ("LockImage").gameObject;
			buttonUnlocked.SetActive (false);
			int t = i+1;
			string levelStr = GameManager.levelMode + GameManager.currentPack + "_" + "level-" + t;
			string levelStars = levelStr + "stars";
			int stars = 0;
			int levelUnlocked = 0;
			if (!PlayerPrefs.HasKey (levelStr)) {
				if (i == 0) {
					levelUnlocked = 1;
				}
				PlayerPrefs.SetInt (levelStr, levelUnlocked);
				PlayerPrefs.SetInt (levelStars, stars);
			} else {
				levelUnlocked = PlayerPrefs.GetInt (levelStr);
				if (levelUnlocked == 1)
					stars = PlayerPrefs.GetInt (levelStars);
			}
			button.GetComponentInChildren<Text> ().text = t.ToString ();
			if (levelUnlocked == 1) {
				lockImage.SetActive (false);
				button.GetComponent<Image> ().sprite = buttonScript.levelUnlocked;
				buttonUnlocked.SetActive (true);
				buttonScript.SetStars (stars);
				button.onClick.AddListener (() => LoadLevel (t));
			}
		}

	}


	void LoadLevel(int level) {
		GameManager.currentLevel = level;
		SceneManager.LoadScene (GameManager.levelMode + GameManager.currentPack + "_" + "level-" + level,LoadSceneMode.Single);
	}
}
