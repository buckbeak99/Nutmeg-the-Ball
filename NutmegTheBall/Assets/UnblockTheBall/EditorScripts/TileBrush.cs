using UnityEngine;
using System.Collections;

public class TileBrush : MonoBehaviour {

	public Vector2 brushSize = Vector2.zero;
	public int tileID = 0;
	public SpriteRenderer renderer2D;
	public int column, row;

	public void UpdateBrush(Sprite sprite) {
		renderer2D.sprite = sprite;
	}

	void OnDrawGizmosSelected() {
		Gizmos.color = Color.red;
		Gizmos.DrawWireCube (transform.position, brushSize);
	}
}
