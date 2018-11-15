using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Text.RegularExpressions;
public class NetworkManager : MonoBehaviour {

	// Use this for initialization
	public NetworkClient nc;


    private Button LButton;
	private UserData userdata;
	public int port;

    private Configuration configuration;

    public Configuration getConfiguration(){
        return configuration;
    }
	void Start () {
		DontDestroyOnLoad(transform.gameObject);
		nc=new NetworkClient();
        SceneManager.sceneLoaded+=OnSceneLoaded;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    
    //Login & Classeviva Stuff

    void OnSceneLoaded(Scene scene,LoadSceneMode loadSceneMode){
        if(scene.name=="Main" && nc.isConnected){
            nc.Disconnect();
        }
    }

    
	public UserData GetUserData(){
        return userdata;
    }

    public void FakeLogin()
    {
        userdata=new UserData();
        userdata.nome="Test";
        userdata.type="S";
        userdata.cognome="User";
        userdata.materie=new Dictionary<string,List<int>>() {
            {"Italiano", new List<int>(new int[]{Random.Range(2,10),Random.Range(2,10),Random.Range(2,10),Random.Range(2,10)})},
            {"Matematica", new List<int>(new int[]{Random.Range(2,10),Random.Range(2,10),Random.Range(2,10)})},
            {"Giurisprudenza", new List<int>(new int[]{Random.Range(2,10),Random.Range(2,10)})}
        };
        serverConnect("127.0.0.1");
    }

	public void serverConnect(string ip){
        nc=new NetworkClient();
		nc.Connect(ip,1470);
        nc.RegisterHandler(MsgType.Connect,OnConnect);
        nc.RegisterHandler(MsgType.Disconnect,OnDisconnect);
        nc.RegisterHandler(555,OnConfig);
	}

    void OnConnect(NetworkMessage netMsg){
        Debug.Log("connect");
    }

    void OnDisconnect(NetworkMessage netMsg){
        ExitProcedure();
    }

    public void ExitProcedure(){
        SceneManager.LoadScene("Main");
        Cursor.visible=true;
        Cursor.lockState=CursorLockMode.None;
        Destroy(gameObject);
    }

    void OnConfig(NetworkMessage netMsg){
        Debug.Log(nc.isConnected);
        configuration=netMsg.ReadMessage<Configuration>();
        SceneManager.LoadScene(configuration.mapName);
    }

	/*public Dictionary<string, List<int>> parseMaterie(string s){
        Dictionary<string, List<int>> materie=new Dictionary<string, List<int>>();
        string[] materiechunk=s.Split(new string[]{"<td width=\"350\" class=\"registro grautext open_sans_condensed_bold font_size_14\" style=\"vertical-align: middle;\" align=\"left\" >"},System.StringSplitOptions.None).Skip(1).ToArray();
        for(int i=0;i<materiechunk.Count();i++){
            string[] materia=materiechunk[i].Split(new string[]{"</td>"},2,System.StringSplitOptions.None);
            materie.Add(materia[0].Replace("&nbsp;","").Trim(),parseVoti(materia[1]));
        }
        return materie;
    }

    public List<int> parseVoti(string chunk){
        List<int> voti=new List<int>();
        string[] splittedChunk=chunk.Split(new string[]{"<p align=\"center\" class=\"s_reg_testo cella_trattino\"  style=\"border:0; margin:0; padding:0;font-weight:bold; \">"},System.StringSplitOptions.None).Skip(1).ToArray();
        //Debug.Log(sa.Count());
        for(int i=0;i<splittedChunk.Count();i++){
            string voto=Regex.Match(splittedChunk[i].Split(new string[]{"</p>"},System.StringSplitOptions.None)[0],@"\d+").Value;
            if(voto != ""){
                voti.Add(int.Parse(voto));
            }
        }
        return voti;
    }
    */

    public Dictionary<string, List<int>> parseMaterie(string s){
        Dictionary<string, List<int>> materie=new Dictionary<string, List<int>>();
        string[] materieChunks=s.Split(new string[]{"\"registro redtext open_sans_condensed_bold font_size_20\""},System.StringSplitOptions.None).Skip(1).ToArray();
        
        foreach(string c in materieChunks){
            materie.Add(c.Split('>')[1].Split('<')[0].Replace("\n",""),parseVoti(c));
        }

        return materie;
    }

    public List<int> parseVoti(string chunk){
        List<int> voti=new List<int>();
        string[] votiChunks=chunk.Split(new string[]{"\"s_reg_testo cella_trattino\""},System.StringSplitOptions.None).Skip(1).ToArray();
        foreach(string c in votiChunks){
            string voto=Regex.Match(c.Split('>')[1].Split('<')[0],@"\d+").Value;
             if(voto != ""){
                voti.Add(int.Parse(voto));
            }
        }
        return voti;
    }
	public IEnumerator lcv(string username,string password,string ip,System.Action failure,System.Action success)
    {
        if(username=="" || password==""){
            failure();
            yield break;
        }
        WWWForm form = new WWWForm();
        form.AddField("uid", username);
        form.AddField("pwd", password);
        UnityWebRequest request = UnityWebRequest.Post("https://web.spaggiari.eu/auth/app/default/AuthApi4.php?a=aLoginPwd", form);
        yield return request.SendWebRequest();
        LoginRequest lr = (LoginRequest) JsonUtility.FromJson(request.downloadHandler.text,typeof(LoginRequest));
        if (lr.data.auth.loggedIn) {
            success();
            Debug.Log(lr.data.auth.accountInfo.nome);
            userdata=lr.data.auth.accountInfo;
            if(userdata.type=="S" || userdata.type=="G"){
                string session;
                request.GetResponseHeaders().TryGetValue("Set-Cookie", out session);
                request = UnityWebRequest.Get("https://web.spaggiari.eu/cvv/app/default/genitori_note.php?classe_id=&studente_id=4234239&ordine=materia&filtro=tutto");
                request.SetRequestHeader("Cookie", session);
                yield return request.SendWebRequest();
                lr.data.auth.accountInfo.materie=parseMaterie(request.downloadHandler.text);
            }
            serverConnect(ip);
        }
        else
        {
            failure();
        }
    }
	
}





    [System.Serializable]
    public class LoginRequest
    {
        public LoginData data;
    }
    [System.Serializable]
    public class LoginData
    {
        public AuthData auth;
    }

    [System.Serializable]
    public class AuthData
    {
        public bool loggedIn;
        public UserData accountInfo;
    }

    [System.Serializable]
    public class UserData
    {
        public int id;
        public string nome;
        public string cognome;
        public string type="";
        public Dictionary<string,List<int>> materie;
    }


	[System.Serializable]
    public class Configuration : MessageBase
    {
        public int timeLimit;
		public float timeLeft;
		public float time2newmatch;
		public int respawnTime;
		public int kills;
		public string mapName;

    }

	[System.Serializable]
    public class ServerAuth : MessageBase
    {
        public string username;
        public int media;
    }

	[System.Serializable]
    public class Welcome : MessageBase
    {
        public int id;
        public User[] users;
		public Vector3 position;
    }

    [System.Serializable]    
	public class User : MessageBase{
		public string username;
		public int id;
        public int kills;
        public bool dead;

        public int media;

        public Vector3 position;

        public Quaternion rotation;

        public Vector3 direction;
	}

    [System.Serializable]
    public class NetArray : MessageBase
    {
		public int[] value;
    }

    [System.Serializable]
    public class NetUpdate : MessageBase
    {
        public int kills;
        public bool dead;
		public Quaternion rot;
        public Quaternion armRot;
		public Vector3 pos;
		public NetDir dir;
    }


	[System.Serializable]
    public class PlayerUpdate : MessageBase
    {
		public int id;

        public int kills;
        public bool dead;
		public Vector3 pos;
		public NetDir dir;
		public Quaternion rot;

        public Quaternion armRot;

    }
	

    [System.Serializable]
    public class NetQuaternion : MessageBase
    {
		public Quaternion value;
    }

    [System.Serializable]
    public class NetVect3 : MessageBase
    {
		public Vector3 value;
    }

    [System.Serializable]
    public class NetDir : MessageBase
    {
		public Vector3 value;
        public bool b;
    }


	[System.Serializable]
    public class NetInt : MessageBase
    {
        public int value;
    }

	[System.Serializable]
    public class PlayerDead : MessageBase
    {
		public int killer;
        public int victim;
    }

    [System.Serializable]
    public class PlayerQuaternion : MessageBase
    {
        public int id;
		public Quaternion quaternion;
    }

    [System.Serializable]
    public class PlayerVect3 : MessageBase
    {
        public int id;
		public Vector3 vect3;
    }

    [System.Serializable]
    public class PlayerDir : MessageBase
    {
        public int id;
		public Vector3 vect3;
        public bool b;
    }

    [System.Serializable]
	public class NetVect3PArray : MessageBase{
		public PlayerVect3[] array;
	}

	[System.Serializable]
    public class PlayerShoot : MessageBase
    {
        public int id;
		public int damage;
    }