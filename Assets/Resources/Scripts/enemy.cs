using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class enemy : MonoBehaviour {

	// Use this for initialization

	public float speed=5f;
	private Vector3 direction;
	bool boost;

	private Transform arm;

	private Vector3 correction;
	private CharacterController cc;

	private ParticleSystem ps;

	private GameManager gameManager;

	private float correctionTimeout=0.3f;

	private Vector3 lastPos;
	private Animator a;

	private Transform sparo;
	private int kills=0;
	private Quaternion armrot;
	private bool immune;
	private int id;
	private float vspeed;
	private int media;

	private GameObject shot;

	private List<int> spari;

	//private bool jumping;

	void Start () {
		cc=GetComponent<CharacterController>();
		ps=GetComponent<ParticleSystem>();
		a=GetComponent<Animator>();
		spari=new List<int>();
		arm=transform.Find("Armature/chest/arm.001");
		sparo=arm.Find("sparo");
		gameManager=GameObject.Find("GameManager").GetComponent<GameManager>();
		lastPos=transform.position;
		shot=Resources.Load<GameObject>("Prefabs/shotenemy");
	}
	
	// Update is called once per frame
	void Update () {
				
				a.SetBool("walking",(Mathf.Abs(direction.x) > 0.01f || Mathf.Abs(direction.z) > 0.01f));
				a.SetBool("running",boost);

				if(spari.Count>0){
					foreach(int d in spari){
						GameObject o=GameObject.Instantiate(shot,sparo.position,sparo.rotation);
						o.GetComponent<colpo>().setDamage(d);
					}
					spari.Clear();
				}

				if(Vector3.Distance(transform.position,correction)>=0.1f && correction != Vector3.zero){
					Vector3 cor=new Vector3(correction.x,!cc.isGrounded ? transform.position.y : correction.y,correction.z);
					if(correctionTimeout>0){
						correctionTimeout-=Time.deltaTime;
						transform.position=Vector3.Lerp(transform.position,cor,0.05f);
						a.SetBool("walking",true);
					}
					else{
						Debug.Log("timeout");
						transform.position=cor;
					}
				}				
				if(!cc.isGrounded){
					vspeed-=20*Time.deltaTime;
				}
				direction.y=vspeed;
				cc.Move(direction*Time.deltaTime);

				lastPos=transform.position;


				/*if(cc.isGrounded && jumping){
					jumping=false;
				}*/
	}

	void LateUpdate(){
		if(!boost) arm.rotation=armrot;
	}


	public void setKills(int k){
		kills=k;
	}

	public void addKill(){

		kills++;
	}

	public int getKills(){
		return kills;
	}

	public void setMedia(int m){
		media=m;
		transform.Find("Cube").GetComponent<Renderer>().material.mainTexture=Resources.Load<Texture>("Assets/av3/avecolor "+(media==-1 ? "4" : media>8 ? "3" : media>6 ? "2" : "1"));
	}

	public int getMedia(){
		return media;
	}

	public int getId(){
		return id;
	}

	public void setId(int i){
		id=i;
	}

	public void die(){
		transform.localScale=Vector3.zero;
		correction=Vector3.zero;
	}

	public void respawn(Vector3 pos){
		transform.position=pos;
		transform.localScale=new Vector3(0.15f,0.15f,0.2f);
	}

	public void setDirection(Vector3 d,bool b){
		direction=d;
		boost=b;
	}

	public void jump(){
		//jumping=true;
		vspeed=7;
	}

	public void setArmRotation(Quaternion rot){
		armrot=rot;
	}

	public void setCorrection(Vector3 c){
		correctionTimeout=0.3f;
		correction=c;
	}

	public void setRotation(Quaternion rot){
		transform.rotation=rot;
	}

	public bool getImmune(){
		return immune;
	}

	public void setImmune(bool i){
		immune=i;
	}



	public void shoot(int damage){
		spari.Add(damage);
	}

	public void damage(int damage){
		sanguina(damage);
		if(!immune){
			gameManager.playerDamaged(id,damage);
		}
	}

	public void sanguina(int damage){
		ParticleSystem.EmissionModule p=ps.emission;
		p.rateOverTime=new ParticleSystem.MinMaxCurve(damage*4, damage*8);
		ps.Play();
	}
}
