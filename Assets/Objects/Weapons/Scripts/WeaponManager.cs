using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Realtime;

public class WeaponManager : MonoBehaviourPunCallbacks {

    private Weapon currentWeapon;
    [SerializeField] public List<Weapon> loadout = new List<Weapon>();
    [SerializeField] private LayerMask weaponsLayer;
    
    private bool isSwapping;
    public int weaponIndex { get; private set; }
    private int previousWeaponIndex = -1;

    private PhotonView pv;

    public Weapon GetCurrentWeapon() { return currentWeapon; }

    private void Awake() 
    {
        pv = GetComponent<PhotonView>();
    }

    private void Start() 
    {
        if (!pv.IsMine) { return; }
        
        //pv.RPC(nameof(RPC_PickUp), RpcTarget.All, loadout[weaponIndex].GetWeaponData().weaponName);
    }

    private void Update() 
    {
        if(!pv.IsMine) { return; }
        if (currentWeapon == null) return;

        for(int i = 0; i < loadout.Count; i++) 
        {
            if (Input.GetKeyDown((i + 1).ToString())) {
                transform.root.GetComponent<PlayerController>().canSprint = true;
                Equip(i);
            }
        }
        
        for (int i = 0; i < loadout.Count; i++) 
        {
            loadout[weaponIndex].transform.GetChild(i).gameObject.layer = 6;
        }
    }

    [PunRPC]
    public void RPC_PickUp(string weaponName) 
    {
        if (!pv.IsMine) return;

        foreach (Weapon _weapon in loadout)  
        {
            if (_weapon.GetWeaponData().weaponName == weaponName) 
            {
                _weapon.MaxAmmo();
                return;
            }
        }

        Weapon _newWeapon = WeaponLibrary.FindWeapon(weaponName);
        object[] data = new object[2];
        data[0] = pv.ViewID;
        data[1] = string.Format($"{weaponName} picked up at position [{_newWeapon.transform.position}]");

        GameObject _newWeaponObject = PhotonNetwork.Instantiate
        (
            Path.Combine("PhotonPrefabs", "Weapons", _newWeapon.name), 
            Vector3.zero, transform.rotation,
            0, data
        );
        _newWeapon = _newWeaponObject.GetComponent<Weapon>();
        
        if (loadout.Count >= 2) 
        {            
            CleanLoadout(loadout[weaponIndex].gameObject);
            loadout[weaponIndex] = _newWeapon;
            isSwapping = true;
            Equip(weaponIndex);

        }
        else {
            loadout.Add(_newWeapon);
            Equip(loadout.Count - 1);
        }
    }

    private void CleanLoadout(GameObject weaponObject)
    {
        PhotonNetwork.Destroy(weaponObject);
    }

    [PunRPC]
    public void Equip(int index) 
    {
        if (index == previousWeaponIndex && !isSwapping) { return; }

        weaponIndex = index;
        
        loadout[weaponIndex].transform.localPosition = loadout[weaponIndex].GetWeaponData().defaultWeaponPosition;
        loadout[weaponIndex].gameObject.SetActive(true);
        loadout[weaponIndex].isWeaponHeld = true;
        currentWeapon = loadout[weaponIndex];

        if(previousWeaponIndex != -1 && !isSwapping) {
            loadout[previousWeaponIndex].StopAllCoroutines();
            loadout[previousWeaponIndex].IsReloading(false);
            loadout[previousWeaponIndex].IsAiming(false);
            loadout[previousWeaponIndex].IsShooting(false);
            loadout[previousWeaponIndex].gameObject.SetActive(false);
            loadout[previousWeaponIndex].isWeaponHeld = false;
        }

        previousWeaponIndex = weaponIndex;

        if(pv.IsMine) {
            Hashtable hash = new Hashtable();
            hash.Add("weaponIndex", weaponIndex);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }

        isSwapping = false;
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (!pv.IsMine && targetPlayer == pv.Owner) {
            Equip((int)changedProps["weaponIndex"]);
        }
    }
}
