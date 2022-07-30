using UnityEngine;
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

    [SerializeField] private GameObject weaponHUD;

    public WeaponManager weaponManager;
    private Weapon currentWeapon;

    private PhotonView pv;

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
        
        ammoCountText.text = currentWeapon.GetCurrentAmmo().ToString();
        ammoReserveText.text = currentWeapon.GetReserveAmmo().ToString();

        UpdateSingleFireText();
        UpdateTextStates();
    }

    private void UpdateVariables() 
    {
        if(weaponManager == null) return;
        
        currentWeapon = weaponManager.GetCurrentWeapon();

    }

    private void UpdateSingleFireText()
    {
        if (currentWeapon.IsSingleFire())
                    fireModeText.text = "S";
                else 
                    fireModeText.text = "A";
    }

    private void UpdateTextStates() 
    {
        if (currentWeapon.GetCurrentAmmo() <= 0 && currentWeapon.GetReserveAmmo() <= 0)
            reloadText.text = "NO AMMO";
        else
            reloadText.text = "RELOADING";

        if(currentWeapon.IsReloading() || currentWeapon.GetCurrentAmmo() <= 0 && currentWeapon.GetReserveAmmo() <= 0) {
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
}
