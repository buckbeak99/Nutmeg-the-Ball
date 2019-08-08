using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour {

	//this property is used for identifying type of a tile
	//M0 - solid tile, meaning it can be moved, but it doesn't contain a path
	//M1,M2,M3,M4,M5,M6 - movable tiles
	//F1,F2,F3,F4,F5,F6 - the same tiles, but they are fixed and can't be moved
	//S1,S2,S3,S4 - start tiles. Only one tile can be a start point
	//G1,G2,G3,G4 - goal tiles. Only one tile can be a goal point
	[HideInInspector] public string type;

	[HideInInspector] public static float movSpeed=15f;

	//Define if the tile is Goal or Start tile
	[HideInInspector] public bool isStartTile = false, isGoalTile=false;

	//Define if the tile is fixed, if so, it can't be moved
	[HideInInspector] public bool isFixed = false;

	//Define if tile just can be moved, but doesn't contain a path. It is always M0 tile
	[HideInInspector] public bool isSolid = false;

	//Define if tile is rotatable
	[HideInInspector] public bool isRotatable = false;

	//Define if tile's connection points are connected
	[HideInInspector] public bool p1Connected=false, p2Connected=false;

	//row and column properties are used only for calculating a hint path in editor
	[HideInInspector] public int row,column;

	[HideInInspector] public bool rotating;

	//We need connection points for checking connections between tiles
	private GameObject connectionPoint1=null,connectionPoint2=null;

	//If all connection points are connected, the tile is marked as connected
	private bool connected=false;

	private float rotationSpeed = 900f;

	private bool scaleAnim = false;
	private float randomTime,timer;
	private Rigidbody2D rb;
	private float rotation,targetZ;


	void Awake() {
		rb = GetComponent<Rigidbody2D> ();
		bool startOrGoalTile = isGoalTile || isStartTile;
		if (!startOrGoalTile) {
			GetComponent<SpriteRenderer> ().color = Color.clear;
			randomTime = Random.Range (0.2f, 0.6f);
			transform.localScale = new Vector3 (0.1f, 0.1f, 0.1f);
		}
		//If the tile is not M0, then it has connection points and we will get the references to the objects
		if (!isSolid) {
			GameObject p1 = transform.Find ("connection_point_1").gameObject;
			if (p1 != null)
				connectionPoint1 = p1;
			//Start tile and Goal tile have only one connection point
			//In other case we will get second connection point
			if (!isStartTile && !isGoalTile) {
				GameObject p2 = transform.Find ("connection_point_2").gameObject;
				if (p2 != null)
					connectionPoint2 = p2;
			}
		}
	}

	public Vector2 getConnectionPoint1Pos() {
		return new Vector2 (connectionPoint1.transform.position.x,connectionPoint1.transform.position.y);
	}

	public void ScaleAnim() {
		Vector3 temp = transform.localScale;
		if (temp.x < 1)
			temp.Set (temp.x + 3f*Time.deltaTime, temp.y + 3f*Time.deltaTime, temp.z + 3f*Time.deltaTime);
		else {
			temp.Set (1f,1f,1f);
			scaleAnim = false;
		}
		transform.localScale = temp;
	}

	//Check if tile is marked as connected. This is used while gameplay, to check if player have finished the path
	public bool IsConnected() {
		if (connectionPoint2 == null)
			connected = p1Connected;
		if (connectionPoint2 != null)
			connected = p1Connected && p2Connected;
		return connected;
	}

	public void BeginRotation() {
		rotation = transform.eulerAngles.z;
		targetZ = transform.eulerAngles.z - 90f;
		rotating = true;
	}

	void Update() {
		if (!scaleAnim && transform.localScale.x<1f)
		timer += Time.deltaTime;
		if (timer > randomTime && transform.localScale.x < 1f && !scaleAnim) {
			scaleAnim = true;
			GetComponent<SpriteRenderer> ().color = Color.white;
		}
		bool startOrGoalTile = isGoalTile || isStartTile;
		if (scaleAnim && !startOrGoalTile)
			ScaleAnim ();
		if (GameManager.instance.goalAchieved && rb!=null) {
			GetComponent<BoxCollider2D> ().isTrigger = true;
			if (!connected && rb.isKinematic && !startOrGoalTile) {
				GetComponent<SpriteRenderer> ().sortingOrder = 20;
				rb.isKinematic = false;
				rb.constraints = RigidbodyConstraints2D.None;
				float xForce = Random.Range (2f,6f);
				float yForce = Random.Range (4f,8f);
				if (transform.position.x > 0)
					xForce *= -1;
				rb.AddForce (new Vector2 (xForce, yForce), ForceMode2D.Impulse);
				rb.AddTorque (360,ForceMode2D.Impulse);
			}
			if (transform.position.y < -20) {
				transform.position = new Vector3 (transform.position.x,-20,transform.position.z);
				Destroy (rb);
			}
		}
			
		if (rotating) {
			if (rotation - Time.deltaTime * rotationSpeed> targetZ) {
				rotation -= Time.deltaTime * rotationSpeed;
				transform.eulerAngles = new Vector3 (transform.eulerAngles.x,transform.eulerAngles.y,rotation);
			} else {
				transform.eulerAngles = new Vector3 (transform.eulerAngles.x,transform.eulerAngles.y,targetZ);
				rotating = false;
			}
		}
			
	}
		
}
