using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class MenuStartGame : MonoBehaviour
{
    
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private Transform timerTextTransform;

    private float countdownTime = 4.0f;
    private float previousTime;
    private float timeRemaining;
    private bool isTimerRunning;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip countdownSound, countdownCompleteSound;

    private void Start() 
    {
        EndCountdown();
    }

    private void Update()
    {
        if (isTimerRunning)
        {
            StartCountdown();
        } 
        else {
            EndCountdown();
        }
    }

    private void StartCountdown()
    {
        if(timeRemaining <= 1)
        {
            audioSource.clip = countdownCompleteSound;
            
            if(timeRemaining <= 0) 
                Launcher.Instance.StartGame();
        }

        if(previousTime != (int)timeRemaining)
        {
            audioSource.Play();
        }
        previousTime = (int)timeRemaining;

        timeRemaining -= Time.deltaTime;
        timerText.text = ((int)timeRemaining).ToString();
    }

    private void EndCountdown()
    {
        timerTextTransform.gameObject.SetActive(false);
    }

    public void OnClick()
    {
        audioSource.clip = countdownSound;
        isTimerRunning = !isTimerRunning;

        timerTextTransform.gameObject.SetActive(isTimerRunning);
        
        timeRemaining = countdownTime;
        previousTime = countdownTime;
    }
}
