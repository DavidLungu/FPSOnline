using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using System.Collections;

public class RoomManager : MonoBehaviourPunCallbacks
{
    private GameObject playerManager;
    
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
            photonView.RPC(nameof(AddPlayer), RpcTarget.AllBufferedViaServer);
        }
    }

    [PunRPC]
    private void AddPlayer()
    {
        GameManager.Instance.playerCount++;
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

}
