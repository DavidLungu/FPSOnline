using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class UIWeapon : MonoBehaviour
{
    [Header("Weapon UI")]
    [SerializeField] private TextMeshProUGUI ammoCountText;
    [SerializeField] private TextMeshProUGUI ammoReserveText;
    [SerializeField] private GameObject ammoBackground;
    [SerializeField] private TextMeshProUGUI reloadText;
    [SerializeField] private TextMeshProUGUI fireModeText;

    [SerializeField] private GameObject hitMarkerObject;

    [SerializeField] private GameObject weaponHUD;

    public WeaponManager weaponManager;
    private Weapon currentWeapon;
    
    private PhotonView pv;

    private enum DamageState
    {
        HEADSHOT,
        BODYSHOT,
        KILLSHOT
    }

    private void Awake() 
    {
        pv = GetComponent<PhotonView>();

        if (!pv.IsMine) 
        { 
            Destroy(this.gameObject); 
            return;
        }
    }

    private void Update() 
    {
        if (!pv.IsMine) { return; }

        UpdateVariables();
        
        if (currentWeapon != null) 
        {
            ammoCountText.text = currentWeapon.GetCurrentAmmo().ToString();
            ammoReserveText.text = currentWeapon.GetReserveAmmo().ToString();

            UpdateSingleFireText();
            UpdateTextStates();
        }
    }

    private void UpdateVariables() 
    {
        if(weaponManager == null) return;
        
        currentWeapon = weaponManager.GetCurrentWeapon();

    }

    private void UpdateSingleFireText()
    {
        if (currentWeapon.FiringMode() == 0)
                    fireModeText.text = "S";
        else if (currentWeapon.FiringMode() == 1)
                    fireModeText.text = "B";
        else if (currentWeapon.FiringMode() == 2)
                    fireModeText.text = "A";
    }

    private void UpdateTextStates() 
    {
        if (currentWeapon.GetCurrentAmmo() <= 0 && currentWeapon.GetReserveAmmo() <= 0)
            reloadText.text = "NO AMMO";
        else
            reloadText.text = "RELOADING";

        if(currentWeapon.isReloading || currentWeapon.GetCurrentAmmo() <= 0 && currentWeapon.GetReserveAmmo() <= 0) {
            ammoCountText.transform.gameObject.SetActive(false);
            ammoReserveText.transform.gameObject.SetActive(false);
            fireModeText.transform.gameObject.SetActive(false);
            reloadText.transform.gameObject.SetActive(true);
        }
        else {
            reloadText.transform.gameObject.SetActive(false);
            ammoCountText.transform.gameObject.SetActive(true);
            ammoReserveText.transform.gameObject.SetActive(true);
            fireModeText.transform.gameObject.SetActive(true);
        }
    }

    public void DisableHUD() 
    {
        weaponHUD.SetActive(false);
    }

    public void DisplayHitmarker(byte state)
    {
        Color hitmarkerColour = Color.white;

        if (state == (byte)DamageState.HEADSHOT) hitmarkerColour = Color.yellow;
        
        else if (state == (byte)DamageState.BODYSHOT) hitmarkerColour = Color.white;
        
        else if (state == (byte)DamageState.KILLSHOT) hitmarkerColour = Color.red;

        var _hitMarker = GameObject.Instantiate(hitMarkerObject, transform, false);
        _hitMarker.GetComponent<Image>().color = hitmarkerColour;
        Destroy(_hitMarker, 0.5f);
    }
}
