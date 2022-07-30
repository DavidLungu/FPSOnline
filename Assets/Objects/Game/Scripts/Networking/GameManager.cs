using UnityEngine;
using Photon.Pun;
using System.IO;
using TMPro;

public class GameManager : MonoBehaviour
{
    public int playerCount { get; set; }

    [SerializeField] private TMP_Text timerText;
    [SerializeField] private GameObject timerTextTransform;

    [SerializeField] private float countdownTime = 4.0f;
    private float previousTime;
    private float timeRemaining;
    private bool isTimerRunning;

    private bool playingMatch;

    [SerializeField] private GameObject mapSpectatorCamera;
    public GameObject playerManager { get; private set; }

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
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        timeRemaining = countdownTime;
    }

    private void Update()
    {
        if (playerCount == PhotonNetwork.CurrentRoom.PlayerCount && !playingMatch)
        {
            timerTextTransform.SetActive(true);
            
            StartCountdown();
        }
    }

    private void StartCountdown()
    {

        if(timeRemaining <= 1)
        {
            //audioSource.clip = countdownCompleteSound;
            
            if(timeRemaining <= 0) 
            {
                StartMatch();
            }
        }

        if(previousTime != (int)timeRemaining)
        {
            //audioSource.Play();
        }
        previousTime = (int)timeRemaining;

        timeRemaining -= Time.deltaTime;
        timerText.text = (timeRemaining).ToString("0,00");
    }

    private void StartMatch()
    {
        if (playingMatch) return;
        
        playerManager = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Player", "PlayerManager"), Vector3.zero, Quaternion.identity);
        mapSpectatorCamera.SetActive(false);
        playingMatch = true;
    }
}
