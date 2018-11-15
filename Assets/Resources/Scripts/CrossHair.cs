using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossHair : MonoBehaviour {

	// Use this for initialization
	OverlayControl overlayControl;
	void Start () {
		overlayControl=GetComponent<OverlayControl>();
	}
	
	// Update is called once per frame
	void Update () {
		if(!overlayControl.isActive()) overlayControl.Show();
	}
}
