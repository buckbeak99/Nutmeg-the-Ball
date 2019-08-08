using UnityEngine;
using System.Collections;

public class GenericDialog : MonoBehaviour {

	private GameObject[] dialogs;

	void Start () {
		GetComponent<RectTransform> ().offsetMax = new Vector2 (0, 0);
		GetComponent<RectTransform> ().offsetMin = new Vector2 (0, 0);
		dialogs = GameObject.FindGameObjectsWithTag ("dialog");
		GameManager.instance.genericDialog = gameObject;
		gameObject.SetActive (false);
	}

	public void SetActiveDialog(string dialogName) {
		for (int i = 0; i < dialogs.Length; i++) {
			if (dialogs [i].name == dialogName)
				dialogs [i].SetActive (true);
			else
				dialogs [i].SetActive (false);
		}
	}
}
