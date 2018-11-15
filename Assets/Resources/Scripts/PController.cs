using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class PController : MonoBehaviour {

	// Use this for initialization
	public float speed;
	private Animator animator;
	private GameManager gameManager;
	private CharacterController cc;
	private Transform shootPosition;
	private Quaternion lastRot;
	private Quaternion lastArmRot;

	private ParticleSystem ps;

	private bool armrotsended;
	private Vector3 lastDir;
	private Vector3 lastPos;

	private Transform arm;

	private int hp;
	private int damage;
	public float updateCoolDown=1f;
	public float lastPosCoolDown=0.5f;
	private float shotCooldownLeft=0.25f;

	private float jumpCoolDown=0.5f;
	private Vector3 direction;
	private bool immune;
	private float vspeed=0;
	private GameObject shot;

	void Start () {
		cc=GetComponent<CharacterController>();
		animator=GetComponent<Animator>();
		ps=GetComponent<ParticleSystem>();
		gameManager=GameObject.Find("GameManager").GetComponent<GameManager>();
		//usernameTxt=transform.Find("username").GetComponent<TextMesh>();
		arm=transform.Find("Armature/chest/arm.001");
		shootPosition=arm.Find("sparo");
		shot=Resources.Load<GameObject>("Prefabs/shot");
		initialize();
		Camera.main.GetComponent<CameraController>().setLookAt(transform);
		transform.Find("Cube").GetComponent<Renderer>().material.mainTexture=Resources.Load<Texture>("Assets/av3/avecolor "+(gameManager.baseDamage==-1 ? "4" : hp>80 ? "3" : hp>60 ? "2" : "1"));
	}

	void OnControllerColliderHit(ControllerColliderHit hit) {
		if(hit.gameObject.tag=="shot"){
			Physics.IgnoreCollision(hit.collider,GetComponent<Collider>());
		}
    }

	public void setArmRotation(float xrot){
		Quaternion r=Quaternion.Euler(arm.rotation.eulerAngles.x - (-(xrot) + 90),arm.rotation.eulerAngles.y,arm.rotation.eulerAngles.z);
		lastArmRot=r;
		arm.rotation=r;
		armrotsended=false;
	}

	// Update is called once per frame
	void Update () {
				shotCooldownLeft-=Time.deltaTime;
				updateCoolDown-=Time.deltaTime;
				lastPosCoolDown-=Time.deltaTime;
				jumpCoolDown-=Time.deltaTime;

				float h = Input.GetAxis("Horizontal");
				float v = Input.GetAxis("Vertical");
				bool b=Input.GetKey(KeyCode.LeftShift);

				animator.SetBool("walking",(Mathf.Abs(h)>0 || Mathf.Abs(v)>0));
				animator.SetBool("running",b);

				Vector3 dir = new Vector3(h, 0, v);
				direction = transform.TransformDirection(dir) * speed * (b ? 1.5f : 1);
				
				if(Input.GetKey(KeyCode.Mouse0) && shotCooldownLeft<=0 && !b){
					shotCooldownLeft=0.25f;
					gameManager.netShoot(damage);
					GameObject o=GameObject.Instantiate(shot,shootPosition.position,shootPosition.rotation);
					o.GetComponent<colpo>().setDamage(damage);
					o.GetComponent<colpo>().setPlayer();
				}

				if(updateCoolDown>0){
					if(lastRot!=transform.rotation){
						gameManager.netRot(transform.rotation);
						lastRot=transform.rotation;
					}

					if(lastDir != direction){
						gameManager.netDir(direction,b);
						lastDir=direction;
					}

					if(!armrotsended){
						gameManager.netArmRot(arm.rotation);
						armrotsended=true;
					}

					if(lastPos != transform.position && lastPosCoolDown<=0){
						gameManager.netPos(transform.position);
						lastPosCoolDown=0.15f;
						lastPos=transform.position;
					}
				}
				else{
					gameManager.netUpdate(transform.position,direction,b,transform.rotation,arm.rotation);
					updateCoolDown=0.5f;
				}

				if(cc.isGrounded && Input.GetKeyDown(KeyCode.Space) && jumpCoolDown<=0){
					gameManager.netJump();
					vspeed=7;
				}
				else if(!cc.isGrounded){
					vspeed-=20*Time.deltaTime;
				}

			direction.y=vspeed;

			cc.Move(direction*Time.deltaTime);
	}

	void LateUpdate(){
		if(!Input.GetKey(KeyCode.LeftShift)){
			arm.rotation=lastArmRot;
		}
	}
	
	public void initialize(){
		immune=false;
		hp=gameManager.baseHpMax;
		if(gameManager.baseDamage!=-1){
			damage=gameManager.baseDamage;
		}
		else{
			damage=5;
		}
		gameManager.setHpUI(hp);
		gameManager.setDamageUI(damage);
	}

	//public void damage(int d){
	public void applyDamage(int d,int id){
		if(!immune){
			hp-=d;
			ParticleSystem.EmissionModule p=ps.emission;
			p.rateOverTime=new ParticleSystem.MinMaxCurve(damage*4, damage*8);
			ps.Play();
			if(hp<=0){ 
				hp=gameManager.baseHpMax;
				gameManager.Dead(id);
				Destroy(gameObject);
			}
			gameManager.setHpUI(hp);
		}
	}

	public bool getImmune(){
		return immune;
	}

	public void setImmune(bool i){
		immune=i;
	}

	public void applyBoost(int boost){
		damage+=boost==-1 ? damage : boost;
		gameManager.setDamageUI(damage);
	}

}
