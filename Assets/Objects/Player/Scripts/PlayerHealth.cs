using UnityEngine;
using Photon.Pun;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health")]
    private int currentPlayerHealth;
    [SerializeField] private int maxPlayerHealth;

    [Header("Regeneration")]
    [SerializeField] private float healthRegenCooldown;
    [SerializeField] private float healthRegenRate;
    private float healthRegenTimer;

    private bool onCooldown;
    private bool canRegenerate;
    private bool addHealth = true;

    private PlayerManager playerManager;
    private PhotonView pv;
    
    public Vector2 GetHealth() { return new Vector2(maxPlayerHealth, currentPlayerHealth); }

    private void Awake() {
        pv = GetComponent<PhotonView>();
        playerManager = PhotonView.Find((int)pv.InstantiationData[0]).GetComponent<PlayerManager>();
    }

    private void Start() {
        if (!pv.IsMine) { return; }

        pv.RPC(nameof(RPC_UpdateHealth), RpcTarget.All, maxPlayerHealth, !addHealth);
    }

    private void Update() {
        if (!pv.IsMine) { return; }

        if(Input.GetKeyDown(KeyCode.F)) TakeDamage(200, -1); // DEBUGGING //

        RegenerateHealth();
    }

    [PunRPC]
    void RPC_UpdateHealth(int health, bool addHealth) {
        if (!pv.IsMine) { return; }

        if (addHealth) {
            currentPlayerHealth += health;
        }
        else {
            currentPlayerHealth = health;
        }
    }

    public void TakeDamage(int damageAmount, int playerActor) {
        pv.RPC(nameof(RPC_TakeDamage), RpcTarget.All, -damageAmount, playerActor);
    }

    [PunRPC]
    void RPC_TakeDamage(int damageAmount, int playerActor) {
        if (!pv.IsMine) { return; }

        pv.RPC(nameof(RPC_UpdateHealth), RpcTarget.All, damageAmount, addHealth);
        
        if (currentPlayerHealth <= 0) {
            KillPlayer(playerActor);
            return;
        }

        Debug.Log("Regenerating");
        canRegenerate = false;
        healthRegenTimer = healthRegenCooldown;
        onCooldown = true;

        Debug.Log(currentPlayerHealth); // DEBUGGING //
    }
    private void RegenerateHealth() {   
        if (onCooldown) {
            healthRegenTimer -= Time.deltaTime;

            if(healthRegenTimer <= 0) {
                canRegenerate = true;
                onCooldown = false;
            }
        }

        if(canRegenerate) 
        {
            if(currentPlayerHealth <= maxPlayerHealth - 0.01f) {
                pv.RPC(nameof(RPC_UpdateHealth), RpcTarget.All, maxPlayerHealth, !addHealth);
            } 
            else {
                healthRegenTimer = healthRegenCooldown;
                canRegenerate = false;
            }        
        }
    }

    private void KillPlayer(int playerActor) 
    {        
        playerManager.ChangeStatSend(PhotonNetwork.LocalPlayer.ActorNumber, 1, 1); // death

        if (playerActor >= 0)                                // other player
            playerManager.ChangeStatSend(playerActor, 0, 1); // kill

        playerManager.DestroyPlayer();
    }
}
