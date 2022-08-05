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
    private float fireRate;
    private float reloadSpeed;
    private float aimSpeed;
    private float bulletSpread;
    private float playerAimFOVMultiplier;
    private float weaponAimFOVMultiplier;

    [SerializeField] private AudioClip[] weaponAudioClips;
    protected Vector3 defaultWeaponPosition { get; private set; }
    protected Vector3 aimingWeaponPosition { get; private set; }

    [SerializeField, Range(0, 2)] private int firingMode;
    
    public bool isWeaponHeld;
    public bool isShooting { get; private set; }
    public bool isReloading { get; private set; }
    public bool isAiming { get; protected set; }
    public bool canAim { get; private set; }
    public bool canShoot { get; private set; }
    private GameObject bulletHolePrefab;
    private GameObject bulletImpactPrefab;
    private TrailRenderer bulletTrail;
    [SerializeField] protected Transform bulletSpawnPoint;
    [SerializeField] private GameObject weaponModel;
    [SerializeField] public Transform playerCamera, weaponCamera;
    private AudioSource mainAudioSource, weaponAudioSource;
    [SerializeField] protected PlayerController player;
    [SerializeField] protected WeaponData weaponData;
    private CameraRecoil cameraRecoilScript;
    protected PhotonView pv;
    
    public float GetCurrentAmmo() => currentAmmo;
    public float GetClipAmmo() => clipAmmo;
    public float GetReserveAmmo() => reserveAmmo;

    public void ToggleShoot() { canShoot = true; }
    public void ToggleAim() { canAim = true; }


    public bool IsShooting(bool _isShooting) => isShooting = _isShooting;
    public bool IsAiming(bool _isAiming) => isAiming = _isAiming;
    public bool IsReloading(bool _isReloading) => isReloading = _isReloading;
    public int FiringMode() => firingMode;
    public WeaponData GetWeaponData() => weaponData;
    
    private UIWeapon weaponHUD;
    private PlayerManager playerManager;

    public enum FireMode
    {
        SEMI_FIRE,
        BURST_FIRE,
        AUTO_FIRE
    }

    protected virtual void Awake() 
    {
        pv = GetComponent<PhotonView>();
    }

    protected virtual void Start() 
    {
        if(!pv.IsMine) { return; }
        
        InitializeWeaponData();

        player = transform.root.GetComponent<PlayerController>();
        playerManager = transform.root.GetComponent<PlayerHealth>().playerManager;
        weaponHUD = playerManager.playerHUD.GetComponent<UIWeapon>();
        pv.RPC(nameof(RPC_InitializeVariables), RpcTarget.All);

        // TEMP FIX, FIND OUT WHY IT IS NOT WORKING //
        // playerCamera = player.transform.Find("Cameras/MainViewCam").transform;
        cameraRecoilScript = playerCamera.parent.GetComponent<CameraRecoil>();
        bulletSpawnPoint = transform.Find("BulletSpawnPoint").transform;


        PlaySound(0, weaponData.equipAudioDistance, false);

        currentAmmo = clipAmmo;
    }

    protected void OnEnable() 
    {
        if(pv == null) return;
        if(!pv.IsMine) return;

        canShoot = false;
        isAiming = false;

        PlaySound(0, weaponData.equipAudioDistance, false);
        transform.localPosition = weaponData.defaultWeaponPosition;
    }

    protected virtual void Update() 
    {
        if(!pv.IsMine) { return; }

        UpdateWeaponData();

        if (!canShoot) { return; }
        
        ReadInputs();

        Aim();       
    }

    protected void ReadInputs() 
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
    protected virtual void RPC_InitializeVariables()
    {
        mainAudioSource = playerCamera.GetComponent<AudioSource>();
        weaponAudioSource = weaponCamera.GetComponent<AudioSource>();

        mainAudioSource.rolloffMode = AudioRolloffMode.Custom;
        weaponAudioSource.rolloffMode = AudioRolloffMode.Custom;
    }
    
    protected virtual void InitializeWeaponData() 
    {
        clipAmmo = weaponData.clipAmmo;
        reserveAmmo = weaponData.reserveAmmo;
        bulletHolePrefab = weaponData.bulletHolePrefab;
        bulletImpactPrefab = weaponData.bulletImpactPrefab;
        bulletTrail = weaponData.bulletTrail;
        bulletTrail.gameObject.layer = 8;
        weaponModel.layer = 6;

        foreach (Transform child in weaponModel.transform) 
        {
            child.gameObject.layer = 6;
        }
    }

    protected virtual void UpdateWeaponData() 
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

    protected virtual Vector3 BulletSpread() 
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

    protected virtual void InstantiateBulletHole(Vector3 hitPoint, Vector3 hitNormal) 
    {
        GameObject _bulletHole = PhotonNetwork.Instantiate(
            Path.Combine("PhotonPrefabs", "Misc", "WeaponEffects", "BulletHolePrefab"), 
            hitPoint + hitNormal * 0.001f, 
            Quaternion.LookRotation(hitNormal, Vector3.up) 
        );        
    }

    protected virtual void InstantiateBulletImpact(Vector3 hitPoint, Vector3 hitNormal) 
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
                
                var damageState = (_hitObject.name == "Head" ? 0 : 1);
                weaponHUD.DisplayHitmarker((byte)damageState);

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
    protected virtual void RPC_SpawnBulletEffects(Vector3 hitPoint, Vector3 hitNormal) 
    {
        InstantiateBulletHole(hitPoint, hitNormal);
        InstantiateBulletImpact(hitPoint, hitNormal);
    }

    [PunRPC]
    protected virtual void RPC_SpawnTrail(Vector3 hitPoint, string hitObject)
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

    protected virtual void PlaySound(byte audioClip, int maxDistance, bool hasPriority)
    {
        if (hasPriority)
            pv.RPC(nameof(RPC_PlaySoundPriority), RpcTarget.All, audioClip, maxDistance);
        else
            pv.RPC(nameof(RPC_PlaySound), RpcTarget.All, audioClip, maxDistance);

    }

    [PunRPC]
    protected virtual void RPC_PlaySoundPriority(byte audioClip, int maxDistance)
    {
        mainAudioSource.PlayOneShot(weaponData.weaponAudioClips[audioClip]);
        mainAudioSource.maxDistance = maxDistance;
    }

    [PunRPC]
    protected virtual void RPC_PlaySound(byte audioClip, int maxDistance)
    {
        weaponAudioSource.PlayOneShot(weaponData.weaponAudioClips[audioClip]);
        weaponAudioSource.maxDistance = maxDistance;
    }

    protected virtual void Aim() 
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

    protected virtual IEnumerator ShootingCooldown() 
    {
        isShooting = true;
        yield return new WaitForSeconds(1f / fireRate);
        isShooting = false;
    }

    protected virtual IEnumerator BurstShootingCooldown()
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

    protected virtual IEnumerator ReloadingCooldown() 
    {
        PlaySound(2, weaponData.reloadAudioDistance, false);

        isReloading = true;
        canAim = false;
        isAiming = false;
        yield return new WaitForSeconds(1f / (reloadSpeed / 10));
        Reload();
        isReloading = false;
        canAim = true;
    }

    protected virtual IEnumerator SpawnTrail(TrailRenderer trail, Vector3 target) 
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
