using UnityEngine;
using Photon.Pun;

public class CameraFOV : MonoBehaviour
{
    private float defaultWeaponFOV, defaultPlayerFOV;
    private float playerAimFOVMultiplier, weaponAimFOVMultiplier;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Camera weaponCamera;
    [SerializeField] private Camera spectatorCamera;
    private Weapon currentWeapon;
    private PhotonView pv;

    private void Awake() 
    {
        pv = GetComponent<PhotonView>();
    }

    private void Start() 
    {
        if (!pv.IsMine) { return; }

        defaultWeaponFOV = weaponCamera.fieldOfView;
        defaultPlayerFOV = playerCamera.fieldOfView;
    }
    private void Update() 
    {
        if (!pv.IsMine) { return; }

        currentWeapon = gameObject.GetComponentInChildren<WeaponManager>().GetCurrentWeapon();
        playerAimFOVMultiplier = currentWeapon.GetWeaponData().playerAimFOVMultiplier;
        weaponAimFOVMultiplier = currentWeapon.GetWeaponData().weaponAimFOVMultiplier;

        AdjustCameraFOV();
    }

    private void AdjustCameraFOV() 
    {
        if(currentWeapon.IsAiming()) {
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, defaultPlayerFOV * playerAimFOVMultiplier, Time.deltaTime * 8f);
            weaponCamera.fieldOfView = Mathf.Lerp(weaponCamera.fieldOfView, defaultWeaponFOV * weaponAimFOVMultiplier, Time.deltaTime * 8f);
        } else {
            spectatorCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, defaultPlayerFOV, Time.deltaTime * 8f);
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, defaultPlayerFOV, Time.deltaTime * 8f);
            weaponCamera.fieldOfView = Mathf.Lerp(weaponCamera.fieldOfView, defaultWeaponFOV, Time.deltaTime * 8f);
        }   
    }
}
