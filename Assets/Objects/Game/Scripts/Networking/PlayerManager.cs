using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class PlayerInfo 
{
    public ProfileData profile;
    public int playerActor;
    public short kills;
    public short deaths;

    public PlayerInfo(ProfileData _profile, int _playerActor, short _kills, short _deaths)
    {
        this.profile = _profile;
        this.playerActor = _playerActor;
        this.kills = _kills;
        this.deaths = _deaths;
    }
}

public class PlayerManager : MonoBehaviourPunCallbacks, IOnEventCallback 
{
    public GameObject playerController { get; private set; }

    public GameObject playerHUD { get; private set; }
    private UIWeapon uiWeapon;
    private UIPlayerHealth uiHealth;

    private UILeaderboard leaderboard;
    private TMP_Text uiMatchWinner;
    public GameObject uiEndMatch { get; private set; }
    public GameState currentState;
    private GameMode currentMode;
    private Weapon[] startingLoadout;

    public ProfileData playerProfile { get; set; }
    private string playerUsername;

    private List<SpawnPoint> spawnPoints = new List<SpawnPoint>();
    [SerializeField] private float respawnTime;
    private string otherPlayerName;

    public List<PlayerInfo> playerInfo = new List<PlayerInfo>();
    public int myIndex;

    private string mainMenu;

    private PhotonView pv;

    public enum EventCodes : byte
    {
        NewPlayer,
        UpdatePlayers,
        ChangeStat
    }

    private void Awake() 
    {
        pv = GetComponent<PhotonView>();
        mainMenu = RoomManager.Instance.mainMenu;
    }

    private void Start() 
    {
        if(!pv.IsMine) return;

        ValidateConnection();   
        pv.RPC("SyncProfile", RpcTarget.All, Launcher.Instance.myProfile.username, Launcher.Instance.myProfile.level, Launcher.Instance.myProfile.xp);

        currentState = GameState.Playing;
        currentMode = GameManager.Instance.gamemode;
        startingLoadout = GameManager.Instance.startingLoadout;

        NewPlayerSend(Launcher.Instance.myProfile);
        
        SpawnPlayer();
    }

    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        //ExitPlayerSend(otherPlayer.ActorNumber);
    }

    
    private void Update()
    {
        if(!pv.IsMine) return;
        
        if (currentState == GameState.Ending) 
        { 
            StopAllCoroutines();
            return; 
        }
    if (leaderboard != null && leaderboard.leaderboardObject != null)
        if (Input.GetKey(KeyCode.Tab)) {
             CreateLeaderboard();
        } 
        else {
            if (leaderboard.leaderboardObject.activeSelf) 
                leaderboard.leaderboardObject.SetActive(false);
        } 
    }

    private void TrySync()
    {
        if (!pv.IsMine) return;

        pv.RPC(nameof(SyncProfile), RpcTarget.All, Launcher.Instance.myProfile.username, Launcher.Instance.myProfile.level, Launcher.Instance.myProfile.xp);

    }

    [PunRPC]
    private void SyncProfile(string _username, int _level, int _xp)
    {
        if (!pv.IsMine) { return; }
        
        playerProfile = new ProfileData(_username, _level, _xp);
        playerUsername = playerProfile.username;
    }

    private void ValidateConnection()
    {
        if (PhotonNetwork.IsConnected) return;
        
        SceneManager.LoadScene(mainMenu);
    }

    private void StateCheck()
    {
        if (currentState == GameState.Ending)
        {
            EndMatch();
        }
    }

    private void SpawnPlayer()
    {
        CreatePlayerController();

        WeaponManager weaponManager = playerController.GetComponentInChildren<WeaponManager>();

        foreach (Weapon _weapon in startingLoadout) 
        {
            weaponManager.photonView.RPC(nameof(weaponManager.RPC_PickUp), RpcTarget.All, _weapon.GetWeaponData().weaponName);
        }
        weaponManager.photonView.RPC(nameof(weaponManager.Equip), RpcTarget.All, 0);
    
        CreatePlayerHUD();
    }

    private void ScoreCheck()
    {
        bool matchWon = false;
        string winner = "Player#0000";
        int objectiveCount = 0;


        foreach (PlayerInfo _playerInfo in playerInfo)
        {
            winner = _playerInfo.profile.username;

            if (currentMode == GameMode.FFA)
            {
                objectiveCount = _playerInfo.kills;

                if (winner == PhotonNetwork.LocalPlayer.NickName) {
                    uiMatchWinner.text = string.Format($"<color=yellow><b>You</b></color> Won!");
                    GameManager.Instance.isWinner = true;
                }
                else {
                    uiMatchWinner.text = string.Format($"<color=red><b>You</b></color> Lose");
                    GameManager.Instance.isWinner = false;
                }
            }

            matchWon = GameManager.Instance.CheckWin(objectiveCount);
            
            if (matchWon) break;
        }

        if (matchWon)
        {
            if (PhotonNetwork.IsMasterClient && currentState != GameState.Ending)
            {
                UpdatePlayersSend((int)GameState.Ending, playerInfo);
            }
        }
    }

    private void EndMatch()
    {
        currentState = GameState.Ending;
        
        uiWeapon.DisableHUD();
        uiHealth.DisableHUD();

        CreateLeaderboard();
        GameManager.Instance.EndMatch();
    }

    public void CreatePlayerHUD() 
    {
        if (currentState == GameState.Ending) return;

        playerHUD = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "UI", "PlayerHUD"), Vector3.zero, Quaternion.identity);
        playerHUD.transform.SetParent(this.transform, false);

        leaderboard = playerHUD.GetComponent<UILeaderboard>();

        InitializeUI();

        uiWeapon.weaponManager = playerController.GetComponentInChildren<WeaponManager>();
        uiHealth.playerHealthScript = playerController.GetComponent<PlayerHealth>();
        uiEndMatch = playerHUD.transform.Find("EndMatch").gameObject;
        uiMatchWinner = uiEndMatch.transform.Find("Winner").GetComponent<TMP_Text>();

        uiEndMatch.SetActive(false);
    }

    private void InitializeUI()
    {
        uiWeapon = playerHUD.GetComponent<UIWeapon>();
        uiHealth = playerHUD.GetComponent<UIPlayerHealth>();
    }

    public void CreateLeaderboard()
    {
        string _gamemodeName = GameManager.Instance.gamemodeName;
        string _mapName = GameManager.Instance.mapName;
        if(leaderboard.gameObject.activeSelf) leaderboard.Leaderboard(_mapName, _gamemodeName, playerInfo);
    }

    public void CreatePlayerController() 
    {
        if (currentState == GameState.Ending) return;

        if (playerHUD != null) {
            PhotonNetwork.Destroy(playerHUD);
        }

        spawnPoints = GameManager.Instance.GetSpawnPoints();
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count-1)].transform;

        playerController = PhotonNetwork.Instantiate
        (
            Path.Combine("PhotonPrefabs", "Player", "PlayerController"), 
            
            spawnPoint.position, spawnPoint.rotation, 
            
            0, new object[] { pv.ViewID }
        );
    }

    public void DisplayKill(int otherActor)
    {   
        Debug.Log($"({nameof(DisplayKill)}) Other player actor: " + otherActor);
        playerHUD.GetComponent<UIPlayerHealth>().DisplayKill(PhotonNetwork.CurrentRoom.GetPlayer(otherActor).NickName);
        playerHUD.GetComponent<UIWeapon>().DisplayHitmarker(2);
    }

    public void DestroyPlayer() 
    {         
        if (currentState == GameState.Ending) return;

        StartCoroutine(RespawnCountdown());
    }

    public void SpectatorCamera(GameObject spectatorCam) // TEMPORARY UNTIL DISCOVER SOLUTION 
    {
        spectatorCam.transform.SetParent(null);
        spectatorCam.GetComponent<Camera>().enabled = true;
        spectatorCam.GetComponent<AudioListener>().enabled = true;
    }

    private IEnumerator RespawnCountdown() 
    {
        playerHUD.GetComponent<UIWeapon>().weaponManager = null;
        playerHUD.GetComponent<UIPlayerHealth>().DisplayRespawn(respawnTime);
        
        if (string.IsNullOrEmpty(otherPlayerName)) otherPlayerName = PhotonNetwork.LocalPlayer.NickName;

        playerHUD.GetComponent<UIPlayerHealth>().otherPlayerName.text = string.Format($"Killed By: <color=#ff270b>[{otherPlayerName}]</color>");
        
        var _spectatorCam = playerController.transform.Find("Cameras/SpectatorCamera");
        SpectatorCamera(_spectatorCam.gameObject);

        PhotonNetwork.Destroy(playerController);

        yield return new WaitForSeconds(respawnTime);

        PhotonNetwork.Destroy(_spectatorCam.gameObject);

        SpawnPlayer();
    }

    public void OnEvent (EventData photonEvent)
    {
        if (photonEvent.Code >= 200) return;                            // photon reserves 200 - 255 for own purposes

        EventCodes eventCodes = (EventCodes) photonEvent.Code;
        object[] objArr = (object[]) photonEvent.CustomData;

        switch (eventCodes)
        {
            case EventCodes.NewPlayer:
                NewPlayerReceive(objArr);
                break;
            case EventCodes.UpdatePlayers:
                UpdatePlayersReceive(objArr);
                break;
            case EventCodes.ChangeStat:
                ChangeStatReceive(objArr);
                break;
        }
    }

    private void SendPackage(byte eventCode, object[] package, ReceiverGroup receiverGroup, bool isReliable)
    {
        RaiseEventOptions _raiseEventOptions = new RaiseEventOptions();
        _raiseEventOptions.Receivers = receiverGroup;

        SendOptions _sendOptions = new SendOptions();
        _sendOptions.Reliability = isReliable;

        PhotonNetwork.RaiseEvent (
            eventCode,
            package,
            _raiseEventOptions,
            _sendOptions
        );
    }

    public void NewPlayerSend (ProfileData profileData)
    {
        object[] package = new object[7];

        package[0] = profileData.username;
        package[1] = profileData.level;
        package[2] = profileData.xp;
        package[3] = PhotonNetwork.LocalPlayer.ActorNumber;
        package[4] = (short) 0;
        package[5] = (short) 0;

        SendPackage((byte)EventCodes.NewPlayer, package, ReceiverGroup.MasterClient, true);
    }
    
    public void NewPlayerReceive (object[] data)
    {
        PlayerInfo _playerInfo = new PlayerInfo(
            new ProfileData(
                (string) data[0],
                (int) data[1],
                (int) data[2]
            ),
            (int) data[3],
            (short) data[4],
            (short) data[5]
        );

        playerInfo.Add(_playerInfo);

        //resync our local player information with the new player
        foreach (GameObject gameObject in GameObject.FindGameObjectsWithTag("PlayerManager")) 
        {
            gameObject.GetComponent<PlayerManager>().TrySync();
        }

        UpdatePlayersSend((int)currentState, playerInfo);
    }

    public void UpdatePlayersSend (int state, List<PlayerInfo> info)
    {
        object[] package = new object[info.Count + 1];

        package[0] = state;
        for (int i = 0; i < info.Count; i++)
        {
            object[] piece = new object[7];

            piece[0] = info[i].profile.username;
            piece[1] = info[i].profile.level;
            piece[2] = info[i].profile.xp;
            piece[3] = info[i].playerActor;
            piece[4] = info[i].kills;
            piece[5] = info[i].deaths;

            package[i + 1] = piece;
        }

        SendPackage((byte)EventCodes.UpdatePlayers, package, ReceiverGroup.All, true);
    }

    public void UpdatePlayersReceive (object[] data)
    {
        currentState = (GameState)data[0];

        //check if there is a new player
        if (playerInfo.Count < data.Length - 1)
        {
            foreach (GameObject gameObject in GameObject.FindGameObjectsWithTag("PlayerManager"))
            {
                //if so, resync our local player information
                gameObject.GetComponent<PlayerManager>().TrySync();
            }
        }

        playerInfo = new List<PlayerInfo>();

        for (int i = 1; i < data.Length; i++)
        {
            object[] extract = (object[]) data[i];

            PlayerInfo _playerInfo = new PlayerInfo (
                new ProfileData (
                    (string) extract[0],
                    (int) extract[1],
                    (int) extract[2]
                ),
                (int) extract[3],
                (short) extract[4],
                (short) extract[5]
            );

            playerInfo.Add(_playerInfo);

            if (PhotonNetwork.LocalPlayer.ActorNumber == _playerInfo.playerActor)
            {
                myIndex = i - 1;
            }
        }

        StateCheck();
    }

    public void ChangeStatSend(int actor, int otherActor, byte stat, byte amount)
    {
        object[] package = new object[] { actor, otherActor, stat, amount };

        SendPackage((byte)EventCodes.ChangeStat, package, ReceiverGroup.All, true);
    }

    public void ChangeStatReceive(object[] data)
    {
        int actor = (int) data[0];
        int otherActor = (int) data[1];
        byte stat = (byte) data[2];
        byte amount = (byte) data[3];

        for (int i = 0; i < playerInfo.Count; i++)
        {
            if (playerInfo[i].playerActor == actor && currentState != GameState.Ending)
            {
                switch (stat)
                {
                    case 0:
                        playerInfo[i].kills += amount;
                        
                        DisplayKill(otherActor);
                        Debug.Log($"Player {playerInfo[i].profile.username}: Kills - {playerInfo[i].kills}");
                        break;
                    case 1:
                        playerInfo[i].deaths += amount;
                        
                        if(otherActor >= 0)
                            otherPlayerName = PhotonNetwork.CurrentRoom.GetPlayer(otherActor).NickName;
                        
                        Debug.Log($"Player {playerInfo[i].profile.username}: Deaths - {playerInfo[i].deaths}");
                        break;
                }
                
                if (i == myIndex) { if (leaderboard.leaderboardObject.activeSelf) CreateLeaderboard(); }
                if (leaderboard.leaderboardObject.activeSelf) CreateLeaderboard();

                break;
            }
        }

        ScoreCheck();
    }
}
