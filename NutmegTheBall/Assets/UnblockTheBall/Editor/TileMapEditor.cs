using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.Collections;
using UnityEditor;
using System;

[CustomEditor(typeof(Board))]
public class TileMapEditor : Editor {

	public Board board;
	TileBrush brush;
	Vector3 mouseHitPos;
	private bool dragTile = true;
	private GameObject targetTile = null;
	private int maxTilesOnBoard;
	private static int[] usedTiles;
	private static GameObject hint;
	private Hint hintScript;
	private int hintCount;
	private GameObject goalTile,startTile;

	bool mouseOnMap {
		get { return mouseHitPos.x > 0 && mouseHitPos.x < board.gridSize.x && mouseHitPos.y < 0 && mouseHitPos.y > -board.gridSize.y;}
	}

	public override void OnInspectorGUI() {
		EditorGUILayout.BeginVertical ();
		Vector2 oldSize = board.mapSize;
		board.mapSize = EditorGUILayout.Vector2Field ("Puzzle Size: ",board.mapSize);
		if (oldSize != board.mapSize)
			UpdateCalculations ();
		Texture2D oldTexture = board.texture2D;
		board.texture2D = (Texture2D) EditorGUILayout.ObjectField ("Texture2D: ",board.texture2D, typeof(Texture2D),false);
		if (oldTexture != board.texture2D) {
			UpdateCalculations ();
			board.tileID = 1;
			CreateBrush ();
		}
		if (board.texture2D == null) {
			EditorGUILayout.HelpBox ("You have not selected a texture 2D yet.", MessageType.Warning);
		} else {
			EditorGUILayout.LabelField ("Tile Size: " + board.tileSize.x + "x" + board.tileSize.y);
			board.tilePadding = EditorGUILayout.Vector2Field ("Tile Padding",board.tilePadding);
			EditorGUILayout.LabelField ("Grid Size In Units: " + board.gridSize.x + "x" + board.gridSize.y);
			EditorGUILayout.LabelField ("Pixels To Units: " + board.pixelsToUnits.ToString());
			UpdateBrush (board.currentTileBrush);

			if(GUILayout.Button("Clear Tiles")){
				if (EditorUtility.DisplayDialog ("Clear map's tiles?", "Are you sure?", "Clear", "Do not clear")) {
					ClearMap ();
				}
			}
			if (GUILayout.Button ("Adjust size"))
				AdjustPuzzlePosition ();
			if (GUILayout.Button ("Generate Hint Path"))
				GenerateHintPath ();
			EditorGUILayout.HelpBox ("Keyboard Controls: \n"+
				"Shift key - Draw current selected tile; \n"+
				"Alt key - remove the tile \n"+
				"D key - toggle rotation on/off \n"+
				"G key - rotate the tile (only for rotation mode) \n"+
				"S key - add a star to the tile \n"+
				"H key - hides/shows hint path \n"+
				"Hold Control Key and drag - drags the tile", MessageType.Info);
		}
		EditorGUILayout.EndVertical ();
	}

	public void UpdateBrush(Sprite sprite) {
		if (brush != null)
			brush.UpdateBrush (sprite);
	}

	void UpdateHitPosition() {
		Plane p = new Plane (board.transform.TransformDirection(Vector3.forward),Vector3.zero);
		Ray ray = HandleUtility.GUIPointToWorldRay (Event.current.mousePosition);
		Vector3 hit = Vector3.zero;
		float dist = 0f;
		if (p.Raycast (ray, out dist))
			hit = ray.origin + ray.direction.normalized * dist;
		mouseHitPos = board.transform.InverseTransformPoint (hit);
	}

	void OnEnable() {
		hint = GameObject.Find ("Hint");
		board = target as Board;
		Tools.current = Tool.View;
		maxTilesOnBoard = (int)(board.mapSize.x * board.mapSize.y);
		usedTiles = new int[maxTilesOnBoard];
		board.tiles = GameObject.Find ("Tiles");
		CalculateUsedTiles ();
		if (board.texture2D != null) {
			UpdateCalculations ();
			GameObject b = GameObject.Find ("Brush");
			if (b == null)
				NewBrush ();
			else
				brush = b.GetComponent<TileBrush> ();
		}
	}

	void OnDisable() {
		DestroyBrush ();
	}

	void OnSceneGUI() {
		if (brush != null) {
			UpdateHitPosition ();
			MoveBrush ();
			if (board.texture2D != null && mouseOnMap) {
				Event cEvent = Event.current;
				if (cEvent.shift)
					Draw ();
				if (cEvent.type==EventType.KeyDown && cEvent.alt)
					RemoveTile ();

				if (cEvent.type == EventType.KeyDown && cEvent.keyCode == KeyCode.S) {
					ToggleStar ();
				}

				if (cEvent.type == EventType.KeyDown && cEvent.keyCode == KeyCode.D) {
					ToggleRotate ();
				}

				if (cEvent.type == EventType.KeyDown && cEvent.keyCode == KeyCode.G) {
					RotateTile ();
				}


				if (cEvent.type == EventType.KeyDown && cEvent.keyCode == KeyCode.H) {
					if (hint == null)
						hint = GameObject.Find ("Hint");
					if (hint) {
						hint.SetActive (!hint.activeSelf);
						EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
					}
				}

				if (cEvent.type == EventType.KeyDown && (cEvent.keyCode==KeyCode.LeftCommand || cEvent.keyCode==KeyCode.LeftControl))
				if (targetTile == null) {
					targetTile = GetTile (brush.row,brush.column);
					if (targetTile != null) {
						dragTile = true;
						brush.renderer2D.sortingOrder = -1000;
					}
				}
				if (cEvent.type == EventType.KeyUp && (cEvent.keyCode==KeyCode.LeftCommand || cEvent.keyCode==KeyCode.LeftControl)) {
					brush.renderer2D.sortingOrder = 1000;
					if (targetTile != null) {
						targetTile = null;
						dragTile = false;
					}
				}
			}
		}
	}

	void AdjustPuzzlePosition() {
		Board boardScript = GameObject.Find ("Tiles").GetComponent<Board> ();
		GameObject boardImage = GameObject.Find ("board-image");
		GameObject puzzle = GameObject.Find ("Tiles");
		SpriteRenderer spriteRenderer = boardImage.GetComponent<SpriteRenderer> ();
		float boardWidth = boardScript.mapSize.x * boardScript.tileSize.x;
		float boardHeight = boardScript.mapSize.y * boardScript.tileSize.y;
		float scaleX = boardWidth / (6.71f-GameManager.boardBorderWidth*2f) / boardScript.pixelsToUnits;
		float scaleY = boardHeight / (6.71f-GameManager.boardBorderWidth*2f) / boardScript.pixelsToUnits;
		boardImage.transform.localScale = new Vector2 (scaleX,scaleY);
		float newX = -spriteRenderer.bounds.size.x / 2f + GameManager.boardBorderWidth*scaleX;
		float newY = spriteRenderer.bounds.size.y / 2f - GameManager.boardBorderWidth*scaleY;
		//puzzle.transform.SetParent (board.transform);
		puzzle.transform.localPosition = new Vector3 (newX,newY,0);

		Camera cam = Camera.main.GetComponent<Camera>();
		Bounds targetBounds = spriteRenderer.bounds;
		float screenRatio = 7.2f / 12.8f;
		float targetRatio = targetBounds.size.x / targetBounds.size.y;

		if (screenRatio >= targetRatio)
		{
			Camera.current.orthographicSize = targetBounds.size.y / 2;
		}
		else
		{
			float differenceInSize = targetRatio / screenRatio;
			cam.orthographicSize = targetBounds.size.y / 2 * differenceInSize;
		}

		cam.transform.position = new Vector3(0, 0, cam.transform.position.z);
		//Adjust main camera's orthographic size in order to fit to board's camera
		Camera.main.orthographicSize = Camera.main.orthographicSize*(cam.orthographicSize/Camera.main.orthographicSize);
		EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
	}

	void ToggleStar() {
		GameObject tile = GetTile (brush.row,brush.column);
		if (tile != null && !tile.GetComponent<Tile> ().isSolid) {
			GameObject star = null;
			Transform[] childs = tile.transform.GetComponentsInChildren<Transform> ();
			for (int i = 0; i < childs.Length; i++)
				if (childs [i].gameObject.name.StartsWith ("star"))
					star = childs [i].gameObject;
			if (star != null) 
				DestroyImmediate (star);
			else {
			UnityEngine.Object prefab = AssetDatabase.LoadAssetAtPath ("Assets/UnblockTheBall/Prefabs/star.prefab", typeof(GameObject));
			star = Instantiate (prefab, Vector3.zero, Quaternion.identity) as GameObject;
			star.name = "star";
			star.transform.parent = tile.transform;
			star.layer = star.transform.parent.gameObject.layer;
			star.transform.localPosition = new Vector3 (0,0,tile.transform.position.z);
			}
		}
		EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
	}

	void ToggleRotate() {
		GameObject tile = GetTile (brush.row,brush.column);
		if (tile != null && !tile.GetComponent<Tile> ().isSolid) {
			GameObject rotateImage = null;
			Transform[] childs = tile.transform.GetComponentsInChildren<Transform> ();
			for (int i = 0; i < childs.Length; i++)
				if (childs [i].gameObject.name=="rotateImage")
					rotateImage = childs [i].gameObject;
			if (rotateImage != null) {
				tile.GetComponent<Tile> ().isRotatable = false;
				DestroyImmediate (rotateImage);
			}
			else {
				UnityEngine.Object prefab = AssetDatabase.LoadAssetAtPath ("Assets/UnblockTheBall/Prefabs/rotate-image.prefab", typeof(GameObject));
				rotateImage = Instantiate (prefab, Vector3.zero, Quaternion.identity) as GameObject;
				rotateImage.name = "rotateImage";
				rotateImage.transform.parent = tile.transform;
				rotateImage.layer = rotateImage.transform.parent.gameObject.layer;
				rotateImage.transform.localPosition = new Vector3 (0,0,tile.transform.position.z);
				tile.GetComponent<Tile> ().isRotatable = true;
			}
		}
		EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
	}

	void RotateTile() {
		GameObject tile = GetTile (brush.row,brush.column);
		if (tile != null && tile.GetComponent<Tile> ().isRotatable) {
			tile.transform.Rotate (new Vector3 (0, 0, -90f));
		}
		EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
	}


	void GenerateHintPath() {
		hint = GameObject.Find ("Hint");
		hintScript = hint.GetComponent<Hint> ();
		Transform[] childObjects = hint.transform.GetComponentsInChildren<Transform> ();
		for (int i = 1; i < childObjects.Length; i++)
			DestroyImmediate (childObjects[i].gameObject);
		startTile = null;
		goalTile = null;
		GameObject[] tiles = GameObject.FindGameObjectsWithTag ("Tile");
		for (int i = 0; i < tiles.Length; i++) {
			if (tiles [i].GetComponent<Tile> ().isStartTile)
				startTile = tiles [i];
			if (tiles [i].GetComponent<Tile> ().isGoalTile)
				goalTile = tiles [i];
		}
		hintCount = 1;
		SetHint (startTile,startTile);
		EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
	}

	void SetHint(GameObject previous,GameObject current) {
		GameObject obj = new GameObject ("hint_path_"+hintCount);
		hintCount += 1;
		SpriteRenderer renderer = obj.AddComponent<SpriteRenderer> ();
		renderer.sortingOrder = 10;
		obj.transform.SetParent (hint.transform);
		obj.transform.position = current.transform.position;
		obj.layer = obj.transform.parent.gameObject.layer;
		float rotation = 0;
		Tile currentTile = current.GetComponent<Tile> ();
		string type = currentTile.type;
		GameObject nextTile = null;
		if (type == "S1" || type == "G1") {
			rotation = 0f;
			renderer.sprite = hintScript.hintImage1;
			if (type=="S1") nextTile = GetTile(currentTile.row+1,currentTile.column);
		}
		if (type == "S2" || type == "G2") {
			rotation = -90f;
			renderer.sprite = hintScript.hintImage1;
			if (type=="S2") nextTile = GetTile(currentTile.row,currentTile.column-1);
		}
		if (type == "S3" || type == "G3") {
			rotation = -180f;
			renderer.sprite = hintScript.hintImage1;
			if (type=="S3") nextTile = GetTile(currentTile.row-1,currentTile.column);
		}
		if (type == "S4" || type == "G4") {
			rotation = -270f;
			renderer.sprite = hintScript.hintImage1;
			if (type=="S4") nextTile = GetTile(currentTile.row,currentTile.column+1);
		}
		if (type == "M1" || type == "F1") {
			rotation = 0f;
			renderer.sprite = hintScript.hintImage2;
			Tile p = previous.GetComponent<Tile> ();
			if (currentTile.row == p.row && (p.column - currentTile.column == 1))
				nextTile = GetTile (currentTile.row+1,currentTile.column);
			else nextTile = GetTile (currentTile.row,currentTile.column+1);
		}
		if (type == "M2" || type == "F2") {
			rotation = -90f;
			renderer.sprite = hintScript.hintImage2;
			Tile p = previous.GetComponent<Tile> ();
			if (currentTile.row == p.row && (p.column - currentTile.column == -1))
				nextTile = GetTile (currentTile.row+1,currentTile.column);
			else nextTile = GetTile (currentTile.row,currentTile.column-1);
		}
		if (type == "M3" || type == "F3") {
			rotation = -180f;
			renderer.sprite = hintScript.hintImage2;
			Tile p = previous.GetComponent<Tile> ();
			if (currentTile.row == p.row && (p.column - currentTile.column == -1))
				nextTile = GetTile (currentTile.row-1,currentTile.column);
			else nextTile = GetTile (currentTile.row,currentTile.column-1);
		}
		if (type == "M4" || type == "F4") {
			rotation = -270f;
			renderer.sprite = hintScript.hintImage2;
			Tile p = previous.GetComponent<Tile> ();
			if (currentTile.row == p.row && (p.column - currentTile.column == 1))
				nextTile = GetTile (currentTile.row-1,currentTile.column);
			else nextTile = GetTile (currentTile.row,currentTile.column+1);
		}
		if (type == "M5" || type == "F5") {
			rotation = 0f;
			renderer.sprite = hintScript.hintImage3;
			Tile p = previous.GetComponent<Tile> ();
			if ((currentTile.row - p.row==1) && p.column == currentTile.column)
				nextTile = GetTile (currentTile.row+1,currentTile.column);
			else nextTile = GetTile (currentTile.row-1,currentTile.column);
		}
		if (type == "M6" || type == "F6") {
			rotation = -90f;
			renderer.sprite = hintScript.hintImage3;
			Tile p = previous.GetComponent<Tile> ();
			if (currentTile.row == p.row && (p.column - currentTile.column==1))
				nextTile = GetTile (currentTile.row,currentTile.column-1);
			else nextTile = GetTile (currentTile.row,currentTile.column+1);
		}
		if (nextTile != null)
			SetHint (current, nextTile);
		obj.transform.Rotate (new Vector3(0,0,rotation));
	}

	void UpdateCalculations() {
		string path = AssetDatabase.GetAssetPath (board.texture2D);
		board.spriteReferences = AssetDatabase.LoadAllAssetsAtPath (path);
		if (board.texture2D!=null) {
			Sprite sprite = (Sprite)board.spriteReferences [1];
			float width = sprite.textureRect.width;
			float height = sprite.textureRect.height;
			board.tileSize = new Vector2 (width, height);
			board.pixelsToUnits = (int)(sprite.rect.width / sprite.bounds.size.x);
			board.gridSize = new Vector2 ((width / board.pixelsToUnits) * board.mapSize.x, (height / board.pixelsToUnits) * board.mapSize.y);
		}
	}

	void CreateBrush() {
		Sprite sprite = board.currentTileBrush;
		if (sprite != null) {
			GameObject br = new GameObject ("Brush");
			br.transform.SetParent (board.transform);
			brush = br.AddComponent<TileBrush> ();
			brush.renderer2D = br.AddComponent<SpriteRenderer> ();
			brush.renderer2D.sortingOrder = 1000;
			int pixelsToUnit = board.pixelsToUnits;
			brush.brushSize = new Vector2 (sprite.textureRect.width/pixelsToUnit,sprite.textureRect.height/pixelsToUnit);
			brush.UpdateBrush (sprite);
		}
	}

	void MoveBrush() {
		float tileSize = board.tileSize.x / board.pixelsToUnits;
		float x = Mathf.Floor (mouseHitPos.x/tileSize)*tileSize;
		float y = Mathf.Floor (mouseHitPos.y/tileSize)*tileSize;
		float col = x / tileSize;
		float row = Mathf.Abs (y / tileSize) - 1;
		brush.column = (int)(col);
		brush.row = (int)(row);
		if (!mouseOnMap)
			return;
		int id = (int)((brush.row * board.mapSize.x) + brush.column);
		brush.tileID = id;
		x += board.transform.position.x + tileSize / 2;
		y += board.transform.position.y + tileSize / 2;
		brush.transform.position = new Vector3 (x,y,board.transform.position.z);
		if (dragTile && targetTile != null) { 
			if (GetTile (brush.row,brush.column) == null) {
				targetTile.transform.position = new Vector3 (x, y, targetTile.transform.position.z);
				targetTile.GetComponent<Tile> ().row = brush.row;
				targetTile.GetComponent<Tile> ().column = brush.column;
				EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
			}
		}
	}
		

	void Draw() {
		float posX = brush.transform.position.x;
		float posY = brush.transform.position.y;
		if (GetTile(brush.row,brush.column)==null) {
			int unused = GetUnusedTile ();
			GameObject tile = new GameObject ("tile_" + unused);
			tile.tag = "Tile";
			tile.transform.SetParent (board.tiles.transform);
			tile.transform.position = new Vector3 (posX, posY, -1);
			tile.layer = tile.transform.parent.gameObject.layer;
			Tile tileScript=tile.AddComponent<Tile> ();
			tileScript.row = brush.row;
			tileScript.column = brush.column;
			tile.AddComponent<SpriteRenderer> ();
			Rigidbody2D rb=tile.AddComponent<Rigidbody2D> ();
			rb.isKinematic = true;
			rb.constraints = RigidbodyConstraints2D.FreezeRotation;
			BoxCollider2D bc = tile.AddComponent<BoxCollider2D> ();
			bc.size = new Vector3 (board.tileSize.x/board.pixelsToUnits, board.tileSize.y/board.pixelsToUnits, 0);
			tile.GetComponent<SpriteRenderer> ().sprite = brush.renderer2D.sprite;
			DefineTile(tile);
			usedTiles [unused] = 1;
		}
		EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
	}

	void DefineTile(GameObject tile) {
		Tile tileScript = tile.GetComponent<Tile> ();
		if (board.tileID == 1) {
			tileScript.type = "M1";
			AddConnectionPoint (tile,1,1);
			AddConnectionPoint (tile,4,2);
		}
		if (board.tileID == 2) {
			tileScript.type = "M2";
			AddConnectionPoint (tile,1,1);
			AddConnectionPoint (tile,2,2);
		}
		if (board.tileID == 3) {
			tileScript.type = "M3";
			AddConnectionPoint (tile,2,1);
			AddConnectionPoint (tile,3,2);
		}
		if (board.tileID == 4) {
			tileScript.type = "M4";
			AddConnectionPoint (tile,3,1);
			AddConnectionPoint (tile,4,2);
		}
		if (board.tileID == 5) {
			tileScript.type = "M5";
			AddConnectionPoint (tile,1,1);
			AddConnectionPoint (tile,3,2);
		}
		if (board.tileID == 6) {
			tileScript.type = "M6";
			AddConnectionPoint (tile,2,1);
			AddConnectionPoint (tile,4,2);
		}
		if (board.tileID == 7) {
			tileScript.type = "F1";
			AddConnectionPoint (tile,1,1);
			AddConnectionPoint (tile,4,2);
			tileScript.isFixed = true;
		}
		if (board.tileID == 8) {
			tileScript.type = "F2";
			AddConnectionPoint (tile,1,1);
			AddConnectionPoint (tile,2,2);
			tileScript.isFixed = true;
		}
		if (board.tileID == 9) {
			tileScript.type = "F3";
			AddConnectionPoint (tile,2,1);
			AddConnectionPoint (tile,3,2);
			tileScript.isFixed = true;
		}
		if (board.tileID == 10) {
			tileScript.type = "F4";
			AddConnectionPoint (tile,3,1);
			AddConnectionPoint (tile,4,2);
			tileScript.isFixed = true;
		}
		if (board.tileID == 11) {
			tileScript.type = "F5";
			AddConnectionPoint (tile,1,1);
			AddConnectionPoint (tile,3,2);
			tileScript.isFixed = true;
		}
		if (board.tileID == 12) {
			tileScript.type = "F6";
			AddConnectionPoint (tile,2,1);
			AddConnectionPoint (tile,4,2);
			tileScript.isFixed = true;
		}
		if (board.tileID == 13) {
			tileScript.type = "S1";
			AddConnectionPoint (tile,1,1);
			tileScript.isFixed = true;
			tileScript.isStartTile = true;
		}
		if (board.tileID == 14) {
			tileScript.type = "S2";
			AddConnectionPoint (tile,2,1);
			tileScript.isFixed = true;
			tileScript.isStartTile = true;
		}
		if (board.tileID == 15) {
			tileScript.type = "S3";
			AddConnectionPoint (tile,3,1);
			tileScript.isFixed = true;
			tileScript.isStartTile = true;
		}
		if (board.tileID == 16) {
			tileScript.type = "S4";
			AddConnectionPoint (tile,4,1);
			tileScript.isFixed = true;
			tileScript.isStartTile = true;
		}
		if (board.tileID == 17) {
			tileScript.type = "M0";
			tileScript.isSolid = true;
		}
		if (board.tileID == 18) {
			tileScript.type = "M0";
			tileScript.isSolid = true;
		}
		if (board.tileID == 19) {
			tileScript.type = "G1";
			AddConnectionPoint (tile,1,1);
			tileScript.isFixed = true;
			tileScript.isGoalTile = true;
		}
		if (board.tileID == 20) {
			tileScript.type = "G2";
			AddConnectionPoint (tile,2,1);
			tileScript.isFixed = true;
			tileScript.isGoalTile = true;
		}
		if (board.tileID == 21) {
			tileScript.type = "G3";
			AddConnectionPoint (tile,3,1);
			tileScript.isFixed = true;
			tileScript.isGoalTile = true;
		}
		if (board.tileID == 22) {
			tileScript.type = "G4";
			AddConnectionPoint (tile,4,1);
			tileScript.isFixed = true;
			tileScript.isGoalTile = true;
		}
	}

	void RemoveTile() {
		GameObject tile = GetTile (brush.row,brush.column);
		if (tile != null) {
			string name = tile.gameObject.name;
			int pos = name.IndexOf ("_");
			string num = name.Substring (pos + 1, name.Length - pos - 1);
			int id = Int32.Parse (num);
			usedTiles[id] = 0;
			DestroyImmediate (tile);
		}
		EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
	}

	void ClearMap(){
		for (var i = 0; i < board.tiles.transform.childCount; i++) {
			Transform t = board.tiles.transform.GetChild(i);
			DestroyImmediate(t.gameObject);
			i--;
		}
		for (int i = 0; i < usedTiles.Length; i++)
			usedTiles [i] = 0;
		hint = GameObject.Find ("Hint");
		Transform[] childObjects = null;
		if (hint != null) {
			childObjects = hint.transform.GetComponentsInChildren<Transform> ();
			for (int i = 1; i < childObjects.Length; i++)
				DestroyImmediate (childObjects [i].gameObject);
		}
		NewBrush ();
		EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
	}

	int GetUnusedTile() {
		int n = 0;
		if (usedTiles.Length > 0) {
			for (int i = 0; i < usedTiles.Length; i++)
				if (usedTiles [i] == 0) {
					n = i; 
					break;
				}
		}
		return n;
	}

	void AddConnectionPoint(GameObject tile, int position, int count) {
		GameObject collider = new GameObject ("connection_point_"+count);
		float w = board.tileSize.x/board.pixelsToUnits;
		float h = board.tileSize.y/board.pixelsToUnits;
		collider.transform.SetParent (tile.transform);
		collider.tag = "connection_point";
		collider.AddComponent<ConnectionPoint> ();
		if (position==1)
			collider.transform.localPosition = new Vector3 (0,-h/2,tile.transform.position.z);
		if (position==2)
			collider.transform.localPosition = new Vector3 (-w/2,0,tile.transform.position.z);
		if (position==3)
			collider.transform.localPosition = new Vector3 (0,h/2,tile.transform.position.z);
		if (position==4)
			collider.transform.localPosition = new Vector3 (w/2,0,tile.transform.position.z);
		BoxCollider2D bc = collider.AddComponent<BoxCollider2D> ();
		bc.isTrigger = true;
		bc.size = new Vector2 (0.2f,0.2f);
	}

	void CalculateUsedTiles() {
		Transform[] tiles = null;
		if (GameObject.Find ("Tiles").transform.childCount > 0) 
			tiles = GameObject.Find ("Tiles").GetComponentsInChildren<Transform> ();
		if (tiles != null) {
			for (int i = 1; i < tiles.Length; i++) {
				if (tiles [i].gameObject.name.StartsWith ("tile_")) {
					string name = tiles [i].gameObject.name;
					int pos = name.IndexOf ("_");
					string num = name.Substring (pos + 1, name.Length - pos - 1);
					int id = Int32.Parse (num);
					usedTiles [id] = 1;
				}
			}
		} else
			for (int i = 0; i < usedTiles.Length; i++)
				usedTiles [i] = 0;
	}

	GameObject GetTile(int row, int column) {
		GameObject tile = null;
		Transform[] tiles = GameObject.Find ("Tiles").GetComponentsInChildren<Transform> ();
		for (int i = 0; i < tiles.Length; i++) {
			Tile t = tiles [i].gameObject.GetComponent<Tile> ();
			if (t!=null)
			if (t.row == row && t.column==column)
				tile=tiles[i].gameObject;
		}
		return tile;
	}

	void NewBrush() {
		if (brush == null)
			CreateBrush ();
	}

	void DestroyBrush() {
		if (brush != null)
			DestroyImmediate (brush.gameObject);
	}
}
