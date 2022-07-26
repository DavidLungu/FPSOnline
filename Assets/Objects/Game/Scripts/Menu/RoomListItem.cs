using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class RoomListItem : MonoBehaviour
{
    [SerializeField] TMP_Text roomName;
    [SerializeField] TMP_Text roomPlayerCount;
    [SerializeField] TMP_Text roomMapName;

    public RoomInfo roomInfo;

    public void CreateRoom(RoomInfo _roomInfo)
    {
        roomInfo = _roomInfo;

        roomName.text = roomInfo.Name;
        roomPlayerCount.text = string.Format($"{roomInfo.PlayerCount}/{roomInfo.MaxPlayers}");
    }

    public void OnClick() 
    {
        Launcher.Instance.JoinRoom(roomInfo);
    }
}
