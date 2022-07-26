using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviour
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
    private RaycastHit slopeHit;
    private Vector3 slopeMoveDirection;

    public bool canSprint { get; set; }
    private bool isSprinting;
    private bool isGrounded;

    [SerializeField] public Transform playerModel { get; private set; }
    private Transform orientationTransform;
    private Transform groundDetectTransform;
    private Rigidbody playerRigid;

    private PhotonView pv;

    private void Awake() 
    {
        pv = GetComponent<PhotonView>();
    }

    private void Start() 
    {
        if(!pv.IsMine) { return; }

        playerRigid = GetComponent<Rigidbody>();
        
        orientationTransform = transform.root.Find("PlayerOrientation").transform;
        groundDetectTransform = transform.root.Find("PlayerModel/GroundDetect").transform;
    }

    private void Update() 
    {
        if(!pv.IsMine) { return; }

        currentSpeed = defaultMoveSpeed;

        horizontalMovement = Input.GetAxisRaw("Horizontal");
        verticalMovement = Input.GetAxisRaw("Vertical"); 
        
        ReadInput();
        IsGrounded();
        HandleDrag();
        SlopeMovement();
    }

    private void FixedUpdate() 
    {
        if(!pv.IsMine) { return; }

        ApplyMovement();
    }

    private void ReadInput() 
    {
        inputDirection = orientationTransform.forward * verticalMovement + orientationTransform.right * horizontalMovement;

        if (Input.GetButton("Sprint") && verticalMovement > 0)
            isSprinting = true;
        else if (Input.GetButtonUp("Sprint") || horizontalMovement != 0 || verticalMovement <= 0)
            isSprinting = false;

        if (Input.GetButtonDown("Jump"))
            AddJumpForce();
    }

    private void ApplyMovement() 
    {
        if (isGrounded && !OnSlope())
            playerRigid.AddForce(inputDirection.normalized * currentSpeed * (canSprint && isSprinting ? sprintMoveSpeedModifier : walkMoveSpeedModifier), ForceMode.Acceleration);
        
        else if (isGrounded && OnSlope())
            playerRigid.AddForce(inputDirection.normalized * currentSpeed * (canSprint && isSprinting ? sprintMoveSpeedModifier : walkMoveSpeedModifier), ForceMode.Acceleration);
        
        else if (!isGrounded)
            playerRigid.AddForce(inputDirection.normalized * currentSpeed * (walkMoveSpeedModifier * airMoveSpeedModifier));
    }

    public float GetPlayerSpeed() { return currentSpeed; }
    public void ModifyPlayerSpeed(float speedModifier) => currentSpeed *= speedModifier;

    private void AddJumpForce() 
    {
        if (isGrounded)
            playerRigid.velocity = new Vector3(playerRigid.velocity.x, jumpForceStrength, playerRigid.velocity.z);
    }

    private void HandleDrag() 
    {
            playerRigid.drag = isGrounded ? groundDrag : airDrag;
    }

    private void IsGrounded() 
    {
        isGrounded = Physics.CheckSphere(groundDetectTransform.position, groundDetectRadius, groundLayerMask);
    }

    private void SlopeMovement() 
    {
        slopeMoveDirection = Vector3.ProjectOnPlane(inputDirection.normalized, slopeHit.normal);
    }

    private bool OnSlope() 
    {
        Debug.DrawRay(transform.position, Vector3.down);
        if (Physics.Raycast(groundDetectTransform.position, Vector3.down, out slopeHit, groundDetectRadius + 1)) {
            
            if (slopeHit.normal != Vector3.up) 
                return true;
            else
                return false;
        }
        return false;
    }
}
