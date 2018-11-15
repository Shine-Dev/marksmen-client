using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class KeyMenu : MonoBehaviour {

	// Use this for initialization
	public float keyCoolDown=15f;
	private float keyCoolDownLeft;
	private List<Text> qts=new List<Text>();
	private List<int> voti;
	private Text mat1key;
	private Text mat2key;
	private Text cooldown;
	private OverlayControl overlayControl;
	private bool enabled=true;

	void Start () {
		for (int i=2;i<11;i++){
			qts.Add(GameObject.Find(i+"qt").GetComponent<Text>());
		}
		mat1key=GameObject.Find("mat1key").GetComponent<Text>();
		mat2key=GameObject.Find("mat2key").GetComponent<Text>();
		cooldown=GameObject.Find("cooldown").GetComponent<Text>();
		overlayControl=GetComponent<OverlayControl>();
	}

	public void initialize(string materia1,string materia2,List<int> voti){
		keyCoolDownLeft=0;
		mat1key.text=materia1;
		mat2key.text=materia2;
		this.voti=voti;
		for (int i=0;i<9;i++){
			qts.ElementAt(i).text=voti.FindAll(x=>x==i+2).Count.ToString();
		}
	}

	public void setEnabled(bool e){
		enabled=e;
	}

	public int canUse(int k){
		return voti.Find(x=>x>=k) == 0 ? -1 : keyCoolDownLeft>0 ? Mathf.RoundToInt(keyCoolDownLeft) : 0;
	}

	public bool useKey(int voto){
		int index=getAppropiateKeyIndex(voto);
		if(index!=-1 && keyCoolDownLeft<=0){
			int votousato=voti[index];
			voti.RemoveAt(index);
			qts[votousato-2].text=voti.FindAll(x=>x==votousato).Count.ToString();
			keyCoolDownLeft=keyCoolDown;
			return true;
		}
		else{
			return false;
		}
	}

	public int getAppropiateKeyIndex(int voto){
		return voti.IndexOf(voti.FindAll(x=> x>=voto).OrderBy(x=>x).FirstOrDefault());
	}
	
	// Update is called once per frame
	void Update () {
		if(enabled){
			keyCoolDownLeft-=Time.deltaTime;
			cooldown.text=keyCoolDownLeft>=0 ? "Cooldown chiavi: "+ Mathf.RoundToInt(keyCoolDownLeft) +"s" : "";
			if(Input.GetKey(KeyCode.Tab)){
				overlayControl.Show();
			}
			else{
				overlayControl.Hide();
			}
		}
	}
}
