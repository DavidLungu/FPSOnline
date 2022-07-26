using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private TMP_Text[] usernameTexts;

    private void Start()
    {
        foreach (TMP_Text usernameText in usernameTexts)
        {
            usernameText.text = Launcher.Instance.myProfile.username;
        }
    }
}
