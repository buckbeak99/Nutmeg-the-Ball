using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LevelButtonScript : MonoBehaviour {
	public Sprite star;
	public Sprite levelUnlocked;
	public GameObject[] stars;

	public void SetStars(int s) {
		if (s > 0) {
			for (int i = 0; i < s; i++) {
				stars [i].GetComponent<Image>().sprite = star;
			}
		}
	}

}
