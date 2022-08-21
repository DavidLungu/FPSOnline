using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class MenuStartGame : MonoBehaviour
{
    [SerializeField] private GameObject leaveButton;

    [SerializeField] private TMP_Text timerText;
    [SerializeField] private Transform timerTextTransform;

    private float countdownTime = 4f;
    private float previousTime;
    private float timeRemaining;
    private bool isTimerRunning;

    [SerializeField] private AudioSource countdownSource, musicSource;
    [SerializeField] private AudioClip countdownSound, countdownCompleteSound;

    private float defaultMusicVolume;
    private bool isMusicFading;

    private PhotonView pv;

    private void OnEnable()
    {
        defaultMusicVolume = musicSource.volume;
    }

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }

    private void Start() 
    {
        if (pv.IsMine)
            EndCountdown();
    }

    private void Update()
    {
        if (!pv.IsMine) return;

        if (PhotonNetwork.IsMasterClient) 
        {   
            if (isTimerRunning)
            {
                pv.RPC(nameof(StartCountdown), RpcTarget.All);
            } else {
                pv.RPC(nameof(EndCountdown), RpcTarget.All);
            }
        }
    }

    [PunRPC]
    private void StartCountdown()
    {
        if(timeRemaining <= 1)
        {
            countdownSource.clip = countdownCompleteSound;
            
            if(timeRemaining <= 0) 
            {
                timeRemaining = 0;
                musicSource.Stop();
                Launcher.Instance.StartGame();
            }

        }

        if (previousTime != (int)timeRemaining)
        {
            countdownSource.Play();
        }
        previousTime = (int)timeRemaining;

        timeRemaining -= Time.deltaTime;
        musicSource.volume = ((timeRemaining / countdownTime) - 0.1f) * defaultMusicVolume;
        timerText.text = ((int)timeRemaining).ToString();
    }

    [PunRPC]
    private void EndCountdown()
    {
        isMusicFading = false;

        timerTextTransform.gameObject.SetActive(false);
        musicSource.volume = Mathf.Lerp(musicSource.volume, defaultMusicVolume, Time.deltaTime * 2f);
    }

    [PunRPC]
    public void DisableLeaveButton()
    {   
        leaveButton.SetActive(!leaveButton.activeSelf);
    }

    [PunRPC]
    private void ClickEvent()
    {
        countdownSource.clip = countdownSound;
        isTimerRunning = !isTimerRunning;

        timerTextTransform.gameObject.SetActive(isTimerRunning);
        
        timeRemaining = countdownTime;
        previousTime = countdownTime;
    }

    public void OnClick()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            pv.RPC(nameof(DisableLeaveButton), RpcTarget.All);
        }

        pv.RPC(nameof(ClickEvent), RpcTarget.All);
    }
}
