using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using System.IO;
using System.Collections;

public class RoomManager : MonoBehaviourPunCallbacks
{
    private void Awake() 
    {
        DontDestroyOnLoad(gameObject);
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
            PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Player", "PlayerManager"), Vector3.zero, Quaternion.identity);
        }
    }


}
