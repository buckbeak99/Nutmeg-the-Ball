using UnityEngine;
using System.Collections;
using UnityEditor;

public class NewTileMapMenu {

	[MenuItem("GameObject/New Puzzle")]
	public static void CreateTileMap() {
		GameObject puzzle = new GameObject ("Tiles");
		Board puzzleScript = puzzle.AddComponent<Board> ();
		puzzleScript.tilePadding = new Vector2 (2f,2f);
		GameObject board = new GameObject ("Board");
		GameObject boardImage = new GameObject ("board-image");
		board.tag = "Board";
		boardImage.AddComponent<SpriteRenderer> ();
		boardImage.GetComponent<SpriteRenderer> ().sprite=puzzleScript.boardSprite;
		boardImage.transform.SetParent (board.transform);
		boardImage.layer = boardImage.transform.parent.gameObject.layer;
		board.transform.position = new Vector3 (0,0.57f,0);
		Sprite boardSprite = boardImage.GetComponent<SpriteRenderer> ().sprite;
		float newX = -boardSprite.bounds.size.x / 2f + GameManager.boardBorderWidth;
		float newY = boardSprite.bounds.size.y / 2f - GameManager.boardBorderWidth;
		puzzle.transform.SetParent (board.transform);
		puzzle.transform.localPosition = new Vector3 (newX,newY,0);

		UnityEngine.Object hintPrefab = AssetDatabase.LoadAssetAtPath ("Assets/UnblockTheBall/Prefabs/Hint.prefab", typeof(GameObject));
		GameObject hint = PrefabUtility.InstantiatePrefab(hintPrefab) as GameObject;
		hint.name = "Hint";
		hint.transform.parent = board.transform;
		hint.transform.localPosition = new Vector3 (0,0,board.transform.position.z);
	}
}
