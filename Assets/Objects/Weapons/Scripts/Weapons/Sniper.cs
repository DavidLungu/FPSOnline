using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sniper : Weapon
{
    [SerializeField] private Camera scopeCamera;
    [SerializeField] private Camera mainViewCamera;
    [SerializeField] private Camera viewModelCamera;

    [SerializeField] private float scopeAimFOVMultiplier;
    [SerializeField] GameObject scopeTexture;
    [SerializeField] private Transform defaultBulletSpawn, aimBulletSpawn;
    [SerializeField] private Animator animator;

    protected override void Start()
    {
        if (!pv.IsMine)
        {
            aimBulletSpawn.localPosition = defaultBulletSpawn.position;
            return;
        }
        
        base.Start();

        this.mainViewCamera = transform.root.Find("Cameras").GetChild(0).GetChild(0).Find("MainViewCam").GetComponent<Camera>();    
        this.viewModelCamera = transform.root.Find("Cameras").GetChild(0).GetChild(0).Find("ViewModelCam").GetComponent<Camera>();   
    }

    protected override void Update()
    {
        if (!pv.IsMine) return;
        
        base.Update();

        scopeCamera.transform.position = aimBulletSpawn.position;

        ToggleScope();
    }

    protected override void Shoot()
    {
        base.Shoot();
        
        // if (isAiming) {
        //    animator.SetTrigger("isShooting");
        // }
    }

    protected override void Aim()
    { 
        player.canSprint = !isAiming;

        Vector3 _target = defaultWeaponPosition;

        if(Input.GetButton(InputManager.AIM) && canAim) {
            _target = aimingWeaponPosition;
        } 
        else {
            _target = defaultWeaponPosition;
        }

        if (Input.GetButtonDown(InputManager.AIM) && canAim) 
        {
            PlaySound(3, weaponData.aimAudioDistance, false);
        } 

        if (transform.localPosition == aimingWeaponPosition) { isAiming = true; } 
        else { isAiming = false; }

        Vector3 destinationPosition = Vector3.MoveTowards(transform.localPosition, _target, Time.deltaTime * weaponData.aimSpeed);
        transform.localPosition = destinationPosition;
    }

    private void ToggleScope()
    {        
        if (viewModelCamera.fieldOfView <= 7f)
        {
            this.mainViewCamera.enabled = false;
        }


        if (isAiming) {
            bulletSpawnPoint.position = new Vector3(aimBulletSpawn.position.x, aimBulletSpawn.position.y - 0.1f, aimBulletSpawn.position.z);
    
            scopeCamera.gameObject.SetActive(true);
            scopeTexture.SetActive(true);

            if (Input.GetButton(InputManager.SPRINT))
            {
                scopeCamera.fieldOfView = Mathf.Lerp(scopeCamera.fieldOfView, weaponData.scopeAimFOVMultiplier * weaponData.scopeAimZoomFOVMultiplier, Time.deltaTime * 8f);
            }
            else {
                scopeCamera.fieldOfView = Mathf.Lerp(scopeCamera.fieldOfView, weaponData.scopeAimFOVMultiplier, Time.deltaTime * 8f);
            }

        }    
        else {
            this.mainViewCamera.enabled = true;
            
            scopeCamera.gameObject.SetActive(false);
            scopeTexture.SetActive(false);

            bulletSpawnPoint.position = defaultBulletSpawn.position;
        }
    }
}
