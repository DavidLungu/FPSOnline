using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class UILeaderboard : MonoBehaviour
{
    public GameObject leaderboardObject;
    [SerializeField] private Transform playerCardHolder;
    
    [Header("Match Details")]
    [SerializeField] private TMP_Text mapName;
    [SerializeField] private TMP_Text gamemodeName;

    public void Leaderboard(string mapName, string gamemodeName, List<PlayerInfo> playerInfo)
    {
        if (GameManager.Instance.gamemode == GameMode.FFA) gamemodeName = "FFA";
        if (GameManager.Instance.gamemode == GameMode.TDM) gamemodeName = "TDM";

        for (int i = 2; i < playerCardHolder.childCount; i++)
        {
            Destroy(playerCardHolder.GetChild(i).gameObject);
        }

        this.mapName.text = mapName;
        this.gamemodeName.text = gamemodeName;

        GameObject playerCard = playerCardHolder.GetChild(1).gameObject;
        playerCard.SetActive(false);

        List<PlayerInfo> sortedPlayers = SortPlayers(playerInfo);

        bool _alternateColours = false;
        
        foreach(PlayerInfo player in sortedPlayers)
        {
            GameObject newPlayerCard = Instantiate(playerCard, playerCardHolder);
            Color32 cardColour = newPlayerCard.GetComponent<RawImage>().color;

            // if (player.playerActor == PhotonNetwork.LocalPlayer.ActorNumber) new Color32((byte)(cardColour.r - 80), (byte)(cardColour.g - 18), (byte)(cardColour.b - 18), cardColour.a);
                
            if (_alternateColours) cardColour = new Color32(cardColour.r, cardColour.g, cardColour.b, 180);
            _alternateColours = !_alternateColours;
            
            newPlayerCard.transform.Find("PlayerName").GetComponent<TMP_Text>().text = player.profile.username;
            newPlayerCard.transform.Find("Kills").GetComponent<TMP_Text>().text = player.kills.ToString();
            newPlayerCard.transform.Find("Deaths").GetComponent<TMP_Text>().text = player.deaths.ToString();
            newPlayerCard.transform.Find("Score").GetComponent<TMP_Text>().text = (player.kills * 100).ToString();

            newPlayerCard.SetActive(true);
        }

        leaderboardObject.gameObject.SetActive(true);
    }

    private List<PlayerInfo> SortPlayers(List<PlayerInfo> playerInfo)
    {
        List<PlayerInfo> sortedPlayerInfo = new List<PlayerInfo>();

        while(sortedPlayerInfo.Count < playerInfo.Count)
        {
            short highest = -1;
            PlayerInfo selection = playerInfo[0];
            foreach (PlayerInfo info in playerInfo)
            {
                if (sortedPlayerInfo.Contains(info) || info.playerActor == PhotonNetwork.LocalPlayer.ActorNumber) continue;

                if (info.kills > highest)
                {
                    selection = info;
                    highest = info.kills;
                }
            }
            sortedPlayerInfo.Add(selection);

        }

        return sortedPlayerInfo;
    }
}
