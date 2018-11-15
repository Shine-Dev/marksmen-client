using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShakingText : MonoBehaviour {

	// Use this for initialization
	public float maxShake;
	private Vector3 startPos;
	public float time;

	private float timeLeft;


	void Start () {
		startPos=transform.localPosition;
		ricarica();
	}

	public void ricarica(){
		timeLeft=time;
	}
	
	// Update is called once per frame
	void Update () {
		timeLeft-=Time.deltaTime;
		if(timeLeft>=0){
			transform.localPosition=startPos+new Vector3(Random.Range(-maxShake,maxShake),Random.Range(-maxShake,maxShake),0.0f);
		}
		else{
			transform.localPosition=startPos;
		}
	}
}
