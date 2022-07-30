using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class MapManager : MonoBehaviour
{
    private Map arenaMap;
    private Map[] arenaMaps;
    [SerializeField] private Transform mapHolder;
    [SerializeField] public Button mapSelectionButton;
    [SerializeField] private RawImage mapSelectionIcon;
    [SerializeField] private TMP_Text mapSelectionName;

    [SerializeField] private RawImage loadingMapIcon;
    [SerializeField] private TMP_Text loadingMapName;

    private RawImage selectedMapImage;
    private string selectedMapName;

    private PhotonView pv;

    private void Awake() 
    {
        pv = transform.GetComponent<PhotonView>();
    }

    private void Start()
    {
        arenaMaps = mapHolder.GetComponentsInChildren<Map>();
    }

    public void SelectMap(string _selectedMap)
    {
        foreach (Map _arenaMap in arenaMaps)
        {
            if (_selectedMap == _arenaMap.mapName)
            {
                Launcher.Instance.selectedMap = _selectedMap;
                arenaMap = _arenaMap;
                
                pv.RPC(nameof(RPC_ChangeMap), RpcTarget.All);

                Debug.Log("Selected " + _selectedMap);
            }
        }
    }

    [PunRPC]
    private void RPC_ChangeMap()
    {
        selectedMapImage = arenaMap.mapImage;
        selectedMapName = arenaMap.mapDisplayName;

        mapSelectionIcon.texture = selectedMapImage.texture;
        mapSelectionName.text = selectedMapName;

        loadingMapIcon.texture = selectedMapImage.texture;
        loadingMapName.text = selectedMapName;
    }

}
