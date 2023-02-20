using UnityEngine;
using System.Collections;
using AStar;

public class Unit : MonoBehaviour {

	public Transform target;
	public float speed = 3f;
	public bool showPath = false;

	Vector3[] path;
	int targetIndex;

	// Use this for initialization
	IEnumerator Start () {
		yield return null;
		yield return null;
		yield return null;
		//PathRequestManager.RequestPath(transform.position, target.position, OnPathReceived);
	}

	void Update () {
		if (Input.GetKeyUp(KeyCode.A)) {
			if (GameObject.Find("A*").GetComponent<AStar.Grid>()) {
                //Debug.Log(GameObject.Find("A*").GetComponent<Grid>().NodeFromWorldPoint(transform.position).gridPosition);
                AStar.PathRequestManager.RequestPath(transform.position, target.position, OnPathReceived);
			}
		}
	}

	void OnPathReceived (Vector3[] newPath, bool success) {
		if (success) {
			path = newPath;
			StopCoroutine("FollowPath");
			StartCoroutine("FollowPath");
		}
	}

	IEnumerator FollowPath () {
		Vector3 currentWaypoint = path[0];

		while (true) {
			if (transform.position == currentWaypoint) {
				targetIndex++;
				if (targetIndex >= path.Length) {
					yield break;
				}
				currentWaypoint = path[targetIndex];
			}
			transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, speed*Time.deltaTime);
			yield return null;
		}
	}

	public void OnDrawGizmos () {
		if (showPath && path != null) {
			for (int i = targetIndex; i < path.Length; i++) {
				Gizmos.color = Color.black;
				Gizmos.DrawCube(path[i], Vector3.one*0.3f);
				if (i == targetIndex) {
					Gizmos.DrawLine(transform.position, path[i]);
				}
				else {
					Gizmos.DrawLine(path[i-1], path[i]);
				}
			}
		}
	}
}
