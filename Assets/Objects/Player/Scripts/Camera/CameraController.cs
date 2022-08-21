using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CameraController : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField] private float sensitivity;
    [SerializeField] private float defaultSensitivityMultiplier = 0.2f;
    private float sensitivityMultiplier;

    [SerializeField] private Transform orientation;
    [SerializeField] private Transform playerModel;
    [SerializeField] private WeaponManager weaponManager;
    [SerializeField] private Transform cameraRot;
    [SerializeField] private Camera mainViewCam, viewModelCam, spectatorCam;
    private Vector2 mousePos;
    private Vector2 cameraRotation;
    
    private PhotonView pv;

    private void Awake() 
    {
        pv = GetComponent<PhotonView>();
    }

    private void Start() 
    {
        if (!pv.IsMine) {
            Destroy(mainViewCam);
            Destroy(viewModelCam);
            Destroy(spectatorCam);
            Destroy(mainViewCam.gameObject.GetComponent<AudioListener>());

            Destroy(mainViewCam.GetComponent<AudioListener>());
            Destroy(viewModelCam.GetComponent<AudioListener>());
            Destroy(spectatorCam.GetComponent<AudioListener>());
            return; 
        }

        SetCursor();
        weaponManager.gameObject.SetActive(true);
    }

    private void Update() 
    {
        if (!pv.IsMine) { return; }

        WeaponAimSensitivity();
        UpdateCameraRotation();
    }

    private void LateUpdate() 
    {
        if (!pv.IsMine) { return; }

        UpdatePlayerRotation();
    }

    private void UpdateCameraRotation() 
    {
        mousePos.x = Input.GetAxisRaw(InputManager.ROTATION_X);
        mousePos.y = Input.GetAxisRaw(InputManager.ROTATION_Y);

        cameraRotation.y +=  mousePos.x * sensitivity * sensitivityMultiplier;
        cameraRotation.x -= mousePos.y * sensitivity * sensitivityMultiplier;

        cameraRotation.x = Mathf.Clamp(cameraRotation.x, -90f, 90f);
    }

    private void UpdatePlayerRotation() 
    {
        cameraRot.localRotation = Quaternion.Euler(cameraRotation.x, cameraRotation.y, 0);
        orientation.transform.localRotation = Quaternion.Euler(0, cameraRotation.y, 0);
        playerModel.transform.localRotation = orientation.localRotation;

        spectatorCam.transform.localRotation = cameraRot.localRotation;
    }

    private void WeaponAimSensitivity()
    {
        if (weaponManager.GetComponent<WeaponManager>().GetCurrentWeapon().isAiming)
        {
            sensitivityMultiplier =  (defaultSensitivityMultiplier * weaponManager.GetCurrentWeapon().GetWeaponData().aimSensitivityMultiplier) / 10f;
        }
        else {
            sensitivityMultiplier = defaultSensitivityMultiplier / 10f;
        }
    }

    private void SetCursor() 
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
