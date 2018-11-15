using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Go : MonoBehaviour {

	public float seconds;
	float secondsremaining;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		secondsremaining-=Time.deltaTime;
		if(secondsremaining<=0){
			transform.localScale=Vector3.zero;
		}
	}

	public void Show(){
		secondsremaining=seconds;
		transform.localScale=Vector3.one;
	}

}
