using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class PlayerListItem : MonoBehaviourPunCallbacks
{
    [SerializeField] TMP_Text playerName;
    [SerializeField] Image ownerIcon;

    private Player player;

    public void AddPlayer(Player _player)
    {
        player = _player;
        playerName.text = _player.NickName;
        // ownerIcon.enabled = gameObject.GetComponent<PhotonView>().Owner.IsMasterClient;
        Debug.Log($"{player.NickName} has joined");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if(player == otherPlayer)
        {
            Destroy(gameObject);
        }
    }

    public override void OnLeftRoom()
    {
        Destroy(gameObject);
    }

}
