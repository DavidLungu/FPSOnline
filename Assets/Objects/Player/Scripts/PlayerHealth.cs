using UnityEngine;
using Photon.Pun;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health")]
    private float currentPlayerHealth;
    [SerializeField] private float maxPlayerHealth;

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

    private void Awake() 
    {
        pv = GetComponent<PhotonView>();
        playerManager = PhotonView.Find((int)pv.InstantiationData[0]).GetComponent<PlayerManager>();
    }

    private void Start() 
    {
        if (!pv.IsMine) { return; }

        pv.RPC(nameof(RPC_UpdateHealth), RpcTarget.All, maxPlayerHealth, !addHealth);
    }

    private void Update() 
    {
        if (!pv.IsMine) { return; }

        if(Input.GetKeyDown(KeyCode.G)) TakeDamage(200, -1); // DEBUGGING //

        RegenerateHealth();
    }

    [PunRPC]
    void RPC_UpdateHealth(float health, bool addHealth) 
    {
        if (!pv.IsMine) { return; }

        if (addHealth) {
            currentPlayerHealth += health;
        }
        else {
            currentPlayerHealth = health;
        }
    }

    public void TakeDamage(float damageAmount, int otherPlayerActor) 
    {
        Debug.Log(nameof(TakeDamage));
        pv.RPC(nameof(RPC_TakeDamage), RpcTarget.All, -damageAmount, otherPlayerActor);
    }

    [PunRPC]
    void RPC_TakeDamage(float damageAmount, int otherPlayerActor) 
    {
        if (!pv.IsMine) { return; }
        Debug.Log(nameof(RPC_TakeDamage));

        pv.RPC(nameof(RPC_UpdateHealth), RpcTarget.All, damageAmount, addHealth);
        
        if (currentPlayerHealth <= 0) 
        {
            Die(otherPlayerActor);
            return;
        }

        canRegenerate = false;
        healthRegenTimer = healthRegenCooldown;
        onCooldown = true;
    }

    private void Die(int otherActor) 
    {
        Debug.Log(nameof(Die));

        if (otherActor >= 0) { 
            playerManager.ChangeStatSend(otherActor, PhotonNetwork.LocalPlayer.ActorNumber, 0, 1);    // other player will get their kill
        }
        
        playerManager.ChangeStatSend(PhotonNetwork.LocalPlayer.ActorNumber, -1, 1, 1);

        playerManager.DestroyPlayer();
    }

    private void RegenerateHealth() 
    {   
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
}
