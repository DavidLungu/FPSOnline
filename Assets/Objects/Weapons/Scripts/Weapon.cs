using UnityEngine;
using Photon.Pun;
using System.Collections;
using System.IO;

public class Weapon : MonoBehaviour {
    private float weaponDamage;
    private float weaponHeadshotMultiplier;

    private int clipAmmo;
    private int reserveAmmo;
    private int currentAmmo;
    private int fireRate;
    private float reloadSpeed;
    private float aimSpeed;
    private float bulletSpread;
    private float playerAimFOVMultiplier;
    private float weaponAimFOVMultiplier;

    [SerializeField] private AudioClip[] weaponAudioClips;
    private Vector3 defaultWeaponPosition;
    private Vector3 aimingWeaponPosition;

    [SerializeField, Range(0, 2)] private int firingMode;
    
    public bool isWeaponHeld;
    private bool isShooting;
    private bool isReloading;
    private bool isAiming;
    private bool canAim;
    private bool canShoot;
    
    private GameObject bulletHolePrefab;
    private GameObject bulletImpactPrefab;
    private TrailRenderer bulletTrail;
    [SerializeField] private Transform bulletSpawnPoint;
    [SerializeField] private GameObject weaponModel;
    [SerializeField] private Transform playerCamera, weaponCamera;
    private AudioSource mainAudioSource, weaponAudioSource;
    [SerializeField] private PlayerController player;
    [SerializeField] private WeaponData weaponData;
    private CameraRecoil cameraRecoilScript;
    private PhotonView pv;
    
    public float GetCurrentAmmo() => currentAmmo;
    public float GetClipAmmo() => clipAmmo;
    public float GetReserveAmmo() => reserveAmmo;

    public void ToggleShoot() { canShoot = true; }

    public bool IsShooting() => isShooting;
    public bool IsAiming() => isAiming;
    public bool IsReloading() => isReloading;
    public int FiringMode() => firingMode;
    public WeaponData GetWeaponData() => weaponData;

    public enum FireMode
    {
        SEMI_FIRE,
        BURST_FIRE,
        AUTO_FIRE
    }

    private void Awake() 
    {
        pv = GetComponent<PhotonView>();
    }

    private void Start() 
    {
        if(!pv.IsMine) { return; }
        
        InitializeWeaponData();

        player = transform.root.GetComponent<PlayerController>();
        pv.RPC(nameof(RPC_InitializeVariables), RpcTarget.All);

        // TEMP FIX, FIND OUT WHY IT IS NOT WORKING //
        // playerCamera = player.transform.Find("Cameras/MainViewCam").transform;
        
        cameraRecoilScript = playerCamera.parent.GetComponent<CameraRecoil>();
        bulletSpawnPoint = transform.Find("BulletSpawnPoint").transform;


        PlaySound(0, weaponData.equipAudioDistance, false);

        currentAmmo = clipAmmo;
        canAim = true;
    }

    private void OnEnable() 
    {
        if(pv == null) return;
        if(!pv.IsMine) return;

        canShoot = false;

        PlaySound(0, weaponData.equipAudioDistance, false);

    }

    private void Update() 
    {
        if(!pv.IsMine) { return; }

        UpdateWeaponData();

        if (!canShoot) { return; }
        
        ReadInputs();

        Aim();       
    }

    private void ReadInputs() 
    {
        if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentAmmo < clipAmmo && reserveAmmo > 0) 
        {
            StartCoroutine(ReloadingCooldown());
        }

        if (!isShooting && !isReloading && currentAmmo > 0)
        {
            if (firingMode == (int)FireMode.SEMI_FIRE) 
            {
                if (Input.GetMouseButtonDown(0)) 
                {
                    Shoot();
                    StartCoroutine(currentAmmo <= 0 ? ReloadingCooldown() : ShootingCooldown());
                }
            }
            else if (firingMode == (int)FireMode.AUTO_FIRE) 
            {
                if (Input.GetMouseButton(0)) 
                {
                    Shoot();
                    StartCoroutine(currentAmmo <= 0 ? ReloadingCooldown() : ShootingCooldown());
                }
            }
            else if (firingMode == (int)FireMode.BURST_FIRE) 
            {
                if (Input.GetMouseButtonDown(0)) 
                {
                    StartCoroutine(currentAmmo <= 0 ? ReloadingCooldown() : BurstShootingCooldown());
                }
            }
        }

    }

    [PunRPC]
    void RPC_InitializeVariables()
    {
        mainAudioSource = playerCamera.GetComponent<AudioSource>();
        weaponAudioSource = weaponCamera.GetComponent<AudioSource>();

        mainAudioSource.rolloffMode = AudioRolloffMode.Custom;
        weaponAudioSource.rolloffMode = AudioRolloffMode.Custom;
    }
    
    private void InitializeWeaponData() 
    {
        clipAmmo = weaponData.clipAmmo;
        reserveAmmo = weaponData.reserveAmmo;
        bulletHolePrefab = weaponData.bulletHolePrefab;
        bulletImpactPrefab = weaponData.bulletImpactPrefab;
        bulletTrail = weaponData.bulletTrail;
        bulletTrail.gameObject.layer = 6;
        weaponModel.layer = 6;

        foreach (Transform child in weaponModel.transform) 
        {
            child.gameObject.layer = 6;
        }
    }

    private void UpdateWeaponData() 
    {
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

    private Vector3 BulletSpread() 
    {
        Vector3 _bulletSpread = playerCamera.position + playerCamera.forward * 1000f;

        if (!player.isGrounded || player.isSprinting) {
             _bulletSpread *= 2.0f;
        }

        _bulletSpread += Random.Range(-bulletSpread, bulletSpread) * playerCamera.up;
        _bulletSpread += Random.Range(-bulletSpread, bulletSpread) * playerCamera.right;
        _bulletSpread.Normalize();

        return _bulletSpread;
    }

    private void InstantiateBulletHole(Vector3 hitPoint, Vector3 hitNormal) 
    {
        GameObject _bulletHole = PhotonNetwork.Instantiate(
            Path.Combine("PhotonPrefabs", "Misc", "WeaponEffects", "BulletHolePrefab"), 
            hitPoint + hitNormal * 0.001f, 
            Quaternion.LookRotation(hitNormal, Vector3.up) 
        );        
    }

    private void InstantiateBulletImpact(Vector3 hitPoint, Vector3 hitNormal) 
    {
        GameObject _bulletImpact = PhotonNetwork.Instantiate(Path.Combine(
            "PhotonPrefabs", "Misc", "WeaponEffects", "BulletImpactPrefab"), 
            hitPoint + hitNormal * 0.001f, 
            Quaternion.identity
        );
        
        _bulletImpact.transform.LookAt(hitPoint + hitNormal);
        // EXTRA MODIFICATION LATER
    }

    protected virtual void Shoot() 
    {
        GameObject _hitObject;

        currentAmmo--;

        PlaySound(1, weaponData.shootAudioDistance, true);

        
        if (Physics.Raycast(playerCamera.position, (isAiming ? playerCamera.forward : BulletSpread()), out var hitInfo, 1000f)) {
            
            _hitObject = hitInfo.collider.transform.gameObject;
            
            if(_hitObject.CompareTag("Player")) 
            {
                _hitObject.GetComponentInParent<IDamageable>()?.TakeDamage
                (
                    (_hitObject.name == "Head") ? weaponDamage * weaponHeadshotMultiplier : weaponDamage,
                    PhotonNetwork.LocalPlayer.ActorNumber
                );

                Debug.Log($"({nameof(Shoot)}){PhotonNetwork.LocalPlayer.NickName}: {PhotonNetwork.LocalPlayer.ActorNumber}");
            }
            else {
                pv.RPC(nameof(RPC_SpawnBulletEffects), RpcTarget.All, hitInfo.point, hitInfo.normal);
            }
        }

        pv.RPC(nameof(RPC_SpawnTrail), RpcTarget.All, hitInfo.point, (hitInfo.collider.gameObject ? hitInfo.collider.gameObject.name : null));
        
        // KICKBACK
    }

    [PunRPC]
    void RPC_SpawnBulletEffects(Vector3 hitPoint, Vector3 hitNormal) 
    {
        InstantiateBulletHole(hitPoint, hitNormal);
        InstantiateBulletImpact(hitPoint, hitNormal);
    }

    [PunRPC]
    void RPC_SpawnTrail(Vector3 hitPoint, string hitObject)
    {
        if (!pv.IsMine) return;

        GameObject _trail = PhotonNetwork.Instantiate(
            Path.Combine("PhotonPrefabs", "Misc", "WeaponEffects", "BulletTrail"), 
            bulletSpawnPoint.position, 
            Quaternion.identity
        );

        // TEMPORARY SOLUTION BECAUSE FINDING EACH OBJECT COLLIDER EVERY TIME WE SHOOT IS REALLY BAD FOR PERFORMANCE
        StartCoroutine(SpawnTrail(_trail.GetComponent<TrailRenderer>(), (GameObject.Find(hitObject).GetComponent<Collider>() ? hitPoint : playerCamera.position + playerCamera.forward * 100f)));
    }

    private void PlaySound(byte audioClip, int maxDistance, bool hasPriority)
    {
        if (hasPriority)
            pv.RPC(nameof(RPC_PlaySoundPriority), RpcTarget.All, audioClip, maxDistance);
        else
            pv.RPC(nameof(RPC_PlaySound), RpcTarget.All, audioClip, maxDistance);

    }

    [PunRPC]
    void RPC_PlaySoundPriority(byte audioClip, int maxDistance)
    {
        mainAudioSource.PlayOneShot(weaponData.weaponAudioClips[audioClip]);
        mainAudioSource.maxDistance = maxDistance;
    }

    [PunRPC]
    void RPC_PlaySound(byte audioClip, int maxDistance)
    {
        weaponAudioSource.PlayOneShot(weaponData.weaponAudioClips[audioClip]);
        weaponAudioSource.maxDistance = maxDistance;
    }

    private void Aim() 
    {
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

        if (Input.GetMouseButtonDown(1)) 
        {
            PlaySound(3, weaponData.aimAudioDistance, false);
        } 

        Vector3 destinationPosition = Vector3.Lerp(transform.localPosition, _target, Time.deltaTime * aimSpeed);
        transform.localPosition = destinationPosition;
    }

    private void Reload() 
    {
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

    private IEnumerator ShootingCooldown() 
    {
        isShooting = true;
        yield return new WaitForSeconds(1f / fireRate);
        isShooting = false;
    }

    private IEnumerator BurstShootingCooldown()
    {
        isShooting = true;
        
        Shoot();
        yield return new WaitForSeconds(1f / fireRate);
        Shoot();
        yield return new WaitForSeconds(1f / fireRate);
        Shoot();
        yield return new WaitForSeconds(1f / fireRate);

        isShooting = false;
    }

    private IEnumerator ReloadingCooldown() 
    {
        PlaySound(2, weaponData.reloadAudioDistance, false);

        isReloading = true;
        canAim = false;
        Reload();
        yield return new WaitForSeconds(1f / (reloadSpeed / 10));
        isReloading = false;
        canAim = true;
    }

    private IEnumerator SpawnTrail(TrailRenderer trail, Vector3 target) 
    {
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
