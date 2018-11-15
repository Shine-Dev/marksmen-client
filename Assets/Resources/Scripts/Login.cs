using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Text.RegularExpressions;

public class Login : MonoBehaviour {

    GameObject username;
    GameObject password;
    GameObject loginerror;

    //private Dictionary<string,List<int>> materie;
    private UserData userdata;
	// Use this for initialization
	void Start () {
        DontDestroyOnLoad(transform.gameObject);
        username = GameObject.Find("Username");
        password = GameObject.Find("Password");
        loginerror = GameObject.Find("loginerror");
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void LoginClasseViva()
    {
        StartCoroutine(lcv());
    }

    public IEnumerator lcv()
    {
        WWWForm form = new WWWForm();
        form.AddField("uid", username.GetComponent<InputField>().text);
        form.AddField("pwd", password.GetComponent<InputField>().text);
        UnityWebRequest request = UnityWebRequest.Post("https://web.spaggiari.eu/auth/app/default/AuthApi4.php?a=aLoginPwd", form);
        yield return request.SendWebRequest();
        LoginRequest lr = (LoginRequest) JsonUtility.FromJson(request.downloadHandler.text,typeof(LoginRequest));
        if (lr.data.auth.loggedIn) {
            userdata=lr.data.auth.accountInfo;
            string session;
            request.GetResponseHeaders().TryGetValue("Set-Cookie", out session);
            request = UnityWebRequest.Get("https://web.spaggiari.eu/cvv/app/default/genitori_voti.php");
            request.SetRequestHeader("Cookie", session);
            yield return request.SendWebRequest();
            lr.data.auth.accountInfo.materie=parseMaterie(request.downloadHandler.text);
        }
        else
        {
            loginerror.transform.localScale=new Vector3(1,1,1);
        }
    }

    /*public Dictionary<string,List<int>> getMaterie(){
        return materie;
    }*/

    public UserData GetUserData(){
        return userdata;
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

        public Dictionary<string,List<int>> materie;
    }

    public Dictionary<string, List<int>> parseMaterie(string s){
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
}

