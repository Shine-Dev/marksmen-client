using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using System.Linq;

public class GameManager : MonoBehaviour {

	// Use this for initialization
    public GameObject player;
    private RespawnTimer respawnTimer;

    private MaterieShow materieShow;

    private Transform quitmenu;

    public KeyMenu keyMenu;
    private CameraController cam;
    private GameObject UI;
	private Text hpTxt;
    private Text maxTxt;
    private Text youTxt;
	private Text dmTxt;
    private Text timeTxt;
    private Text recordTxt;

    private Image hpFill;

    public int kills;

    private int id;
	private UserData userData;
	private NetworkManager nm;

	public int baseHpMax;
	public int baseDamage;

	public string username;

    public float timeLeft;

    public int recordHolder;
    public int maxPoints=0;

    private Dictionary<int,GameObject> players;

    private List<OverlayControl> overlayItems;


	void Start () {
        Cursor.visible=!Cursor.visible;
        Cursor.lockState=CursorLockMode.Locked;
        QualitySettings.vSyncCount = 0;  // VSync must be disabled
        Application.targetFrameRate = 30;
        players=new Dictionary<int,GameObject>();
        hpTxt=GameObject.Find("hp").GetComponent<Text>();
		dmTxt=GameObject.Find("dm").GetComponent<Text>();
        youTxt=GameObject.Find("youTxt").GetComponent<Text>();
        maxTxt=GameObject.Find("maxTxt").GetComponent<Text>();
        timeTxt=GameObject.Find("timeTxt").GetComponent<Text>();
        recordTxt=GameObject.Find("recordTxt").GetComponent<Text>();
        hpFill=GameObject.Find("hpfill").GetComponent<Image>();
        keyMenu=GameObject.Find("menuchiavi").GetComponent<KeyMenu>();
        quitmenu=GameObject.Find("quitmenu").transform;
        UI=GameObject.Find("UI");
        respawnTimer=GameObject.Find("esito").GetComponent<RespawnTimer>();
        materieShow=GameObject.Find("materieshow").GetComponent<MaterieShow>();
        cam=GameObject.Find("Camera").GetComponent<CameraController>();
        overlayItems=new List<OverlayControl>((OverlayControl[])GameObject.FindObjectsOfType(typeof(OverlayControl)));
		nm=GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
        maxTxt.text=nm.getConfiguration().kills.ToString();

        if(nm.getConfiguration().time2newmatch>0){
            timeLeft=0;
            respawnTimer.Start(nm.getConfiguration().time2newmatch,"Partita terminata!");
        }
        else{
            timeLeft=nm.getConfiguration().timeLeft;
        }

		calculateProperties();

        nm.nc.RegisterHandler(100,MatchEnded);
        nm.nc.RegisterHandler(101,MatchStarted);
        nm.nc.RegisterHandler(778,OnWelcome);
        nm.nc.RegisterHandler(811,OnEnemyConnect);
        
        ServerAuth serverAuth=new ServerAuth();
        serverAuth.username=username;
        serverAuth.media=baseDamage;
        Debug.Log("Connected: "+ nm.nc.isConnected);
        nm.nc.Send(777,serverAuth);
        regenerateMaterie();
	}

    public void spawnPlayer(Vector3 pos){
        if(GameObject.Find("player")==null){
            player=GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/player"),pos,Quaternion.identity);
        }
    }

    public void regenerateMaterie(){
        if(baseDamage!=-1){
            userData.materie=userData.materie.OrderBy(x=>System.Guid.NewGuid()).ToDictionary(x => x.Key, x => x.Value);
            string materia1=userData.materie.ElementAt(0).Key;
            string materia2=userData.materie.ElementAt(1).Key;
            materieShow.Start(materia1,materia2,userData.materie.ElementAt(0).Value.Average(),userData.materie.ElementAt(1).Value.Average());
            keyMenu.initialize(materia1,materia2,userData.materie.Where(pair => pair.Key==materia1 || pair.Key==materia2).Select(pair => pair.Value).ToList().SelectMany(x=>x).ToList());
        }
        else{
            keyMenu.setEnabled(false);
        }
    }

    public void netUpdate(Vector3 pos,Vector3 dir,bool b,Quaternion rot,Quaternion armRot){
        //Debug.Log("Net Update");
        NetUpdate netUpdate=new NetUpdate();
        netUpdate.dir=new NetDir();
        netUpdate.dir.value=dir;
        netUpdate.dir.b=b;
        netUpdate.pos=pos;
        netUpdate.rot=rot;
        netUpdate.kills=kills;
        netUpdate.armRot=armRot;
        netUpdate.dead=(player.transform.localScale==Vector3.zero);
        nm.nc.Send(899,netUpdate);
    }
    public void netRot(Quaternion r){
        NetQuaternion rotUpdate=new NetQuaternion();
        rotUpdate.value=r;
        nm.nc.Send(900,rotUpdate);
    }

    public void netPos(Vector3 pos){
        NetVect3 posUpdate=new NetVect3();
        posUpdate.value=pos;
        nm.nc.Send(901,posUpdate);
    }

    public void netDir(Vector3 dir,bool boost){
        //NetVect3 dirUpdate=new NetVect3();
        NetDir nd=new NetDir();
        nd.value=dir;
        nd.b=boost;
        nm.nc.Send(902,nd);
    }

    public void netJump(){
        nm.nc.Send(908,new EmptyMessage());
    }

    public void netShoot(int damage){
        NetInt shootUpdate=new NetInt();
        shootUpdate.value=damage;
        nm.nc.Send(903,shootUpdate);
    }

    public void netArmRot(Quaternion r){
        NetQuaternion armRotUpdate=new NetQuaternion();
        armRotUpdate.value=r;
        nm.nc.Send(907,armRotUpdate);
    }

    public void Dead(int id){
        NetInt dead=new NetInt();
        dead.value=id;
        nm.nc.Send(904,dead);
        UI.transform.localScale=new Vector3(0,0,0);
        respawnTimer.Start(nm.getConfiguration().respawnTime,"Sei stato eliminato da "+ players[id].gameObject.transform.Find("username").GetComponent<TextMesh>().text);
        enemy e=players[id].GetComponent<enemy>();
        e.addKill();

        if(e.getKills()>maxPoints){
            setRecordHolder(id,e.getKills());
        }
    }

    public void playerDamaged(int id, int damage){
        PlayerShoot playerDamaged=new PlayerShoot();
        playerDamaged.id=id;
        playerDamaged.damage=damage;
        nm.nc.Send(905,playerDamaged);
    }

	//server reply methods

    void OnWelcome(NetworkMessage netMsg){
        Welcome welcome=netMsg.ReadMessage<Welcome>();
        spawnPlayer(welcome.position);
        id=welcome.id;
        
        foreach(User u in welcome.users){  
            GameObject enemy=GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/enemy"),u.position,u.rotation);
            enemy.transform.Find("username").GetComponent<TextMesh>().text=u.username;
            enemy.name="e" + u.id;
            enemy.GetComponent<enemy>().setId(u.id);
            enemy.GetComponent<enemy>().setMedia(u.media);
            enemy.GetComponent<enemy>().setKills(u.kills);
            players.Add(u.id,enemy);
            if(u.dead){
                enemy.transform.localScale=new Vector3(0,0,0);
            }
            if(u.kills>maxPoints){
                setRecordHolder(u.id,u.kills);
            }
        }
        
        nm.nc.RegisterHandler(791,OnReadyReply);
        nm.nc.RegisterHandler(810,OnEnemyDisconnect);
        nm.nc.RegisterHandler(898,OnEnemyUpdate);
        nm.nc.RegisterHandler(910,OnEnemyRotChange);
        nm.nc.RegisterHandler(911,OnEnemyPosCorrection);
        nm.nc.RegisterHandler(912,OnEnemyDirChange);
        nm.nc.RegisterHandler(917,OnEnemyArmRotate);
        nm.nc.RegisterHandler(913,OnEnemyShoot);
        nm.nc.RegisterHandler(914,OnEnemyDied);
        nm.nc.RegisterHandler(915,OnDamage);
        nm.nc.RegisterHandler(916,OnEnemyRespawn);
        nm.nc.RegisterHandler(918,OnEnemyJump);
        nm.nc.RegisterHandler(926,OnPlayerRespawn);
        

        NetArray na=new NetArray();
        na.value=players.Keys.ToArray();
        nm.nc.Send(790,na);

    }

    void MatchEnded(NetworkMessage netMsg){
        respawnTimer.Start(nm.getConfiguration().respawnTime,getMatchEndedString());
        timeLeft=0;
        recordHolder=0;
        maxPoints=0;
        kills=0;
        recordTxt.text=maxPoints.ToString();
        youTxt.text=kills.ToString();
        player.GetComponent<PController>().setImmune(true);
        foreach(GameObject u in players.Values){
            u.GetComponent<enemy>().setImmune(true);
            u.GetComponent<enemy>().setKills(0);
        }
	}

    void MatchStarted(NetworkMessage netMsg){
        respawnTimer.Stop();
        //keyMenu.restart();
        timeLeft=nm.getConfiguration().timeLimit;
        NetVect3PArray a=netMsg.ReadMessage<NetVect3PArray>();
        player.GetComponent<PController>().setImmune(false);
        for(int i=0;i<a.array.Length;i++){    
            if(a.array[i].id==id){          
                player.GetComponent<PController>().initialize();
                player.transform.position=a.array[i].vect3;
                regenerateMaterie();
            }
            else{
                try{
                    players[a.array[i].id].GetComponent<enemy>().setImmune(false);
                    players[a.array[i].id].GetComponent<enemy>().setCorrection(Vector3.zero);
                    players[a.array[i].id].transform.position=a.array[i].vect3;
                }
                catch{
                    Debug.Log("User disconnected in the meantime!");
                }
            }
        }
	}


    void OnReadyReply(NetworkMessage netMsg){
        NetArray oldPlayers=netMsg.ReadMessage<NetArray>();
        foreach(int c in oldPlayers.value){
            playerDisconnect(c);
        }
	}

    void OnEnemyDisconnect(NetworkMessage netMsg){
        NetInt userDisconnect=netMsg.ReadMessage<NetInt>();
        playerDisconnect(userDisconnect.value);
	}

    void OnEnemyJump(NetworkMessage netMsg){
        NetInt userJump=netMsg.ReadMessage<NetInt>();
        players[userJump.value].GetComponent<enemy>().jump();
	}

    void OnEnemyDied(NetworkMessage netMsg){
        PlayerDead playerDied=netMsg.ReadMessage<PlayerDead>();
        enemy victim=players[playerDied.victim].GetComponent<enemy>();
        if(playerDied.killer==id){
            addKill();
            if(kills>maxPoints){
                setRecordHolder(-1,kills);
            }
            
            if(baseDamage==-1 && victim.getMedia()!=-1){
                player.GetComponent<PController>().applyBoost(victim.getMedia());
            }
            else if(baseDamage!=-1 && victim.getMedia()==-1){
                player.GetComponent<PController>().applyBoost(-1);
            }
        }
        else{
             enemy en=players[playerDied.killer].GetComponent<enemy>();
             en.addKill();
             if(en.getKills()>maxPoints){
                 setRecordHolder(en.getId(),en.getKills());
             }
        }
        victim.die();
	}

    void OnEnemyRespawn(NetworkMessage netMsg){
        PlayerVect3 playerRespawn=netMsg.ReadMessage<PlayerVect3>();
        players[playerRespawn.id].GetComponent<enemy>().respawn(playerRespawn.vect3);

	}

    void OnPlayerRespawn(NetworkMessage netMsg){
        NetVect3 playerRespawn=netMsg.ReadMessage<NetVect3>();
        spawnPlayer(playerRespawn.value);
        UI.transform.localScale=Vector3.one;
	}

    void OnEnemyConnect(NetworkMessage netMsg){
        User u=netMsg.ReadMessage<User>();
        GameObject enemy=GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/enemy"),u.position,u.rotation);
        enemy ecomp=enemy.GetComponent<enemy>();
        enemy.transform.Find("username").GetComponent<TextMesh>().text=u.username;
        enemy.name="e" + u.id;
        ecomp.setId(u.id);
        ecomp.setMedia(u.media);
        ecomp.setKills(u.kills);
        players.Add(u.id,enemy);
	}


    void OnEnemyRotChange(NetworkMessage netMsg){
        PlayerQuaternion enemyRot=netMsg.ReadMessage<PlayerQuaternion>();
        //TODO: not necessary for now, check later if it's worth it.
		players[enemyRot.id].GetComponent<enemy>().setRotation(enemyRot.quaternion);
    }

    void OnEnemyArmRotate(NetworkMessage netMsg){
        PlayerQuaternion enemyRot=netMsg.ReadMessage<PlayerQuaternion>();
        //TODO: not necessary for now, check later if it's worth it.
		players[enemyRot.id].GetComponent<enemy>().setArmRotation(enemyRot.quaternion);
    }


    void OnEnemyUpdate(NetworkMessage netMsg){
        //Debug.Log("NET UPDATE RECEIVED");
        PlayerUpdate enemyUpdate=netMsg.ReadMessage<PlayerUpdate>();
        //TODO: not necessary for now, check later if it's worth it.
		enemy e=players[enemyUpdate.id].GetComponent<enemy>();
        e.setRotation(enemyUpdate.rot);
        e.setDirection(enemyUpdate.dir.value,enemyUpdate.dir.b);
        e.setCorrection(enemyUpdate.pos);
        //e.setArmRotation(enemyUpdate.armRot);
        if(enemyUpdate.dead && players[enemyUpdate.id].transform.localScale==Vector3.one){
            players[enemyUpdate.id].transform.localScale=Vector3.zero;
        }
        else if(!enemyUpdate.dead && players[enemyUpdate.id].transform.localScale==Vector3.zero){
            players[enemyUpdate.id].transform.localScale=new Vector3(0.15f,0.15f,0.23f);;
        }

        if(enemyUpdate.kills != e.getKills()){
            e.setKills(enemyUpdate.kills);
            if(e.getKills() > maxPoints){
                setRecordHolder(enemyUpdate.id,e.getKills());
            }
            else if(recordHolder==e.getId() && e.getKills()<maxPoints){
                search4NewHolder();
            }
        }
    }

    void OnEnemyShoot(NetworkMessage netMsg){
        PlayerShoot enemyShoot=netMsg.ReadMessage<PlayerShoot>();
        //TODO: not necessary for now, check later if it's worth it.
		players[enemyShoot.id].GetComponent<enemy>().shoot(enemyShoot.damage);
    }

    void OnEnemyDirChange(NetworkMessage netMsg){
        PlayerDir enemyDir=netMsg.ReadMessage<PlayerDir>();
        players[enemyDir.id].GetComponent<enemy>().setDirection(enemyDir.vect3,enemyDir.b);
        
	}

    void OnEnemyPosCorrection(NetworkMessage netMsg){
        PlayerVect3 enemyPos=netMsg.ReadMessage<PlayerVect3>();
        players[enemyPos.id].GetComponent<enemy>().setCorrection(enemyPos.vect3);
	}


    void OnDamage(NetworkMessage netMsg){
        PlayerShoot playerDamaged=netMsg.ReadMessage<PlayerShoot>();
        player.GetComponent<PController>().applyDamage(playerDamaged.damage,playerDamaged.id);
	}

    public void playerDisconnect(int id){
        GameObject u=players[id];
        players.Remove(id);
        Destroy(u);
        if(id==recordHolder){
            search4NewHolder();
        }
    }


	// Update is called once per frame
	void Update () {
        if(Input.GetKeyDown(KeyCode.Escape)){
            MenuStatus();
        }
        
        if(timeLeft>=0){
            timeLeft=Mathf.Max(0,timeLeft-Time.deltaTime);
            float minutes=Mathf.Floor(timeLeft/60);
            float seconds=Mathf.Floor(timeLeft-minutes*60);
            timeTxt.text=minutes + ":" + (seconds>=10 ? seconds.ToString() : "0"+seconds);
        }
	}

    void LateUpdate(){
         //Overlay Control
        List<OverlayControl> activeOverlayItems=overlayItems.FindAll(x=>x.isActive());
        if(activeOverlayItems.Count>0){
            int HighestRank=activeOverlayItems.Max(x=>x.rank);
            foreach(OverlayControl c in activeOverlayItems.FindAll(x=> x.rank<HighestRank && x.isActive())){
                c.Hide();
            }
        }
    }

    public string getMatchEndedString(){
        return maxPoints == 0 ? "Pareggio!" :  recordHolder==-1  ? "Hai vinto il match!" : players[recordHolder].transform.Find("username").GetComponent<TextMesh>().text + " ha vinto il match!";
    }

    public void search4NewHolder(){
            int max=0;
            int maxholder=0;
            foreach(GameObject c in players.Values){
                enemy e=c.GetComponent<enemy>();
                int ckills=e.getKills();
                if(ckills>max){
                    max=ckills;
                    maxholder=e.getId();
                }
            }
            if(kills>max){
                max=kills;
                maxholder=-1;
            }
            setRecordHolder(maxholder,max);
    }

    public void setRecordHolder(int id,int v){
        recordHolder=id;
        maxPoints=v;
        recordTxt.text=maxPoints.ToString();
    }

	public void setHpUI(int value){
		hpTxt.text=value.ToString() + "/" + baseHpMax;
        float perc=(float)value/baseHpMax;
        hpFill.fillAmount=perc;
        hpFill.color=(perc<0.4f) ? new Color(0.866666667f,0.007843137f,0,1) : (perc<0.6) ? new Color(0.847058824f,0.847058824f,0,1) : (perc <0.9) ? new Color(0.254901961f,0.847058824f,0,1) : new Color(0,0.560784314f,1,1);
	}

	public void setDamageUI(int value){
		dmTxt.text=value.ToString();
	}

	public void addKill(){
		kills++;
		youTxt.text=kills.ToString();
	}

    public void calculateProperties(){
        userData=nm.GetUserData();
        if(userData.type=="S" || userData.type=="G"){
            double sommaMedie=0;
		    foreach(KeyValuePair<string,List<int>> materia in userData.materie){
                sommaMedie+=materia.Value.Average();
		    }
            baseDamage=Mathf.RoundToInt((float)sommaMedie/userData.materie.Count);
		    baseHpMax=baseDamage*10;
        }
        else{
            baseDamage=-1;
            baseHpMax=200;
        }
        username=userData.nome.Replace(" ","_") + "_" + userData.cognome.Replace(" ","_");
    }

    public void QuitMatch(){
        nm.ExitProcedure();
    }

    public void QuitGame(){
        Application.Quit();
    }

    public void MenuStatus(){
        Cursor.visible=!Cursor.visible;
        Cursor.lockState=Cursor.lockState==CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
        quitmenu.localScale=!Cursor.visible ? Vector3.zero : Vector3.one;    
    }


}
