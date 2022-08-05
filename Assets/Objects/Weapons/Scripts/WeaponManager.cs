using UnityEngine;
using Photon.Pun;
using System.IO;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Realtime;

public class WeaponManager : MonoBehaviourPunCallbacks {

    private Weapon currentWeapon;
    [SerializeField] private Weapon[] weapons;
    [SerializeField] private LayerMask weaponsLayer;
    
    public int weaponIndex { get; private set; }
    private int previousWeaponIndex = -1;

    private PhotonView pv;

    public Weapon GetCurrentWeapon() { return currentWeapon; }

    private void Awake() {
        pv = GetComponent<PhotonView>();
    }

    private void Start() {
        if(pv.IsMine) {
            previousWeaponIndex = -1;
            Equip(0);
        } else {
            return;
        }
    }

    private void Update() {
        if(!pv.IsMine) { return; }

        for(int i = 0; i < weapons.Length; i++) {
            if (Input.GetKeyDown((i + 1).ToString())) {
                transform.root.GetComponent<PlayerController>().canSprint = true;
                Equip(i);
            }
        }
        
        PickUp();
    }

    private void PickUp() {
        // Instantiate Weapon GameObjects 
        // Set positions to Loadout position
        // Parent them to loadout

        // Change child layers if ours
        for (int i = 0; i < weapons.Length; i++) {
            weapons[weaponIndex].transform.GetChild(i).gameObject.layer = 6;
        }
    }

    private void Equip(int _index) {
        if (_index == previousWeaponIndex) { return; }

        weaponIndex = _index;

        weapons[weaponIndex].gameObject.SetActive(true);
        weapons[weaponIndex].isWeaponHeld = true;
        currentWeapon = weapons[weaponIndex];

        if(previousWeaponIndex != -1) {
            weapons[previousWeaponIndex].StopAllCoroutines();
            weapons[previousWeaponIndex].IsReloading(false);
            weapons[previousWeaponIndex].IsAiming(false);
            weapons[previousWeaponIndex].IsShooting(false);
            weapons[previousWeaponIndex].gameObject.SetActive(false);
            weapons[previousWeaponIndex].isWeaponHeld = false;
        }

        previousWeaponIndex = weaponIndex;

        if(pv.IsMine) {
            Hashtable hash = new Hashtable();
            hash.Add("weaponIndex", weaponIndex);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (!pv.IsMine && targetPlayer == pv.Owner) {
            Equip((int)changedProps["weaponIndex"]);
        }
    }
}
