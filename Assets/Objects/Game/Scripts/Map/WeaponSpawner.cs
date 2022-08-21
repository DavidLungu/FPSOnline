using UnityEngine;
using Photon.Pun;

public class WeaponSpawner : MonoBehaviour, Interactable
{
    [SerializeField] private WeaponData weaponData;
    private string selectedWeaponName;
    private GameObject selectedWeaponModel;
    [SerializeField] Transform weaponModelDisplay, tempModelDisplay;
    [SerializeField] private bool isPowerWeapon;
    
    [Header("Active")]
    [SerializeField] private bool isRotating;
    [SerializeField] private float rotationSpeed;

    public bool isDisabled { get; private set; }
    private float wait;
    [SerializeField] private float cooldown;

    string Interactable.interactableName
    {
        get { return selectedWeaponName; }
        set { }
    }

    bool Interactable.isEmphasised
    {
        get {return isPowerWeapon; } 
        set { }
    }
    
    private PhotonView pv;

    private void Awake() 
    {
        pv = GetComponent<PhotonView>();

    }

    private void Start()
    {
        tempModelDisplay.gameObject.SetActive(false);
        GenerateItem();
    }

    private void GenerateItem()
    {
        selectedWeaponName = weaponData.weaponName;

        WeaponData _newWeaponData = WeaponLibrary.FindWeapon(selectedWeaponName).GetWeaponData();

        selectedWeaponModel = Instantiate(_newWeaponData.weaponModel, weaponModelDisplay.position, weaponModelDisplay.rotation, this.transform);
        selectedWeaponModel.transform.localScale = tempModelDisplay.localScale;
    }

    private void Update()
    {
        if (selectedWeaponModel != null && isRotating)
            if (selectedWeaponModel.activeSelf) RotateItem();
            
        if (isDisabled)
        {
            if (wait > 0)
            {
                wait -= Time.deltaTime;
            }

            else
            {
                EnablePickup();
            }
        }
    }

    private void RotateItem()
    {
        selectedWeaponModel.transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }

    public void Interact(GameObject player)
    {
        if (!isDisabled)
        {
            WeaponManager _weaponManager;

            _weaponManager = player.transform.root.Find("Cameras/CameraRot/CameraRecoil/ViewModelCam/Loadout").GetComponent<WeaponManager>();
            _weaponManager.photonView.RPC(nameof(_weaponManager.RPC_PickUp), RpcTarget.All, selectedWeaponName);
            pv.RPC(nameof(DisablePickup), RpcTarget.All);
        }
    }

    [PunRPC]
    private void DisablePickup()
    {
        isDisabled = true;
        wait = cooldown;

        selectedWeaponModel.SetActive(false);
    }

    private void EnablePickup()
    {
        isDisabled = false;
        wait = 0;

        selectedWeaponModel.SetActive(true);
    } 
}
