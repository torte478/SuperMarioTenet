using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour, ITimelined
{
	private GameObject target;

	public float followAhead = 2.6f;
	public float smoothing = 5;
	public bool canMove;
	public bool canMoveBackward = false;

	private Transform leftEdge;
	private Transform rightEdge;
	private float cameraWidth;
	private Vector3 targetPosition;

	public Timeline timeline;
	public GameObject mario;
	public GameObject invertedMario;

    public bool Replaying => throw new System.NotImplementedException();


    // Use this for initialization
    void Start () {

		target = mario;
		timeline.OnInverted += OnTimelineInverted;

		GameObject boundary = GameObject.Find ("Level Boundary");
		leftEdge = boundary.transform.Find ("Left Boundary").transform;
		rightEdge = boundary.transform.Find ("Right Boundary").transform;
		float aspectRatio = GetComponent<MainCameraAspectRatio> ().targetAspects.x /
		                    GetComponent<MainCameraAspectRatio> ().targetAspects.y;
		cameraWidth = Camera.main.orthographicSize * aspectRatio;

		// Initialize camera's position
		Vector3 spawnPosition = FindObjectOfType<LevelManager>().FindSpawnPosition();
		targetPosition = new Vector3 (spawnPosition.x, transform.position.y, transform.position.z);

		bool passedLeftEdge = targetPosition.x < leftEdge.position.x + cameraWidth;

		if (rightEdge.position.x - leftEdge.position.x <= cameraWidth * 2) {  // center camera if already within boundaries
			transform.position = new Vector3 ((leftEdge.position.x + rightEdge.position.x) / 2f, targetPosition.y, targetPosition.z);
			canMove = false;
		} else if (passedLeftEdge) { // do not let camera shoot pass left edge
			transform.position = new Vector3 (leftEdge.position.x + cameraWidth, targetPosition.y, targetPosition.z);
			canMove = true;
		} else {
			transform.position = new Vector3 (targetPosition.x + followAhead, targetPosition.y, targetPosition.z);
			canMove = true;
		}
	}

    private void OnTimelineInverted(int direction)
    {
		if (direction > 0)
        {
			target = mario;
			canMoveBackward = false;
        }
		else
        {
			target = invertedMario;
			canMoveBackward = true;
        }

		transform.position = new Vector3(target.transform.position.x, transform.position.y, transform.position.z);
	}


    // Update is called once per frame
    void Update () {
		if (!canMove) return;

		bool passedLeftEdge = transform.position.x < leftEdge.position.x + cameraWidth;
		bool passedRightEdge = transform.position.x > rightEdge.position.x - cameraWidth;

		targetPosition = new Vector3(target.transform.position.x, transform.position.y, transform.position.z);

		if (!passedRightEdge && targetPosition.x - leftEdge.position.x >= cameraWidth - followAhead && !canMoveBackward)
		{
			targetPosition = new Vector3(targetPosition.x + followAhead, targetPosition.y, targetPosition.z);
			if (targetPosition.x > transform.position.x)
				transform.position = Vector3.Lerp(transform.position, targetPosition, smoothing * Time.deltaTime);
		}
		else if (canMoveBackward && !passedLeftEdge && rightEdge.position.x - targetPosition.x >= cameraWidth - followAhead)
		{
			targetPosition = new Vector3(targetPosition.x - followAhead, targetPosition.y, targetPosition.z);
			if (targetPosition.x < transform.position.x)
				transform.position = Vector3.Lerp(transform.position, targetPosition, smoothing * Time.deltaTime);
		}
	}

    public void Play(ISnapshot snapshot)
    {
        throw new System.NotImplementedException();
    }
}