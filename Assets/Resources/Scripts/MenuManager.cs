using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class MenuManager : MonoBehaviour {

	// Use this for initialization
	private InputField usernamein;
    private InputField passwordin;
	private InputField ipin;
	private Button LButton;
	private Toggle ricordami;
	private GameObject loginerror;
	private NetworkManager nm; 
	void Start () {
		usernamein = GameObject.Find("Username").GetComponent<InputField>();
        passwordin = GameObject.Find("Password").GetComponent<InputField>();
		ipin = GameObject.Find("IP").GetComponent<InputField>();
        loginerror = GameObject.Find("loginerror");
        LButton=GameObject.Find("LButton").GetComponent<Button>();
		ricordami=GameObject.Find("ricordami").GetComponent<Toggle>();
		nm=GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
		try{
			StreamReader r=new StreamReader(@"ricordami.json");
			RicordamiConfig rc=JsonUtility.FromJson<RicordamiConfig>(r.ReadToEnd());
			usernamein.text=rc.username;
			passwordin.text=rc.password;
			ipin.text=rc.ip;
			r.Close();
		}
		catch(System.Exception e){
			Debug.Log(e);
		}
	}

	public void login(){
		StatusLB();
		StartCoroutine(nm.lcv(usernamein.text,passwordin.text,ipin.text,LoginFailure,LoginSuccess));
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKey(KeyCode.Return) && LButton.enabled) login();
		if(Input.GetKey(KeyCode.Backslash)) nm.FakeLogin();
	}

	void LoginFailure(){
		StatusLB();
		loginerror.transform.localScale=Vector3.one;
	}

	void LoginSuccess(){
		if(ricordami.isOn){
			RicordamiConfig rc=new RicordamiConfig();
			rc.username=usernamein.text;
			rc.password=passwordin.text;
			rc.ip=ipin.text;
			using(StreamWriter w=new StreamWriter(@"ricordami.json")){
					w.Write(JsonUtility.ToJson(rc));
					w.Close();
			}
		}
	}

	void StatusLB(){
        LButton.enabled=!LButton.enabled;
        LButton.transform.Find("Text").GetComponent<Text>().text=LButton.enabled ? "Login" : "Connessione...";
    }

	private class RicordamiConfig{
		public string username;
		public string password;
		public string ip;
	}

	public void GameClose(){
		Application.Quit();
	}
}

