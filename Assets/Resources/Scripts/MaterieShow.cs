using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MaterieShow : MonoBehaviour {

	// Use this for initialization
	public float duration;
	private float timeLeftCounter;
	private GameObject mat1;
	private GameObject mat2;
	private OverlayControl overlayControl;

	void Start () {
		mat1=GameObject.Find("materia1");
		mat2=GameObject.Find("materia2");
		overlayControl=GetComponent<OverlayControl>();
	}
	
	// Update is called once per frame
	void Update () {
		if(overlayControl.isActive()){
			timeLeftCounter-=Time.deltaTime;
			if(timeLeftCounter<=0) overlayControl.Hide();
		}
	}

	public void Start(string materia1,string materia2,double media1,double media2){
			timeLeftCounter=duration;
			overlayControl.Show();
			mat1.GetComponent<Text>().text=materia1.ToUpper();
			mat1.GetComponent<Text>().color=(media1 < 5) ? new Color(0.345098039f,0.874509804f,0.925490196f,1) : (media1 < 8) ? new Color(0.803921569f,0.341176471f,0.945098039f,1) : new Color(0.905882353f,0.894117647f,0,1);
			mat1.GetComponent<Shadow>().effectColor=(media1 < 5) ? new Color(0.043137255f,0.368627451f,1,1) : (media1 < 8) ? new Color(0.815686275f,0.043137255f,0.501960784f,1) : new Color(1,0.643137255f,0.043137255f,1);
			mat1.GetComponent<ShakingText>().ricarica();
			mat2.GetComponent<Text>().color=(media2 < 5) ? new Color(0.345098039f,0.874509804f,0.925490196f,1) : (media2 < 8) ? new Color(0.803921569f,0.341176471f,0.945098039f,1) : new Color(0.905882353f,0.894117647f,0,1);
			mat2.GetComponent<Shadow>().effectColor=(media2 < 5) ? new Color(0.043137255f,0.368627451f,1,1) : (media2 < 8) ? new Color(0.815686275f,0.043137255f,0.501960784f,1) : new Color(1,0.643137255f,0.043137255f,1);
			mat2.GetComponent<Text>().text=materia2.ToUpper();
			mat2.GetComponent<ShakingText>().ricarica();
	}

	public void Stop(){
		overlayControl.Hide();
		transform.localScale=Vector3.zero;
	}
}
