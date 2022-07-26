using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
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

    public ProfileData playerProfile { get; set; }
    private string playerUsername;

    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();
    [SerializeField] private float respawnTime;

    public List<PlayerInfo> playerInfo = new List<PlayerInfo>();
    public int myIndex;

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
    }

    private void Start() 
    {
        if(!pv.IsMine) return;


        ValidateConnection();        
        CreatePlayerController();
        CreatePlayerHUD();
        NewPlayerSend(Launcher.Instance.myProfile);
    }

    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public override void OnLeftRoom ()
    {
        base.OnLeftRoom();
        SceneManager.LoadScene("menu_scene");
    }

    [PunRPC]
    private void SyncProfile(string _username, int _level, int _xp)
    {
        playerProfile = new ProfileData(_username, _level, _xp);
        playerUsername = playerProfile.username;

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

    private void ValidateConnection()
    {
        if (PhotonNetwork.IsConnected) return;
        SceneManager.LoadScene("menu_scene");
    }

    public void NewPlayerSend(ProfileData profileData)
    {
        object[] package = new object[6];

        package[0] = profileData.username;
        package[1] = profileData.level;
        package[2] = profileData.xp;
        package[3] = PhotonNetwork.LocalPlayer.ActorNumber;
        package[4] = (short) 0;                                 // kills
        package[5] = (short) 0;                                 // deaths

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.NewPlayer,
            package,
            new RaiseEventOptions {Receivers = ReceiverGroup.MasterClient},
            new SendOptions { Reliability = true }
        );
    }

    public void NewPlayerReceive(object[] data)
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

        foreach (GameObject gameObject in GameObject.FindGameObjectsWithTag("PlayerManager")) 
        {
            pv.RPC(nameof(SyncProfile), RpcTarget.All, Launcher.Instance.myProfile.username, Launcher.Instance.myProfile.level, Launcher.Instance.myProfile.xp);
        }

        UpdatePlayersSend(playerInfo);
    }

    public void UpdatePlayersSend(List<PlayerInfo> info)
    {
        object[] package = new object[info.Count];

        for ( int i = 0; i < info.Count; i++)                    // get the info of all players
        {
            object[] piece = new object[6];

            piece[0] = info[i].profile.username;
            piece[1] = info[i].profile.level;
            piece[2] = info[i].profile.xp;
            piece[3] = info[i].playerActor;
            piece[4] = info[i].kills;
            piece[5] = info[i].deaths;

            package[i] = piece;
        }

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.UpdatePlayers,
            package,
            new RaiseEventOptions {Receivers = ReceiverGroup.All},
            new SendOptions { Reliability = true }
        );
    }

    public void UpdatePlayersReceive(object[] data)
    {
        playerInfo = new List<PlayerInfo>();

        for (int i = 0; i < data.Length; i++)
        {
            object[] extract = (object[]) data[i];

            PlayerInfo _playerInfo = new PlayerInfo(
                new ProfileData(
                    (string) extract[0],
                    (int) extract[1],
                    (int) extract[2]
                ), 
                (int) extract[3],
                (short) extract[4],
                (short) extract[5]
            );

            playerInfo.Add(_playerInfo);
            Debug.Log($"{(string)extract[0]} - \nKills: {(short)extract[4]} \nDeaths: {(short)extract[5]}");

            if (PhotonNetwork.LocalPlayer.ActorNumber == _playerInfo.playerActor) 
            {
                myIndex = i;
                Debug.Log($"Actor Number: {PhotonNetwork.LocalPlayer.ActorNumber} - Player Actor: {_playerInfo.playerActor}");
            }
        }
    }

    public void ChangeStatSend(int actor, byte stat, byte amount)
    {
        object[] package = new object[] { actor, stat, amount };

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.ChangeStat,
            package, 
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
        );
    }

    public void ChangeStatReceive(object[] data)
    {
        int actor = (int) data[0];
        byte stat = (byte) data[1];
        byte amount = (byte) data[2];

        for (int i = 0; i < playerInfo.Count; i++)
        {
            if (playerInfo[i].playerActor == actor)
            {
                switch (stat)
                {
                    case 0:
                        playerInfo[i].kills += amount;
                        Debug.Log($"Player {playerInfo[i].profile.username}: Kills - {playerInfo[i].kills}");
                        break;
                    case 1:
                        playerInfo[i].deaths += amount;
                        Debug.Log($"Player {playerInfo[i].profile.username}: Deaths - {playerInfo[i].deaths}");
                        break;
                }
            }
        }
    }

    private void GetSpawnPoints() 
    {
        var _spawnPoints = GameObject.Find("-- MAP --/SpawnManager");
        
        foreach (Transform _spawnPoint in _spawnPoints.transform) {
            if(!_spawnPoint.GetComponent<SpawnPoint>().isActive) {
                spawnPoints.Remove(_spawnPoint);
            } 
            else {
                spawnPoints.Add(_spawnPoint);
            }
        }
    }

    public void CreatePlayerHUD() 
    {
        playerHUD = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "UI", "PlayerHUD"), Vector3.zero, Quaternion.identity);
        playerHUD.transform.SetParent(this.transform, false);
        playerHUD.GetComponent<UIWeapon>().weaponManager = playerController.GetComponentInChildren<WeaponManager>();
        playerHUD.GetComponent<UIPlayerHealth>().playerHealthScript = playerController.GetComponent<PlayerHealth>();
    }

    public void CreatePlayerController() 
    {
        
        if (playerHUD != null) {
            PhotonNetwork.Destroy(playerHUD);
        }

        GetSpawnPoints();
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count-1)];

        playerController = PhotonNetwork.Instantiate
        (
            Path.Combine("PhotonPrefabs", "Player", "PlayerController"), 
            
            spawnPoint.position, spawnPoint.rotation, 
            
            0, new object[] { pv.ViewID }
        );
    }

    public void DestroyPlayer() 
    { 
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
        playerHUD.GetComponent<UIPlayerHealth>().HUD_Respawn(respawnTime);
        
        var _spectatorCam = playerController.transform.Find("Cameras/SpectatorCamera");
        SpectatorCamera(_spectatorCam.gameObject);

        PhotonNetwork.Destroy(playerController);

        yield return new WaitForSeconds(respawnTime);

        PhotonNetwork.Destroy(_spectatorCam.gameObject);

            
        CreatePlayerController();
        CreatePlayerHUD();
    }
}
