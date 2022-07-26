using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Map : MonoBehaviour
{
    public string mapDisplayName;
    public string mapName;
    public RawImage mapImage;

    private MapManager mapManager;

    private void Start()
    {
        mapManager = FindObjectOfType<MapManager>();
    }

    public void OnClick()
    {
        mapManager.SelectMap(mapName);
    }
}
