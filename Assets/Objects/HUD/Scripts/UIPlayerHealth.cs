using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class UIPlayerHealth : MonoBehaviour
{
    [SerializeField] private Image playerHealthBar;
    [SerializeField] private GameObject playerHealthBarObject;

    [SerializeField] private TextMeshProUGUI playerHealthText;
    [SerializeField] private GameObject playerHealthTextObject;

    
    [Header("Timer")]
    private float remainingTime;
    [SerializeField] private TextMeshProUGUI timerCounter;
    [SerializeField] private GameObject respawnObject;

    [SerializeField] private UIWeapon weaponUI;
    public PlayerHealth playerHealthScript { get; set; }
    private PhotonView pv;

    private void Awake() 
    {
        pv = GetComponent<PhotonView>();
    }

    private void Start() 
    {
        if (!pv.IsMine) {
            Destroy(this.gameObject); 
            return; 
        }

        EnableHUD();
    }

    private void Update() 
    {
        if (!pv.IsMine) { return; }

        UpdateVariables();
        UpdateHealthBar();
    }

    private void UpdateVariables() 
    {
        if (remainingTime > 0) {
            remainingTime -= Time.deltaTime;
        }

        timerCounter.text = string.Format($"Respawning in: {(int)remainingTime + 1}");
    }

    private void UpdateHealthBar() 
    {
        float _maxHealth = playerHealthScript.GetHealth().x;
        float _currentHealth = playerHealthScript.GetHealth().y;

        playerHealthBar.transform.localScale = new Vector3((_currentHealth / _maxHealth), 1, 1);
        playerHealthText.text = ((int)_currentHealth).ToString();
    }

    public void HUD_Respawn(float time) 
    {
        StartCoroutine(RespawnCountdown(time));
    }

    public void EnableHUD()
    {
        respawnObject.SetActive(false);
        playerHealthTextObject.SetActive(true);
        playerHealthBarObject.SetActive(true);
    }

    public void DisableHUD() 
    {
        playerHealthTextObject.SetActive(false);
        playerHealthBarObject.SetActive(false);
    }

    private IEnumerator RespawnCountdown(float time) {
        
        remainingTime = time;

        DisableHUD();
        weaponUI.DisableHUD();

        respawnObject.SetActive(true);

        yield return new WaitForSeconds(time);

        respawnObject.SetActive(false);
    }

}
