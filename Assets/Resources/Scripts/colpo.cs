using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class colpo : MonoBehaviour {

	// Use this for initialization
	private CharacterController cc;
	private int damage;
	private bool player;
	public float ttl=10f;
	void Start () {
		cc=GetComponent<CharacterController>();
		cc.enabled=true;
	}
	
	void Update () {
		ttl-=Time.deltaTime;
		cc.Move(transform.TransformDirection(Vector3.forward)*50*Time.deltaTime);
		if(ttl<=0){
			Destroy(gameObject);
		}
	}
    void OnControllerColliderHit(ControllerColliderHit hit) {
		if(player && hit.gameObject.tag=="enemy"){
			hit.gameObject.GetComponent<enemy>().damage(damage);
		}
		else if(hit.gameObject.tag=="enemy"){
			hit.gameObject.GetComponent<enemy>().sanguina(damage);
		}
		if(hit.gameObject.tag!="shot"){
			Destroy(gameObject);
		}
    }

	public void setDamage(int d){
		damage=d;
		Color c=(d < 15) ? new Color(0.19215686274f,0.89411764705f,1,1) : (d < 25) ? new Color(0.95686274509f,0.19215686274f,1,1) : (d < 100) ? new Color(1,0.98823529411f,0.07058823529f,1) : new Color(0.57254902f,0,0,1);
		GetComponent<Light>().color=c;
		GetComponent<Renderer>().material.color=c;

	}

	public int getDamage(){
		return damage;
	}

	public void setPlayer(){
		player=!player;
	}

	public bool getPalyer(){
		return player;
	}
}
