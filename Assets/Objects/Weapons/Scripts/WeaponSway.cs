using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Weapon))]
public class WeaponSway : MonoBehaviour
{
    private float weaponSwayIntensity;
    private float weaponSwaySmoothing;
    private Weapon WeaponScript;
    private PhotonView pv;

    private void Start() {
        pv = GetComponent<PhotonView>();

        if (!pv.IsMine) { return; }

        WeaponScript = GetComponent<Weapon>();
    }

    private void Update() {
        if (!pv.IsMine) { return; }

        weaponSwayIntensity = WeaponScript.GetWeaponData().weaponSwayIntensity;
        weaponSwaySmoothing = WeaponScript.GetWeaponData().weaponSwaySmoothing;

        float _mouseX = Input.GetAxisRaw("Mouse X") * weaponSwayIntensity;
        float _mouseY = Input.GetAxisRaw("Mouse Y") * weaponSwayIntensity;
        
        Sway(_mouseX, _mouseY);
    }

    private void Sway(float _mouseX, float _mouseY) {
        Quaternion _rotationX = Quaternion.AngleAxis(_mouseY, Vector3.right);
        Quaternion _rotationY = Quaternion.AngleAxis(-_mouseX, Vector3.up);

        if(WeaponScript.isAiming || WeaponScript.isReloading) { 
            _rotationX = Quaternion.AngleAxis(0, Vector3.right);
            _rotationY = Quaternion.AngleAxis(0, Vector3.up);
        }

        Quaternion targetRotation = _rotationX * _rotationY;

        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, weaponSwaySmoothing * Time.fixedDeltaTime);
    }

}
