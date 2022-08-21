using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviour, IPunInstantiateMagicCallback 
{
    [Header("Player Movement")]
    [SerializeField] private float defaultMoveSpeed;
    [SerializeField] private float walkMoveSpeedModifier;
    [SerializeField] private float sprintMoveSpeedModifier;
    [SerializeField] private float airMoveSpeedModifier;
    [SerializeField] private float jumpForceStrength;
    private float currentSpeed;
    private float horizontalMovement, verticalMovement;
    private Vector3 inputDirection = Vector2.zero;
    
    [Header("Grounding")]
    [SerializeField] private float groundDrag = 6f;
    [SerializeField] private float airDrag = 2f;

    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private float groundDetectRadius;

    [Header("Slopes")]
    [SerializeField] private float maxSlopeAngle;
    [SerializeField] private float slopeSpeedModifier;
    private RaycastHit slopeHit;

    public bool canSprint { get; set; }
    public bool isSprinting { get; private set; }
    public bool isMoving { get; private set; }
    public bool isGrounded { get; private set; }
    private bool exitingSlope;

    [SerializeField] public Transform playerModel { get; private set; }
    private Transform orientationTransform;
    private Transform groundDetectTransform;
    private Rigidbody playerRigid;

    public UIPlayer playerHUD { get; private set; }
    private PlayerManager playerManager;

    private PhotonView pv;

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] data = info.photonView.InstantiationData;

        playerManager = PhotonNetwork.GetPhotonView((int)data[0]).GetComponent<PlayerManager>();
    }

    private void Awake() 
    {
        pv = GetComponent<PhotonView>();
    }

    private void Start() 
    {
        if(!pv.IsMine) { return; }

        playerRigid = GetComponent<Rigidbody>();
        playerHUD = playerManager.playerHUD.GetComponent<UIPlayer>();
        
        orientationTransform = transform.root.Find("PlayerOrientation").transform;
        groundDetectTransform = transform.root.Find("PlayerModel/GroundDetect").transform;
    }

    private void Update() 
    {
        if(!pv.IsMine) { return; }

        currentSpeed = defaultMoveSpeed;

        horizontalMovement = Input.GetAxisRaw("Horizontal");
        verticalMovement = Input.GetAxisRaw("Vertical"); 

        if (horizontalMovement != 0 || verticalMovement != 0) {
            isMoving = true;
        }
        
        isMoving = false;


        ReadInput();
        IsGrounded();
        HandleDrag();
        GetMoveSlopeDirection();
        SlopeControl();
    }

    private void FixedUpdate() 
    {
        if(!pv.IsMine) { return; }

        ApplyMovement();
    }

    private void ReadInput() 
    {
        inputDirection = orientationTransform.forward * verticalMovement + orientationTransform.right * horizontalMovement;

        if (Input.GetButton(InputManager.SPRINT) && verticalMovement > 0)
            isSprinting = true;
        else if (Input.GetButtonUp(InputManager.SPRINT) || horizontalMovement != 0 || verticalMovement <= 0)
            isSprinting = false;

        if (Input.GetButtonDown(InputManager.JUMP))
            AddJumpForce();
    }

    private void ApplyMovement() 
    {
        if (isGrounded && !OnSlope())
            playerRigid.AddForce(inputDirection.normalized * currentSpeed * (canSprint && isSprinting ? sprintMoveSpeedModifier : walkMoveSpeedModifier), ForceMode.Acceleration);
        
        else if (isGrounded && OnSlope() && !exitingSlope)
            playerRigid.AddForce(GetMoveSlopeDirection() * currentSpeed * (canSprint && isSprinting ? sprintMoveSpeedModifier : walkMoveSpeedModifier) * slopeSpeedModifier, ForceMode.Force);
        
        else if (!isGrounded)
            playerRigid.AddForce(inputDirection.normalized * currentSpeed * (walkMoveSpeedModifier * airMoveSpeedModifier));
    }

    public float GetPlayerSpeed() { return currentSpeed; }
    public void ModifyPlayerSpeed(float speedModifier) => currentSpeed *= speedModifier;

    private void AddJumpForce() 
    {
        if (isGrounded) {
            playerRigid.velocity = new Vector3(playerRigid.velocity.x, jumpForceStrength, playerRigid.velocity.z);
            exitingSlope = true;
        }
    }

    private void HandleDrag() 
    {
        playerRigid.drag = isGrounded ? groundDrag : airDrag;
    }

    private void IsGrounded() 
    {
        exitingSlope = false;
        isGrounded = Physics.CheckSphere(groundDetectTransform.position, groundDetectRadius, groundLayerMask);
    }

    private void SlopeControl()
    {
        playerRigid.useGravity = !OnSlope();

        if (OnSlope() && !exitingSlope)
        {
            if (playerRigid.velocity.magnitude > currentSpeed) {
                playerRigid.velocity = playerRigid.velocity.normalized * currentSpeed;
            }

            if (playerRigid.velocity.y > 0) {
                playerRigid.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }
    }

    private Vector3 GetMoveSlopeDirection() 
    {
        return Vector3.ProjectOnPlane(inputDirection, slopeHit.normal).normalized;
    }

    private bool OnSlope() 
    {
        Debug.DrawRay(transform.position, Vector3.down);
        if (Physics.Raycast(groundDetectTransform.position, Vector3.down, out slopeHit, groundDetectRadius - 0.25f)) {
            float _angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return _angle < maxSlopeAngle && _angle != 0;
        }

        return false;
    }
}
