using UnityEngine;
using Photon.Pun;

public class CameraRecoil : MonoBehaviour
{
    private Vector3 currentRotation;
    private Vector3 targetRotation;

    private Vector3 hipFireRecoil;
    private Vector3 aimFireRecoil;

    private float recoilTravelSpeed;
    private float recoilReturnSpeed;
    
    [SerializeField] private WeaponManager weaponManager;

    private Weapon currentWeapon;
    private WeaponData currentWeaponData;
    private PhotonView pv;

    private void Awake() {
        pv = GetComponent<PhotonView>();
    }

    private void Start() {
        if(!pv.IsMine) { return; }

        weaponManager = GetComponentInChildren<WeaponManager>();
    }

    private void Update() {
        if (!pv.IsMine) { return; }

        currentWeapon = weaponManager.GetCurrentWeapon();

        if (currentWeapon != null) 
        {
            currentWeaponData = currentWeapon.GetWeaponData();
        }
    }

    private void FixedUpdate() {
        if (!pv.IsMine) { return; }
        if (currentWeapon == null) { return; }

        CalculateRecoil();
        
        if(currentWeapon.isShooting) { RecoilFire(currentWeapon); }    
    }

    private void CalculateRecoil() {
        recoilTravelSpeed = currentWeaponData.recoilTravelSpeed;
        recoilReturnSpeed = currentWeaponData.recoilReturnSpeed;
        
        hipFireRecoil = currentWeaponData.hipFireRecoil;
        aimFireRecoil = currentWeaponData.aimFireRecoil;

        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, recoilReturnSpeed * Time.deltaTime);
        currentRotation = Vector3.Lerp(currentRotation, targetRotation, recoilTravelSpeed * Time.deltaTime);
        transform.localRotation = Quaternion.Euler(currentRotation);
    }

    public void RecoilFire(Weapon _weapon) {
        if (_weapon.isAiming) { 
            targetRotation += new Vector3(
                aimFireRecoil.x * 0.1f, 
                Random.Range(-aimFireRecoil.y, aimFireRecoil.y) * 0.1f, 
                Random.Range(-aimFireRecoil.z, aimFireRecoil.z) * 0.1f
            );

        }
        else {
            targetRotation += new Vector3(
                hipFireRecoil.x * 0.1f, 
                Random.Range(-hipFireRecoil.y, hipFireRecoil.y) * 0.1f, 
                Random.Range(-hipFireRecoil.z, hipFireRecoil.z) * 0.1f
            );
        }
    }
}
