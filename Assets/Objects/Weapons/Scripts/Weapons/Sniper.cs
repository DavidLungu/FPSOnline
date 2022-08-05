using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sniper : Weapon
{
    [SerializeField] private Camera scopeCamera;
    [SerializeField] private Camera mainViewCamera;
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
    }

    protected override void Update()
    {
        if (!pv.IsMine) return;
        
        base.Update();

        scopeCamera.transform.position = aimBulletSpawn.position;
        scopeCamera.fieldOfView = weaponData.scopeAimFOVMultiplier;

        ToggleScope();
    }

    protected override void Aim()
    { 
        player.canSprint = !isAiming;

        Vector3 _target = defaultWeaponPosition;

        if(Input.GetMouseButton(1) && canAim) {
            _target = aimingWeaponPosition;
        } 
        else {
            _target = defaultWeaponPosition;
        }

        if (Input.GetMouseButtonDown(1)) 
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

        if (isAiming) {
            bulletSpawnPoint.position = new Vector3(aimBulletSpawn.position.x, aimBulletSpawn.position.y - 0.1f, aimBulletSpawn.position.z);
    
            scopeCamera.gameObject.SetActive(true);
            scopeTexture.SetActive(true);


            if (isShooting) {
                animator.Play($"{weaponData.weaponName}_Shoot");
            }
        }    
        else {
            this.mainViewCamera.enabled = true;
            
            scopeCamera.gameObject.SetActive(false);
            scopeTexture.SetActive(false);

            bulletSpawnPoint.position = defaultBulletSpawn.position;
        }

        if (mainViewCamera.fieldOfView <= 6f)
        {
            this.mainViewCamera.enabled = false;
        }

    }
}
