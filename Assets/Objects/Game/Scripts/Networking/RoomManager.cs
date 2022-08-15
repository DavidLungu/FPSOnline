using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using System.Collections;
using Photon.Realtime;

public class RoomManager : MonoBehaviourPunCallbacks
{
    private GameObject playerManager;
    public string mainMenu = "menu_scene";

    public string selectedMapName;
    public string selectedGamemode;

    private int currentPlayerCount;
    private string currentScene;
    public static RoomManager Instance;
    
    private void Awake() 
    {
        if (Instance == null) {
            Instance = this;
        }
        else {
            if (Instance != this)
            {
                Destroy(Instance);
                Instance = this;
            }
        }

        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F2)) Disconnect();

        if (GameManager.Instance != null) GameManager.Instance.playerCount = currentPlayerCount;
    }

    public override void OnEnable() 
    {
        base.OnEnable();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public override void OnDisable() 
    {
        base.OnDisable();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode) 
    {
        if(scene.name == Launcher.Instance.selectedMap) {
            PhotonNetwork.AutomaticallySyncScene = true;
            currentScene = scene.name;
            GameManager.Instance.playerCount = currentPlayerCount;
        }
    }

    public override void OnJoinedRoom()
    {
        currentPlayerCount = PhotonNetwork.CurrentRoom.PlayerCount;
    }

    public override void OnLeftRoom ()
    {
        base.OnLeftRoom();
        SceneManager.LoadScene(mainMenu);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
            Debug.LogFormat($"{newPlayer} has joined the game.");
            currentPlayerCount++;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {       
        Debug.LogFormat($"{otherPlayer} has left the game.");
        currentPlayerCount--;
    }


    private void Disconnect()
    {
        Destroy(GameManager.Instance.playerManager);
        StartCoroutine(DisconnectAndLoad());
    }

    IEnumerator DisconnectAndLoad()
    {
        PhotonNetwork.Disconnect();
        
        while (PhotonNetwork.IsConnected)
            yield return null;

        Destroy(GameManager.Instance);

        SceneManager.LoadScene("menu_scene");
        Destroy(Instance);

    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        Destroy(gameObject);
    }

}
