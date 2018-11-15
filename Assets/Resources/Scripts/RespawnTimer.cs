using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RespawnTimer : MonoBehaviour {

	// Use this for initialization
	private Text timeleft;
	private GameObject esitoTxt;
	private GameObject materieShow;
	private float timeLeftCounter;
	private OverlayControl overlayControl;

	void Start () {
		timeleft=GameObject.Find("timeleft").GetComponent<Text>();
		esitoTxt=GameObject.Find("esitoTxt");
		overlayControl=GetComponent<OverlayControl>();
	}
	
	// Update is called once per frame
	void Update () {
		if(overlayControl.isActive() && timeLeftCounter>0){
			timeLeftCounter-=Time.deltaTime;
			timeleft.text=System.Convert.ToInt32(timeLeftCounter).ToString();
		}
		else if(overlayControl.isActive()){
			overlayControl.Hide();
		}
	}

	public void Start(float time,string txt){
		if(overlayControl.isActive()) overlayControl.Hide();
		overlayControl.Show();
		timeLeftCounter=time;
		esitoTxt.GetComponent<Text>().text=txt;
		esitoTxt.GetComponent<ShakingText>().ricarica();
	}

	public void Stop(){
		overlayControl.Hide();
	}
}
