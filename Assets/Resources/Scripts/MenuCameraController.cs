using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuCameraController : MonoBehaviour {

	// Use this for initialization
	Transform focalPoint;
	void Start () {
		focalPoint=GameObject.Find("focalPoint").transform;
	}
	
	// Update is called once per frame
	void Update () {
		transform.RotateAround (focalPoint.position, Vector3.up, 5 * Time.deltaTime);
	}
}
