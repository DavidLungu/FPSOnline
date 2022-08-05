using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using System.IO;
using TMPro;
using Photon.Realtime;

public enum GameMode
{
    FFA,
    TDM
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
    public bool isWinner;
    [SerializeField] private AudioClip winnerSound, loserSound;
    public int playerCount { get; set; }

    
    [Space]
    public GameMode mode = GameMode.FFA;
    public GameState state = GameState.Waiting;

    [SerializeField] private GameObject uiWaitForPlayers;

    [Header("Timer")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private GameObject timerTextTransform;
    [SerializeField] private float countdownTime = 4.0f;
    private float previousTime;
    private float timeRemaining;
    
    [Header("Misc")]
    [SerializeField] private GameObject mapSpectatorCamera;
    [SerializeField] private AudioSource audioSource;
    public GameObject playerManager { get; private set; }

    private string mainMenu = RoomManager.Instance.mainMenu;

    public static GameManager Instance;
    
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
    }

    private void Start()
    {
        timeRemaining = countdownTime;
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
        if (timeRemaining <= 1)
        {
            //audioSource.clip = countdownCompleteSound;
            
            if(timeRemaining <= 0) 
            {
                StartMatch();
            }
        }

        if (previousTime != (int)timeRemaining)
        {
            //audioSource.Play();
        }
        previousTime = (int)timeRemaining;

        timeRemaining -= Time.deltaTime;
        timerText.text = (timeRemaining).ToString("00");
    }

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
