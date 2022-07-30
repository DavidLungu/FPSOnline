using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class UIPlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private Image playerHealthBar;
    [SerializeField] private GameObject playerHealthBarObject;

    [SerializeField] private TextMeshProUGUI playerHealthText;
    [SerializeField] private GameObject playerHealthTextObject;

    private TMP_Text[] killUsernames;
    [SerializeField] private GameObject killUI, killInfoObject;

    
    [Header("Timer")]
    [SerializeField] private TextMeshProUGUI timerCounter;
    private float remainingTime;

    [SerializeField] public TMP_Text otherPlayerName;
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

    public void DisplayRespawn(float time) 
    {

        StartCoroutine(RespawnCountdown(time));
    }

    public void DisplayKill(string otherPlayer)
    {
        var _killInfoObject = Instantiate(killInfoObject, Vector3.zero, Quaternion.identity, killUI.transform);

        killUsernames = _killInfoObject.transform.GetComponentsInChildren<TMP_Text>();

        foreach (var username in killUsernames)
        {
            username.text = string.Format($"Killed <color=red>{otherPlayer}</color>");
        }

        Destroy(_killInfoObject, _killInfoObject.GetComponent<Animation>().clip.length);

    }

    public void EnableHUD()
    {
        otherPlayerName.text = PhotonNetwork.LocalPlayer.NickName;
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
