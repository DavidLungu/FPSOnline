using UnityEngine;

[CreateAssetMenu(menuName="New Item/Weapon", fileName="New Weapon")]
public class WeaponData : ScriptableObject
{    
    [Header("Gun Data")]
    public string weaponName = "";

    public Vector3 defaultWeaponPosition;
    public Vector3 aimingWeaponPosition;

    [Header("Combat")]
    public int weaponDamage;
    public float weaponHeadshotMultiplier;
    
    [Header("Shooting")]
    public int reserveAmmo;
    public int clipAmmo;
    public int fireRate;
    public float reloadSpeed;
    public float aimSpeed;
    public float bulletSpread;

    [Header("Recoil")]
    public Vector3 hipFireRecoil;
    public Vector3 aimFireRecoil;
    public float recoilTravelSpeed;
    public float recoilReturnSpeed;

    [Header("Camera Settings")]
    public float playerAimFOVMultiplier;
    public float weaponAimFOVMultiplier;

   [Header("Sway")]
    public float weaponSwayIntensity;
    public float weaponSwaySmoothing;

    [Header("Extra")]
    public GameObject bulletHolePrefab;
    public GameObject bulletImpactPrefab;
    public TrailRenderer bulletTrail;

    [Header("Sound")]
    public AudioClip[] weaponAudioClips;
    public int shootAudioDistance, equipAudioDistance, reloadAudioDistance, aimAudioDistance;

}
