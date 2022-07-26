using UnityEngine;
using Photon.Pun;
using System.Collections;
using System.IO;

public class Weapon : MonoBehaviour {
    private int weaponDamage;
    private int weaponHeadshotMultiplier;

    private int clipAmmo;
    private int reserveAmmo;
    private int currentAmmo;
    private int fireRate;
    private float reloadSpeed;
    private float aimSpeed;
    private float bulletSpread;
    private float playerAimFOVMultiplier;
    private float weaponAimFOVMultiplier;

    private Vector3 defaultWeaponPosition;
    private Vector3 aimingWeaponPosition;

    [SerializeField] private bool isSingleFire;
    
    public bool isWeaponHeld;
    private bool isShooting;
    private bool isReloading;
    private bool isAiming;
    private bool canAim;
    
    private GameObject bulletHolePrefab;
    private GameObject bulletImpactPrefab;
    private TrailRenderer bulletTrail;
    [SerializeField] private Transform bulletSpawnPoint;
    [SerializeField] private Transform playerCamera;
    [SerializeField] private PlayerController player;
    [SerializeField] private WeaponData weaponData;
    private CameraRecoil cameraRecoilScript;
    private PhotonView pv;
    
    public float GetCurrentAmmo() => currentAmmo;
    public float GetClipAmmo() => clipAmmo;
    public float GetReserveAmmo() => reserveAmmo;

    public bool IsShooting() => isShooting;
    public bool IsAiming() => isAiming;
    public bool IsReloading() => isReloading;
    public bool IsSingleFire() => isSingleFire;
    public WeaponData GetWeaponData() => weaponData;

    private void Awake() {
        pv = GetComponent<PhotonView>();
    }

    private void Start() {
        if(!pv.IsMine) { return; }
        
        InitializeWeaponData();

        player = transform.root.GetComponent<PlayerController>();

        // TEMP FIX, FIND OUT WHY IT IS NOT WORKING //
        // playerCamera = player.transform.Find("Cameras/MainViewCam").transform;
        
        cameraRecoilScript = playerCamera.parent.GetComponent<CameraRecoil>();
        bulletSpawnPoint = transform.Find("BulletSpawnPoint").transform;

        currentAmmo = clipAmmo;
        canAim = true;
    }

    private void Update() {
        if(!pv.IsMine) { return; }

        UpdateWeaponData();

        ReadInputs();

        Aim();       
    }

    private void ReadInputs() {
         if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentAmmo < clipAmmo && reserveAmmo > 0) {
            StartCoroutine(ReloadingCooldown());
        }

        if((isSingleFire ? Input.GetMouseButtonDown(0) : Input.GetMouseButton(0)) && !isShooting && !isReloading && currentAmmo > 0) {
            Shoot();
            StartCoroutine(currentAmmo <= 0 ? ReloadingCooldown() : ShootingCooldown());
        }
    }
    private void InitializeWeaponData() {

        clipAmmo = weaponData.clipAmmo;
        reserveAmmo = weaponData.reserveAmmo;
        bulletHolePrefab = weaponData.bulletHolePrefab;
        bulletImpactPrefab = weaponData.bulletImpactPrefab;
        //bulletTrail = weaponData.bulletTrail;
        weaponData.weaponModel.layer = 6;
    }

    private void UpdateWeaponData() {
        defaultWeaponPosition = weaponData.defaultWeaponPosition;
        aimingWeaponPosition = weaponData.aimingWeaponPosition;

        weaponDamage = weaponData.weaponDamage;
        weaponHeadshotMultiplier = weaponData.weaponHeadshotMultiplier;
        fireRate = weaponData.fireRate;
        reloadSpeed = weaponData.reloadSpeed;
        aimSpeed = weaponData.aimSpeed;
        bulletSpread = weaponData.bulletSpread;
        
        playerAimFOVMultiplier = weaponData.playerAimFOVMultiplier;
        weaponAimFOVMultiplier = weaponData.weaponAimFOVMultiplier;
    }

    private Vector3 BulletSpread() {
        Vector3 _bulletSpread = playerCamera.position + playerCamera.forward * 1000f;
        
        _bulletSpread += Random.Range(-bulletSpread, bulletSpread) * playerCamera.up;
        _bulletSpread += Random.Range(-bulletSpread, bulletSpread) * playerCamera.right;
        _bulletSpread.Normalize();

        return _bulletSpread;
    }

    private void InstantiateBulletHole(Vector3 hitPoint, Vector3 hitNormal) {
        GameObject _bulletHole = PhotonNetwork.Instantiate(
            Path.Combine("PhotonPrefabs", "Misc", "WeaponEffects", "BulletHolePrefab"), 
            hitPoint + hitNormal * 0.001f, 
            Quaternion.LookRotation(hitNormal, Vector3.up) 
        );        
    }

    private void InstantiateBulletImpact(Vector3 hitPoint, Vector3 hitNormal) {
        GameObject _bulletImpact = PhotonNetwork.Instantiate(Path.Combine(
            "PhotonPrefabs", "Misc", "WeaponEffects", "BulletImpactPrefab"), 
            hitPoint + hitNormal * 0.001f, 
            Quaternion.identity
        );
        
        ParticleSystem _impactParticles = _bulletImpact.GetComponentInChildren<ParticleSystem>();
        _bulletImpact.transform.LookAt(hitPoint + hitNormal);
        // EXTRA MODIFICATION LATER

        StartCoroutine(DestroyBulletImpact(_bulletImpact, _impactParticles.main.startLifetime.constant));
    }

    protected virtual void Shoot() {
        GameObject _hitObject;

        currentAmmo--;
        
        if (Physics.Raycast(playerCamera.position, (isAiming ? playerCamera.forward : BulletSpread()), out var hitInfo, 1000f)) {
            
            _hitObject = hitInfo.collider.transform.gameObject;
            
            if(hitInfo.collider.transform.gameObject.CompareTag("Player")) 
            {
                hitInfo.collider.gameObject.GetComponentInParent<IDamageable>()?.TakeDamage
                (
                    (_hitObject.name == "Head") ? weaponDamage * weaponHeadshotMultiplier : weaponDamage,
                    PhotonNetwork.LocalPlayer.ActorNumber
                );
            }
            else {
                pv.RPC(nameof(RPC_SpawnBulletEffects), RpcTarget.All, hitInfo.point, hitInfo.normal);
            }
        }

        pv.RPC(nameof(RPC_SpawnTrail), RpcTarget.All, hitInfo.point, (hitInfo.collider.gameObject ? hitInfo.collider.gameObject.name : null));
        
        // KICKBACK
    }

    [PunRPC]
    void RPC_SpawnBulletEffects(Vector3 hitPoint, Vector3 hitNormal) {
        InstantiateBulletHole(hitPoint, hitNormal);
        InstantiateBulletImpact(hitPoint, hitNormal);
    }

    [PunRPC]
    void RPC_SpawnTrail(Vector3 hitPoint, string hitObject){
        GameObject _trail = PhotonNetwork.Instantiate(
            Path.Combine("PhotonPrefabs", "Misc", "WeaponEffects", "BulletTrail"), 
            bulletSpawnPoint.position, Quaternion.identity
        );

        // TEMPORARY SOLUTION BECAUSE FINDING EACH OBJECT COLLIDER EVERY TIME WE SHOOT IS REALLY BAD FOR PERFORMANCE
        StartCoroutine(SpawnTrail(_trail.GetComponent<TrailRenderer>(), (GameObject.Find(hitObject).GetComponent<Collider>() ? hitPoint : playerCamera.position + playerCamera.forward * 100f)));
    }

    private void Aim() {
        player.canSprint = !isAiming;

        Vector3 _target = defaultWeaponPosition;

        if(Input.GetMouseButton(1) && canAim) {
            _target = aimingWeaponPosition;
            isAiming = true;
        } 
        else {
            _target = defaultWeaponPosition;
            isAiming = false;
        }

        Vector3 destinationPosition = Vector3.Lerp(transform.localPosition, _target, Time.deltaTime * aimSpeed);
        transform.localPosition = destinationPosition;
    }

    private void Reload() {
        int _ammoNeeded = clipAmmo - currentAmmo;

        if(_ammoNeeded >= reserveAmmo) {
            currentAmmo += reserveAmmo;
            reserveAmmo -= _ammoNeeded;
        } 
        else {
            currentAmmo = clipAmmo;
            reserveAmmo -= _ammoNeeded;
        }

        if(reserveAmmo <= 0) reserveAmmo = 0;
    }

    private IEnumerator ShootingCooldown() {
        isShooting = true;
        yield return new WaitForSeconds(1f / fireRate);
        isShooting = false;
    }

    private IEnumerator ReloadingCooldown() {
        isReloading = true;
        canAim = false;
        Reload();
        yield return new WaitForSeconds(1f / (reloadSpeed / 10));
        isReloading = false;
        canAim = true;
    }

    private IEnumerator DestroyBulletImpact(GameObject _bulletImpact, float delay) {
        yield return new WaitForSeconds(delay);
        PhotonNetwork.Destroy(_bulletImpact);
    }

    private IEnumerator SpawnTrail(TrailRenderer trail, Vector3 target) {
        float time = 0;
        Vector3 startPosition = trail.transform.position;

        while (time < 1) {
            trail.transform.position = Vector3.Lerp(startPosition, target, time);
            time += Time.deltaTime / trail.time;

            yield return null;
        }

        trail.transform.position = target;

        yield return new WaitForSeconds(trail.time);

        PhotonNetwork.Destroy(trail.gameObject);
        // SPAWN IMPACT 
    }
}
