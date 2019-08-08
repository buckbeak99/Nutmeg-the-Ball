using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.UI;
using System.IO;

public class GameManager : MonoBehaviour {

	public static GameManager instance = null;
	[HideInInspector] public GameObject board;

	//This used only for current level
	[HideInInspector] public int stars;


	[HideInInspector] public bool goalAchieved = false;
	[HideInInspector]public static int StandartMode_maxLevels = 40, RotationMode_maxLevels = 20;
	[HideInInspector]public static int currentMode_maxLevels, currentMode_maxPacks;
	[HideInInspector]public static int StandartMode_maxPacks = 4, RotationMode_maxPacks = 2;
	[HideInInspector]public static bool soundEnabled;
	[HideInInspector]public GameObject genericDialog;
	[HideInInspector]public static int hintCount;
	[HideInInspector]public static int rewardsCount = 0,rewardsLimit=3;
	[HideInInspector]public const string STANDART_MODE = "LP", ROTATION_MODE = "RP";
	[HideInInspector]public static string levelMode;
	public GameObject genericDialogprefab;
	public static float boardBorderWidth = 0.15f;
	public static Vector2 tileSize;
	public static int currentPack;
	public static int currentLevel;
	public GameObject ballPrefab;
	public GameObject levelCompletedDialog_prefab;
	private new Camera camera;
	private GameObject levelCompletedDialog;
	private float boardInitX, boardInitY;
	private Vector3 mouseHitPos;
	private bool startSwipe;
	private GameObject targetTile;
	private BoxCollider2D boardCollider;
	private int tileX, tileY, moveToX, moveToY;
	private Vector3 targetPosition;
	private const int NO_MOVE = 0, MOV_RIGHT = 1, MOV_LEFT=2, MOV_UP = 3, MOV_DOWN = 4, ROTATE = 5;
	private int boardState;
	private GameObject ball;
	private Ball ballScript;
	private GameObject startTile,goalTile;
	private float tileRotation;
	private GameObject[] tiles;
	private string packageName="com.sust_accessDenied.nutmegtheballslidingpuzzle";

	void Awake() {
		Application.targetFrameRate = 60;
		QualitySettings.vSyncCount = 0;
		//Use singleton pattern, which allows only one
		//GameManager's instance between all the scenes
		if (instance == null) {
			instance = this;
		} else if (instance != this) {
			Destroy (gameObject);
		}
		DontDestroyOnLoad (gameObject);

	}


	void Start() {
		levelMode=STANDART_MODE;
		currentMode_maxPacks = StandartMode_maxPacks;
		currentMode_maxLevels = StandartMode_maxLevels;
		string soundSetting = "SoundSettings";
		if (!PlayerPrefs.HasKey ("Hints")) {
			PlayerPrefs.SetInt ("Hints", 20);
			hintCount = 20;
		} else
			hintCount = PlayerPrefs.GetInt ("Hints");
		if (!PlayerPrefs.HasKey (soundSetting)) {
			PlayerPrefs.SetInt (soundSetting, 1);
			soundEnabled = true;
		}
		else {
			if (PlayerPrefs.GetInt (soundSetting) == 1)
				soundEnabled = true;
			if (PlayerPrefs.GetInt (soundSetting) == 0)
				soundEnabled = false;
		}
		if (!PlayerPrefs.HasKey ("Rewards")) {
			PlayerPrefs.SetInt ("Rewards", 0);
			rewardsCount = 0;
		} else
			rewardsCount = PlayerPrefs.GetInt ("Rewards");
		Debug.Log ("rewardsCount = "+rewardsCount);
		SoundManager.instance.PlaySound (SoundManager.instance.music);
	}

	public void LoadLevel() {
		goalAchieved = false;
		stars = 0;
		camera = Camera.main.GetComponent<Camera> ();

		GameObject.Find ("LevelPackText").GetComponent<Text> ().text = "Level Pack " + currentPack;
		GameObject.Find ("LevelText").GetComponent<Text> ().text = "Level " + currentLevel;

		tiles = GameObject.FindGameObjectsWithTag ("Tile");
		for (int i = 0; i < tiles.Length; i++) {
			if (tiles [i].GetComponent<Tile> ().isStartTile)
				startTile = tiles [i];
			if (tiles [i].GetComponent<Tile> ().isGoalTile)
				goalTile = tiles [i];
		}
		ball = Instantiate (ballPrefab);
		ball.transform.position = startTile.transform.position;
		ballScript = ball.GetComponent<Ball> ();
		Vector2 startPoint = startTile.GetComponent<Tile> ().getConnectionPoint1Pos ();

		GameObject brush = GameObject.Find ("Brush");
		if (brush != null)
			DestroyImmediate (brush);
		//Get board GameObject
		boardState = NO_MOVE;
		board = GameObject.FindGameObjectWithTag("Board");
		GameObject puzzle = GameObject.Find("Tiles");
		Board puzzleScript = puzzle.GetComponent<Board> ();
		tileSize = new Vector2(puzzleScript.tileSize.x/puzzleScript.pixelsToUnits,puzzleScript.tileSize.y/puzzleScript.pixelsToUnits);
		boardInitX = puzzle.transform.position.x;
		boardInitY = puzzle.transform.position.y;
		int boardGridWidth = (int)puzzleScript.mapSize.x;
		int boardGridHeight = (int)puzzleScript.mapSize.y;
		for (int i = 0; i < boardGridWidth; i++) {
			for (int k = 0; k < boardGridHeight; k++) {
				GameObject boardRect = new GameObject ("boardRect");
				boardRect.transform.SetParent (board.transform);
				boardCollider = boardRect.AddComponent<BoxCollider2D> ();
				boardCollider.size = new Vector3 (tileSize.x - 0.1f, tileSize.y - 0.1f, 0);
				boardCollider.tag = "BoardCollider";
				boardCollider.isTrigger = true;
				boardRect.transform.position = new Vector3 (boardInitX+tileSize.x/2f+tileSize.x*i, boardInitY-tileSize.y/2f-tileSize.y*k, 1);
			}
		}
		GameObject canvas = GameObject.Find ("Canvas");
		levelCompletedDialog = Instantiate (levelCompletedDialog_prefab);
		levelCompletedDialog.transform.SetParent (canvas.transform,false);
		levelCompletedDialog.SetActive (false);

		hintCount = PlayerPrefs.GetInt ("Hints");
		SetHintCountText ();
		if (genericDialog == null) {
			genericDialog = Instantiate (genericDialogprefab);
			canvas = GameObject.Find ("Canvas");
			genericDialog.transform.SetParent (canvas.transform,false);
		}
	}

	public void FinishLevel() {
		//Update stars for current level
		string currentLevelStr = levelMode+currentPack+"_"+"level-"+currentLevel;
		string levelStars = currentLevelStr + "stars";
		int s = PlayerPrefs.GetInt (levelStars);
		string packNameStr = levelMode+"_level_pack_" + currentPack;
		string starsCollectedStr = packNameStr + "stars";
		string packCompletedStr = packNameStr + "completed";
		//If current collected stars amount is greater then last record, then
		//update stars amount for current level and for current pack
		if (stars > s) {
			PlayerPrefs.SetInt (levelStars, stars);
			int oldStars = PlayerPrefs.GetInt (starsCollectedStr);
			PlayerPrefs.SetInt (starsCollectedStr,oldStars+(stars-s));
		}
		if (currentLevel + 1 <= currentMode_maxLevels) {
			//Unlock next level
			string nextLevelStr = levelMode+currentPack+"_"+"level-"+(currentLevel + 1);
			PlayerPrefs.SetInt (nextLevelStr, 1);
		} else {
			PlayerPrefs.SetInt (packCompletedStr,1);
			if (currentPack + 1 <= currentMode_maxPacks) {
				PlayerPrefs.SetInt (levelMode+"_level_pack_"+(currentPack+1),1);
			}
		}
	}

	public void UpdateLevel() {

		if (Input.GetMouseButtonDown(0) && boardState==NO_MOVE &&!ballScript.move
			&& !goalAchieved) {
			startSwipe = true;
			targetTile = GetTileClicked ();

		}

		if (Input.GetMouseButtonUp (0)) {
			if (targetTile != null && boardState==NO_MOVE) {
				GameObject tile = GetTileClicked ();
				Tile tileScript = null;
				if (tile!=null) tileScript = tile.GetComponent<Tile> ();
				if (tile == targetTile && tileScript.isRotatable && !ballScript.move) {
					if (!tileScript.rotating)
						tileScript.BeginRotation ();
					boardState = ROTATE;
				}
			}
			startSwipe = false;
		}
		if (startSwipe && targetTile != null) 
			CheckClickPosition ();

		if (boardState != NO_MOVE && boardState!= ROTATE && targetTile != null) {
			if (!targetTile.GetComponent<Tile> ().isFixed)
				MoveTile (targetTile);
			else {
				boardState = NO_MOVE;
				targetTile = null;
				FinishMove ();
			}
		}

		if (boardState == ROTATE) {
			bool anyRotation = false;
			for (int i = 0; i < tiles.Length; i++) {
				if (tiles [i].GetComponent<Tile> ().rotating)
					anyRotation = true;
			}
			if (!anyRotation) {
				FinishMove();
			}
		}

	}

	GameObject GetTileClicked() {
		GameObject b = null;
		float mouseX = camera.ScreenToWorldPoint (Input.mousePosition).x;
		float mouseY = camera.ScreenToWorldPoint (Input.mousePosition).y;
		RaycastHit2D hitInfo = Physics2D.Raycast (new Vector2 (mouseX,mouseY), Vector2.zero, 0);
		if (hitInfo) {
			if (hitInfo.collider.gameObject.tag=="Tile") {
				b = hitInfo.collider.gameObject;
				tileX = Mathf.FloorToInt((b.transform.position.x-boardInitX)/tileSize.x);
				tileY = Mathf.FloorToInt((b.transform.position.y+boardInitY)/tileSize.y);
			} 
			if (hitInfo.collider.gameObject.tag=="star-collectable" ||
				hitInfo.collider.gameObject.tag=="connection_point_1" ||
				hitInfo.collider.gameObject.tag=="connection_point_2") {
				b = hitInfo.collider.transform.parent.gameObject;
				tileX = Mathf.FloorToInt((b.transform.position.x-boardInitX)/tileSize.x);
				tileY = Mathf.FloorToInt((b.transform.position.y+boardInitY)/tileSize.y);
			} 
		} 	
		return b;
	}

	public void SetHintCountText() {
		GameObject hintObject = GameObject.Find ("Hint-Button");
		if (hintObject != null) {
			GameObject txt = hintObject.transform.Find ("Text").gameObject;
			txt.GetComponent<Text> ().text = "Hint ("+hintCount+")";
		}
	}


	void FinishMove() {
		boardState = NO_MOVE;
		targetTile = null;
		SoundManager.instance.PlaySound (SoundManager.instance.moveTileSound);
		CheckPathCompleted ();
	}


	void MoveTile(GameObject tile) {
		if (tile.transform.position != targetPosition) {
			float step = Tile.movSpeed * Time.deltaTime;
			tile.transform.position = Vector3.MoveTowards (tile.transform.position, targetPosition, step);
		} else
			FinishMove ();
	}

	public void CheckPathCompleted() {
		if (!goalAchieved) {
			GameObject nextTile = startTile.transform.Find ("connection_point_1").GetComponent<ConnectionPoint> ().connectedTile;
			if (nextTile != null)
				CheckNextPath (startTile, nextTile);
			else
				ballScript.ClearWaypoints ();
		}
	}

	void CheckNextPath(GameObject previousTile, GameObject currentTile) {
		GameObject nextTile = null;
		if (currentTile.GetComponent<Tile> ().IsConnected ()) {
			GameObject connectedTile2 = null;
			GameObject connectedTile1 = currentTile.transform.Find ("connection_point_1").GetComponent<ConnectionPoint> ().connectedTile;
			if (!currentTile.GetComponent<Tile> ().isStartTile && !currentTile.GetComponent<Tile> ().isGoalTile)
				connectedTile2 = currentTile.transform.Find ("connection_point_2").GetComponent<ConnectionPoint> ().connectedTile;
			if (connectedTile1 != previousTile)
				nextTile = connectedTile1;
			if (connectedTile2 != previousTile && connectedTile2 != null)
				nextTile = connectedTile2;

			if (nextTile != null) {
				ballScript.AddWaypoint (currentTile.transform);
				CheckNextPath (currentTile, nextTile);
			} else {
				ballScript.AddWaypoint (currentTile.transform);
				if (!ball.GetComponent<Ball> ().move) {
					ball.GetComponent<Ball> ().move = true;
					SoundManager.instance.PlaySound (SoundManager.instance.pathCompletedSound);
				}
			}

		} else 
			ball.GetComponent<Ball> ().ClearWaypoints ();
	}

	void CheckClickPosition() {
		float mouseX = camera.ScreenToWorldPoint (Input.mousePosition).x;
		float mouseY = camera.ScreenToWorldPoint (Input.mousePosition).y;

		RaycastHit2D hitInfo = Physics2D.Raycast (new Vector2 (mouseX, mouseY), Vector2.zero, 0);
		if (hitInfo) {
			if (hitInfo.collider.gameObject.tag=="BoardCollider") {
				GameObject c = hitInfo.collider.gameObject;
				moveToX = Mathf.FloorToInt((c.transform.position.x-boardInitX)/tileSize.x);
				moveToY = Mathf.FloorToInt((c.transform.position.y+boardInitY)/tileSize.y);
				if (targetTile != null && boardState==NO_MOVE) {
					if (tileY == moveToY) {
						if (tileX == moveToX-1) {
							boardState = MOV_RIGHT;
						}
						if (tileX == moveToX+1) {
							boardState = MOV_LEFT;
						}
					}
					if (tileX == moveToX) {
						if (tileY == moveToY-1) {
							boardState = MOV_UP;
						}
						if (tileY == moveToY+1) {
							boardState = MOV_DOWN;
						}
					}
					targetPosition = new Vector3 (c.transform.position.x,c.transform.position.y,targetTile.transform.position.z);
				}
			} 
		} 	
	}

	//UI Management section

	public void ShowLevelCompletedDialog(int stars) {
		if (!levelCompletedDialog.activeSelf) {
			levelCompletedDialog.GetComponent<RectTransform> ().offsetMax = new Vector2 (0, 0);
			levelCompletedDialog.GetComponent<RectTransform> ().offsetMin = new Vector2 (0, 0);
			levelCompletedDialog.SetActive (true);
			levelCompletedDialog.GetComponent<LevelCompletedDialog> ().ShowStars (stars);
		}
	}

	public void LoadHomeScene() {
		SceneManager.LoadScene ("HomeScene");
	}

	public void LoadSelectLevelScene() {
		SceneManager.LoadScene ("SelectLevel");
	}

	public void LoadStandartModePacks() {
		levelMode = STANDART_MODE;
		currentMode_maxPacks = StandartMode_maxPacks;
		currentMode_maxLevels = StandartMode_maxLevels;
		LoadSelectLevelPackScene ();
	}

	public void LoadRotationModePacks() {
		levelMode = ROTATION_MODE;
		currentMode_maxPacks = RotationMode_maxPacks;
		currentMode_maxLevels = RotationMode_maxLevels;
		LoadSelectLevelPackScene ();
	}

	public void LoadSelectLevelPackScene() {
		SceneManager.LoadScene ("SelectPackScene");
	}


	public void RestartLevelW() {
		instance.RestartLevel ();
	}

	void RestartLevel() {
		SceneManager.LoadScene (SceneManager.GetActiveScene().name,LoadSceneMode.Single);
	}

	public void PlayNextLevelW() {
		instance.PlayNextLevel ();
	}

	public void ShowExitDialog() {
		instance.ExitDialog ();
	}

	void ExitDialog() {
		if (genericDialog != null) {
			genericDialog.GetComponent<GenericDialog> ().SetActiveDialog ("ExitDialog");
			genericDialog.SetActive (true);
		}
	}

	public void ShowEarnHintsDialog() {
		instance.EarnHintsDialog ();
	}

	void EarnHintsDialog() {
		if (genericDialog != null) {
			genericDialog.GetComponent<GenericDialog> ().SetActiveDialog ("EarnHintsDialog");
			genericDialog.SetActive (true);
		}
	}

	public void ShowRewardLimitDialog() {
		if (genericDialog != null) {
			genericDialog.GetComponent<GenericDialog> ().SetActiveDialog ("RewardLimitMetDialog");
			genericDialog.SetActive (true);
		}
	}

	public void ShowVideoUnavailableDialog() {
		if (genericDialog != null) {
			genericDialog.GetComponent<GenericDialog> ().SetActiveDialog ("VideoUnavailableDialog");
			genericDialog.SetActive (true);
		}
	}

	public void ShowRewardedVideo() {
		instance.PlayRewardedVideo ();
	}

	void PlayRewardedVideo() {
		if (Application.internetReachability == NetworkReachability.NotReachable) {
			genericDialog.GetComponent<GenericDialog> ().SetActiveDialog ("InternetErrorDialog");
			genericDialog.SetActive (true);
		} else {
			genericDialog.SetActive (false);
			AdsController.instance.ShowRewardedVideo ();
		}
	}

	public void ShowCongratsDialog() {
		if (genericDialog != null) {
			genericDialog.GetComponent<GenericDialog> ().SetActiveDialog ("CongratsDialog");
			genericDialog.SetActive (true);
		}
	}

	public void CancelDialog() {
		if (instance.genericDialog != null)
			instance.genericDialog.SetActive (false);
	}

	public void Exit() {
		#if ADMOB
		AdsController.instance.Destroy ();
		#endif
		Application.Quit ();
	}

	void PlayNextLevel() {
		string packNameStr = "level_pack_" + currentPack;
		string packCompletedStr = packNameStr + "completed";
		if (currentLevel + 1 <= currentMode_maxLevels) {
			currentLevel += 1;
			SceneManager.LoadScene (levelMode+currentPack+"_"+"level-"+currentLevel, LoadSceneMode.Single);
		} else
			SceneManager.LoadScene ("SelectPackScene",LoadSceneMode.Single);		
	}

	public void RateUs() {
		#if UNITY_ANDROID
		Application.OpenURL("market://details?id="+packageName);
		#endif
	}

	public void ShareIt() {
		instance.ShareLink ();
	}

	void ShareLink() {
		string bodyString = "";
		string subjectString = "New Android Game";
		//Refernece of AndroidJavaClass class for intent
		AndroidJavaClass intentClass = new AndroidJavaClass ("android.content.Intent");
		//Refernece of AndroidJavaObject class for intent
		AndroidJavaObject intentObject = new AndroidJavaObject ("android.content.Intent");
		//call setAction method of the Intent object created
		intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
		//set the type of sharing that is happening
		intentObject.Call<AndroidJavaObject>("setType", "text/plain");
		//add data to be passed to the other activity i.e., the data to be sent
		intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_SUBJECT"), subjectString);
		bodyString = "I play this new cool puzzle game - https://play.google.com/store/apps/details?id=" +packageName;
		intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"),bodyString);
		//get the current activity
		AndroidJavaClass unity = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
		AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");
		//start the activity by sending the intent data
		currentActivity.Call ("startActivity", intentObject);

	}

	void Update() {
		if (Input.GetKeyDown (KeyCode.Escape)) {
			if (SceneManager.GetActiveScene ().name == "HomeScene")
				instance.ExitDialog ();
		}
	}

	void OnApplicationQuit(){
		GameManager.instance = null;
	}


}
