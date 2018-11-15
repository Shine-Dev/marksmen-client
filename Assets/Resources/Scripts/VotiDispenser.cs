using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VotiDispenser : MonoBehaviour {

	// Use this for initialization
	public int votoNeeded;
	private GameManager gameManager;
	private BottomAlert bottomalert;
	private BoostShow boostShow;
	private bool triggered;
	void Start () {
		gameManager=GameObject.Find("GameManager").GetComponent<GameManager>();
		bottomalert=GameObject.Find("wepalert").GetComponent<BottomAlert>();
		boostShow=GameObject.Find("boostshow").GetComponent<BoostShow>();
	}
	
	// Update is called once per frame
	void Update () {
		if(gameManager.baseDamage!=-1){
			if(gameManager.player != null && Vector3.Distance(gameManager.player.transform.position,transform.position)<1.4f){
				triggered=true;
				int canUse=gameManager.keyMenu.canUse(votoNeeded);
				bottomalert.show(canUse==0 ? "Premi 'E' per usare" : canUse>0 ? "Ricarica in corso "+ canUse +"s.." : "Chiavi insufficenti!",(canUse==-1));
				if(Input.GetKeyDown(KeyCode.E) && gameManager.keyMenu.useKey(votoNeeded)){
					int v=Random.Range(1,100);
					int boost=v < 60 ? votoNeeded-2 : v <90 ? votoNeeded-1 : votoNeeded;
					boostShow.Show(boost);
					gameManager.player.GetComponent<PController>().applyBoost(boost);
				}
			}
			else if(triggered && gameManager.player != null && Vector3.Distance(gameManager.player.transform.position,transform.position)>0.8f){
				triggered=false;
				bottomalert.hide();
			}
		}
	}
}
