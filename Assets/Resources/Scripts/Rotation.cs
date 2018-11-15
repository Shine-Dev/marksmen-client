using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotation : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		transform.rotation=Quaternion.Euler(0,transform.rotation.eulerAngles.y+(75*Time.deltaTime),0);
	}
}
