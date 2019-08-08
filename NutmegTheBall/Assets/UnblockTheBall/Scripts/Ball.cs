using UnityEngine;
using System.Collections.Generic;

public class Ball : MonoBehaviour {

	public bool move = false;
	private List<Transform> waypoints;
	private int targetWaypointNum = 0;
	private Transform targetWaypoint;
	private float movSpeed = 6f;
	private float timer = 0;

	void OnTriggerEnter2D(Collider2D c) {
		if (c.gameObject.tag == "star-collectable" && !GameManager.instance.goalAchieved) {
			c.gameObject.SetActive (false);
			GameManager.instance.stars += 1;
			SoundManager.instance.PlaySound (SoundManager.instance.scoreSound);
		}
	}


	void Start() {
		waypoints = new List<Transform> ();
	}

	public void AddWaypoint(Transform w) {
		waypoints.Add (w);
	}

	public void ClearWaypoints() {
		waypoints.Clear ();
	}

	void FollowPath() {
		transform.position = Vector3.MoveTowards(transform.position, targetWaypoint.position,   movSpeed*Time.deltaTime);
		if(transform.position == targetWaypoint.position)
		{
			if (targetWaypointNum < waypoints.Count - 1) {
				targetWaypointNum += 1;
			} 
			targetWaypoint = waypoints [targetWaypointNum];
		}
	}

	void Update() {
		if (move) {
			if (targetWaypoint == null) {
				targetWaypoint = waypoints [0];
			}
			FollowPath ();
		}
		if (move && targetWaypointNum == waypoints.Count - 1 && transform.position == targetWaypoint.transform.position) {
			if (!GameManager.instance.goalAchieved) {
				SoundManager.instance.PlaySound (SoundManager.instance.goalSound);
				GameManager.instance.goalAchieved = true;
			}
			move = false;
			GameManager.instance.FinishLevel ();
		}
		if (GameManager.instance.goalAchieved) {
			if (timer<1.2f) timer += Time.deltaTime;
			if (timer > 1.2f)
				GameManager.instance.ShowLevelCompletedDialog (GameManager.instance.stars);
		}
	}

}
