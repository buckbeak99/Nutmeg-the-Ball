using UnityEngine;
using System.Collections;

public class ConnectionPoint : MonoBehaviour {
	public GameObject connectedTile=null;
	private Tile parentTile;

	void OnTriggerEnter2D(Collider2D c) {
		if (c.gameObject.tag == "connection_point") {
			connectedTile = c.transform.parent.gameObject;
			if (name == "connection_point_1")
				parentTile.p1Connected = true;
			if (name == "connection_point_2")
				parentTile.p2Connected = true;
		}
	}

	void OnTriggerExit2D(Collider2D c) {
		if (c.gameObject.tag == "connection_point") {
			connectedTile = null;
			if (name == "connection_point_1")
				parentTile.p1Connected = false;
			if (name == "connection_point_2")
				parentTile.p2Connected = false;
		}
	}

	void Awake() {
		parentTile = transform.parent.gameObject.GetComponent<Tile> ();
	}

}
