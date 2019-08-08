using UnityEngine;
using System.Collections;

public class Board : MonoBehaviour {

	public Vector2 mapSize = new Vector2(4,4);
	public Texture2D texture2D;
	public Vector2 tileSize = new Vector2();
	public Vector2 tilePadding = new Vector2();
	public Vector2 gridSize = new Vector2();
	public Object[] spriteReferences;
	public int pixelsToUnits = 100;
	public int tileID = 0;
	public Vector3 screenBounds;
	[HideInInspector]public GameObject tiles;
	public Sprite boardSprite;

	public Sprite currentTileBrush {
		get {return spriteReferences [tileID] as Sprite;}
	}
		

	void OnDrawGizmosSelected() {
		Vector2 pos = transform.position;
		if (texture2D != null) {
			Gizmos.color = Color.gray;
			Vector3 tile = new Vector3(tileSize.x / pixelsToUnits, tileSize.y / pixelsToUnits);
			Vector2 offset = new Vector2(tile.x / 2, tile.y / 2);
			for (int row=0;row<mapSize.y;row++) {
				for (int column = 0; column < mapSize.x; column++) {
					float newX = (column * tile.x) + offset.x + pos.x-screenBounds.x;
					float newY = -(row * tile.y) - offset.y + pos.y+screenBounds.y;

					Gizmos.DrawWireCube( new Vector2(newX, newY), tile);
				}
			}

			Gizmos.color = Color.white;
			float centerX = pos.x + gridSize.x / 2-screenBounds.x;
			float centerY = pos.y - gridSize.y / 2+screenBounds.y;
			Gizmos.DrawWireCube (new Vector2(centerX,centerY),gridSize);
		}
	}
}
