using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class UIPlayer : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private Image playerHealthBar;
    [SerializeField] private GameObject playerHealthBarObject;

    [SerializeField] private TMP_Text playerHealthText;
    [SerializeField] private GameObject playerHealthTextObject;

    private TMP_Text[] killUsernames;
    [SerializeField] private GameObject killUI, killInfoObject;

    [Header("Prompts")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private TMP_Text interactionText;

    
    [Header("Match Details")]
    [SerializeField] private GameObject matchDetails;
    [SerializeField] private TMP_Text matchTimer;
    [SerializeField] public TMP_Text myObjective, enemyObjective;
    
    [Header("Respawn")]
    [SerializeField] private TMP_Text respawnTimer;
    private float remainingRespawnTime;

    [Space]

    [SerializeField] public TMP_Text otherPlayerName;
    [SerializeField] private GameObject respawnObject;

    [SerializeField] private UIWeapon weaponUI;
    public PlayerHealth playerHealthScript { get; set; }
    private PlayerManager playerManager;
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

        playerManager = transform.root.GetComponent<PlayerManager>();

        EnableHUD();
    }

    private void Update() 
    {
        if (!pv.IsMine) { return; }
        if (playerManager.currentState == GameState.Ending) 
        {
            StopAllCoroutines();
            respawnObject.SetActive(false);
            
            return;
        }

        UpdateVariables();
        UpdateHealthBar();
    }

    private void UpdateVariables() 
    {
        if (remainingRespawnTime > 0) {
            remainingRespawnTime -= Time.deltaTime;
        }

        respawnTimer.text = string.Format($"Respawning in: {(int)remainingRespawnTime + 1}");
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

    public void DisplayInteractionPrompt(string interactableName, bool isPowerWeapon)
    {
        string promptColour = "";
        
        if (isPowerWeapon) {
            promptColour = "#FFEF00";
        } else {
            promptColour = "#89CD34";
        }
    
        interactionText.text = $"Press <color=#56D2CE>{InputManager.INTERACT}</color> to pick up <color={promptColour}><b>{interactableName}</b></color>";
        interactionPrompt.SetActive(true);
    }

    public void DisableInteractionPrompt()
    {
        interactionPrompt.SetActive(false);
    }

    public void RefreshTimer(string time)
    {
        matchTimer.text = time;
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
        matchDetails.SetActive(false);
        DisableInteractionPrompt();
    }

    private IEnumerator RespawnCountdown(float time) {
        remainingRespawnTime = time;

        DisableHUD();
        weaponUI.DisableHUD();

        respawnObject.SetActive(true);

        yield return new WaitForSeconds(time);

        respawnObject.SetActive(false);
    }

}
