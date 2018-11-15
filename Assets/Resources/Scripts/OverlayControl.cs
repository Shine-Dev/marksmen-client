using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverlayControl : MonoBehaviour {

	// Use this for initialization
	public int rank; 
	
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void Show(){
		transform.localScale=Vector3.one;
	}

	public void Hide(){
		transform.localScale=Vector3.zero;
	}

	public bool isActive(){
		return transform.localScale==Vector3.one;
	}


}
