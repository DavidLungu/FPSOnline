using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using TMPro;

public enum GameMode
{
    FFA,
    TDM
}

public enum GameType
{
    Elimination
}


public enum GameState
{
    Waiting,
    Starting,
    Playing,
    Ending
}

public class GameManager : MonoBehaviourPunCallbacks
{
    [Header("Match Details")]
    public int objectiveCount;
    public int matchLength;
    [HideInInspector] public int remainingMatchTime;
    public bool isWinner;
    [SerializeField] private AudioClip winnerSound, loserSound;
    public int playerCount { get; set; }

    
    [Space]
    public string mapName;
    public string gamemodeName;
    public GameMode gameMode = GameMode.FFA;
    public GameType gameType = GameType.Elimination;
    public GameState state = GameState.Waiting;
    public Weapon[] startingLoadout;

    [SerializeField] private GameObject uiWaitForPlayers;

    [Header("Countdown Timer")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private GameObject timerTextTransform;
    [SerializeField] private float countdownTime = 4.0f;
    private float previousCountdownTime;
    private float countdownTimeRemaining;
    
    
    [Header("Misc")]
    [SerializeField] private GameObject mapSpectatorCamera;
    [SerializeField] private AudioSource audioSource;
    public GameObject playerManager { get; private set; }

    private List<SpawnPoint> playerSpawns = new List<SpawnPoint>();

    private string mainMenu;

    public static GameManager Instance;
    
    private void Awake() 
    {
        if (!PhotonNetwork.IsConnected) SceneManager.LoadScene(0);

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
    }

    private void Start()
    {
        mainMenu = RoomManager.Instance.mainMenu;
        mapName = RoomManager.Instance.selectedMapName;
        gamemodeName = RoomManager.Instance.selectedGamemode;
        countdownTimeRemaining = countdownTime;
    }

    private void Update()
    {
        if (playerCount == PhotonNetwork.CurrentRoom.PlayerCount && state == GameState.Waiting) {
            state = GameState.Starting;
        }

        if (state == GameState.Starting)
        {
            timerTextTransform.SetActive(true);
            
            StartCountdown();
        }
    }

    private void StartCountdown()
    {
        if (countdownTimeRemaining <= 1)
        {
            //audioSource.clip = countdownCompleteSound;
            
            if(countdownTimeRemaining <= 0) 
            {            
                countdownTimeRemaining = 0;
                if (PhotonNetwork.IsMasterClient) photonView.RPC(nameof(StartMatch), RpcTarget.AllBufferedViaServer);
            }
        }

        if (previousCountdownTime != (int)countdownTimeRemaining)
        {
            //audioSource.Play();
        }
        previousCountdownTime = (int)countdownTimeRemaining;

        countdownTimeRemaining -= Time.deltaTime;
        timerText.text = (countdownTimeRemaining).ToString("00");
    }

    public List<SpawnPoint> GetSpawnPoints() 
    {
        var _spawnPoints = FindObjectsOfType<SpawnPoint>();
        List<SpawnPoint> availableSpawns = new List<SpawnPoint>();
        
        foreach (SpawnPoint _spawnPoint in _spawnPoints) 
        {    
            if(!_spawnPoint.isActive) {
                availableSpawns.Remove(_spawnPoint);
            } 

            availableSpawns.Add(_spawnPoint);
        }
        
        playerSpawns = availableSpawns;
        return availableSpawns;
    }

    [PunRPC]
    private void StartMatch()
    {
        if (state == GameState.Playing) return;
        
        uiWaitForPlayers.SetActive(false);
        
        if(PhotonNetwork.IsConnected)
            playerManager = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Player", "PlayerManager"), Vector3.zero, Quaternion.identity);
        else
            playerManager = Instantiate(Resources.Load<GameObject>(Path.Combine("PhotonPrefabs", "Player", "PlayerManager")), Vector3.zero, Quaternion.identity);

        mapSpectatorCamera.SetActive(false);
        state = GameState.Playing;
    }

    private IEnumerator MatchTimer(float matchTime) // !
    {
        yield return new WaitForSeconds(1f);

    }

    public void EndMatch()
    {
        state = GameState.Ending;
        
        if (PhotonNetwork.IsMasterClient) 
        {
            PhotonNetwork.CurrentRoom.IsVisible = false;
            PhotonNetwork.CurrentRoom.IsOpen = false;
        }

        if (isWinner) audioSource.clip = winnerSound;
        else audioSource.clip = loserSound;

        playerManager.GetComponent<PlayerManager>().uiEndMatch.SetActive(true);
        audioSource.PlayOneShot(audioSource.clip);

        StartCoroutine(End(3f));
    }

    private IEnumerator End(float delay)
    {
        yield return new WaitForSeconds(5.6f);
        
        playerManager.GetComponent<PlayerManager>().CreateLeaderboard(); 

        yield return new WaitForSeconds(2.4f);

        PhotonNetwork.Destroy(playerManager.GetComponent<PlayerManager>().playerController);


        yield return new WaitForSeconds(delay);

        if(PhotonNetwork.IsMasterClient) { PhotonNetwork.DestroyAll(); }

        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.LeaveRoom();
    }

    public bool CheckWin(int objectiveCount)
    {
        if (objectiveCount >= this.objectiveCount)
        {
            return true;
        }

        return false;
    }
}
