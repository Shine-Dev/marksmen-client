using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BottomAlert : MonoBehaviour {

	// Use this for initialization
	public Text wepalerttext;
	public Image image;
	void Start () {
		wepalerttext=GameObject.Find("wepalerttext").GetComponent<Text>();
		image=GetComponent<Image>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void show(string text,bool error){
		if(error) image.color=new Color(255,0,0,0.5f);
		else image.color=new Color(0,0,0,0.5f);

		wepalerttext.text=text;
		transform.localScale=Vector3.one;
	}

	public void hide(){
		transform.localScale=Vector3.zero;
	}
}
