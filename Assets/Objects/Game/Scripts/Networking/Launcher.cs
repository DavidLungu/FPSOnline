using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;

public class ProfileData 
{
    public string username;
    public int level;
    public int xp;

    public ProfileData()
    {
        this.username = "";
        this.level = 0;
        this.xp = 0;
    }

    public ProfileData(string _username, int _level, int _xp)
    {
        this.username = _username;
        this.level = _level;
        this.xp = _xp;
    }
}

public class Launcher : MonoBehaviourPunCallbacks
{
    [SerializeField] private string GAME_VERSION;
    

    [Header("Profile")]
    public ProfileData myProfile = new ProfileData();
    public TMP_InputField usernameField;

    [Header("Player List")]
    [SerializeField] private Transform playerListContent;
    [SerializeField] private GameObject playerListPrefab;


    [Header("Rooms")]
    [SerializeField] private TextMeshProUGUI roomName;
    [SerializeField] private TMP_InputField roomNameInputField;
    [SerializeField] private Transform roomListContent;
    [SerializeField] private GameObject roomListPrefab;
    public RoomOptions roomOptions = new RoomOptions();
    private List<RoomInfo> currentRoomList = new List<RoomInfo>();


    [Header("Map")]
    [SerializeField] MapManager mapManager;
    public string selectedMap { get; set; }

    [SerializeField] private TextMeshProUGUI errorText;
    [SerializeField] private GameObject startMatchButton;

    public static Launcher Instance;
 
    private void Awake() 
    {
        if (Instance != null) {
            Destroy(this);
            return;
        }

        Instance = this;
        
        PhotonNetwork.GameVersion = GAME_VERSION;
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.AutomaticallySyncScene = true;           
        SetCursor(); 
    }

    public override void OnConnectedToMaster() 
    {        
        Debug.Log("Connected to Master");
        JoinLobby();
    }

    public void JoinLobby() 
    {
        MenuManager.Instance.OpenMenu("LoadingMenu");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby() 
    {
        myProfile.username = PlayerPrefs.GetString("username");

        if(string.IsNullOrEmpty(myProfile.username))
        {
            MenuManager.Instance.OpenMenu("CreateProfile");
            return;
        } 

        PhotonNetwork.NickName = myProfile.username;

        MenuManager.Instance.OpenMenu("MainMenu");
        Debug.Log("Joined Lobby");
        
    }

    public void CreateUser() 
    {
        if (string.IsNullOrEmpty(usernameField.text) || !usernameField.text.All(char.IsLetterOrDigit)) {
            usernameField.text = "Player" + Random.Range(0, 9999).ToString("0000");
        }
    
        PlayerPrefs.SetString("username", usernameField.text);

        myProfile.username = usernameField.text;
        PhotonNetwork.NickName = myProfile.username;

        Debug.Log(PhotonNetwork.NickName);
        MenuManager.Instance.OpenMenu("MainMenu");
    }

    public void CreateRoom() 
    {
        roomOptions.MaxPlayers = 8;

        PhotonNetwork.CreateRoom(
            !string.IsNullOrEmpty(roomNameInputField.text) 
            ? roomNameInputField.text : $"{PhotonNetwork.NickName}'s Room",
            roomOptions
        );
        
        MenuManager.Instance.OpenMenu("LoadingMenu");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        OnErrorEncountered(returnCode, message);
    }

    public void JoinRoom(RoomInfo info) 
    {
        PhotonNetwork.JoinRoom(info.Name);
        MenuManager.Instance.OpenMenu("Loading");
    }

    public override void OnJoinedRoom() 
    {
        roomName.text = PhotonNetwork.CurrentRoom.Name;

        mapManager.SelectMap("map_bastion_s");
        MenuManager.Instance.OpenMenu("RoomMenu");

        Player[] players = PhotonNetwork.PlayerList;

        foreach (Transform child in playerListContent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < players.Length; i++) 
        {
            Instantiate(playerListPrefab, playerListContent).GetComponent<PlayerListItem>().AddPlayer(players[i]);
        }

        startMatchButton.SetActive(PhotonNetwork.IsMasterClient);
        mapManager.mapSelectionButton.enabled = PhotonNetwork.IsMasterClient;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Instantiate(playerListPrefab, playerListContent).GetComponent<PlayerListItem>().AddPlayer(newPlayer);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        UpdateRoomList(roomList);
    }

    private void UpdateRoomList(List<RoomInfo> roomList)
    {
        foreach (Transform _roomListContent in roomListContent)
        {
            Destroy(_roomListContent.gameObject);
        }

        for (int i = 0; i < roomList.Count; i++)
        {
            if(roomList[i].RemovedFromList) continue;
            Instantiate(roomListPrefab, roomListContent).GetComponent<RoomListItem>().CreateRoom(roomList[i]);    
        }

        currentRoomList = roomList;
    }

    public void LeaveRoom() 
    {
        PhotonNetwork.LeaveRoom();
        MenuManager.Instance.OpenMenu("LoadingMenu");
    }

    public override void OnLeftRoom() 
    {
        UpdateRoomList(currentRoomList);
    }

    public override void OnJoinRoomFailed(short returnCode, string message) 
    {
        errorText.text = string.Format("ERROR {0}: {1}", returnCode, message);
        MenuManager.Instance.OpenMenu("Error");
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        startMatchButton.SetActive(PhotonNetwork.IsMasterClient);
        mapManager.mapSelectionButton.enabled = PhotonNetwork.IsMasterClient;
    }
    
    private void OnErrorEncountered(short returnCode, string message)
    {
        MenuManager.Instance.OpenMenu("ErrorMenu");
        errorText.text = string.Format($"{returnCode}: {message}");
    }

    [PunRPC]
    public void StartGame() 
    {
        MenuManager.Instance.OpenMenu("LoadingMatchMenu");
        PhotonNetwork.LoadLevel(selectedMap);
    }

    public void ExitApplication() 
    {
        Application.Quit();
    }

    private void SetCursor() 
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

}
