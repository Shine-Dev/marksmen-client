using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoostShow : MonoBehaviour {

	// Use this for initialization
	private OverlayControl overlayControl;
	private GameObject boostText;
	private GameObject plusText;
	private GameObject dmText;
	private float timeLeft;
	public float time;
	void Start () {
		overlayControl=GetComponentInParent<OverlayControl>();
		boostText=GameObject.Find("boost");
		plusText=GameObject.Find("plus");
		dmText=GameObject.Find("dmboost");
	}
	
	// Update is called once per frame
	void Update () {
		if(overlayControl.isActive() && timeLeft>=0){
			timeLeft-=Time.deltaTime;
		}
		else if(overlayControl.isActive() && timeLeft<=0){
			overlayControl.Hide();
		}
	}

	public void Show(int boost){
		timeLeft=time;
		Color textColor=(boost < 5) ? new Color(0.345098039f,0.874509804f,0.925490196f,1) : (boost < 8) ? new Color(0.803921569f,0.341176471f,0.945098039f,1) : new Color(0.905882353f,0.894117647f,0,1);
		Color shadowColor=(boost < 5) ? new Color(0.043137255f,0.368627451f,1,1) : (boost < 8) ? new Color(0.815686275f,0.043137255f,0.501960784f,1) : new Color(1,0.643137255f,0.043137255f,1);
		plusText.GetComponent<Text>().color=textColor;
		plusText.GetComponent<Shadow>().effectColor=shadowColor;
		plusText.GetComponent<ShakingText>().ricarica();
		boostText.GetComponent<Text>().color=textColor;
		boostText.GetComponent<Shadow>().effectColor=shadowColor;
		boostText.GetComponent<Text>().text=boost.ToString();
		boostText.GetComponent<ShakingText>().ricarica();
		dmText.GetComponent<Text>().color=textColor;
		dmText.GetComponent<Shadow>().effectColor=shadowColor;
		dmText.GetComponent<ShakingText>().ricarica();
		overlayControl.Show();
	}

	
}
