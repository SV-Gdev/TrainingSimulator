using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour, IDataPersistence
{
    public float moveSpeed = 5f;
    public float jumpForce = 5f;

    public Transform GroundCheck;
    public float GroundDistace = 0.03f;
    public LayerMask groundMask;
    public LayerMask weaponCheckMask;

    private Rigidbody rb;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isGrounded;
    private bool isOnIce;
    private float defaultDrag; // <-- NEW: Remembers your normal drag
    public Vector3 direction;

    [SerializeField] Camera cam;
    [SerializeField] InventoryManager inventoryManager;
    [HideInInspector] public Animator anim;

    float rotationX = 0;
    MovementBase currenttstate;

    public IdleState Idle = new IdleState();
    public WalkState Walk = new WalkState();

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            defaultDrag = rb.drag; // <-- NEW: Save drag on awake
        }
    }

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        SwitchState(Idle);
    }

    private void OnEnable()
    {
        if (DataPersistenceManager.Instance != null)
            DataPersistenceManager.Instance.RegisterDataPersistence(this);
    }

    private void OnDisable()
    {
        if (DataPersistenceManager.Instance != null)
            DataPersistenceManager.Instance.UnregisterDataPersistence(this);
    }

    public void LoadData(GameData data)
    {
        transform.position = data.playerPosition;
        if (rb != null)
        {
            rb.position = data.playerPosition;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    public void SaveData(ref GameData data)
    {
        data.playerPosition = transform.position;
    }

    void Update()
    {
        CheckGround();

        if (currenttstate != null) currenttstate.UpdateState(this);

        if (anim != null)
        {
            anim.SetFloat("vinput", moveInput.y);
            anim.SetFloat("hrinput", moveInput.x);
        }

        if (UnityEngine.Input.GetKey(KeyCode.E))
        {
            Camera targetCamera = cam != null ? cam : Camera.main;
            if (targetCamera != null)
            {
                Ray ray = targetCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
                RaycastHit hitinfo;

                if (Physics.Raycast(ray, out hitinfo, 30, weaponCheckMask, QueryTriggerInteraction.Collide))
                {
                    ItemPickable item = hitinfo.collider.gameObject.GetComponent<ItemPickable>();
                    if (item != null)
                    {
                        if (inventoryManager != null) inventoryManager.ItemPicked(hitinfo.collider.gameObject);
                        else Debug.LogWarning("InventoryManager is missing!");
                    }
                }
            }
        }
    }

    public void OnDrawGizmos()
    {
        Camera targetCamera = cam != null ? cam : Camera.main;
        if (targetCamera != null)
        {
            Debug.DrawRay(targetCamera.transform.position, targetCamera.transform.forward * 30, Color.red);
        }
    }

    public void SwitchState(MovementBase state)
    {
        currenttstate = state;
        currenttstate.EnterState(this);
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    void OnMovement(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void MovePlayer()
    {
        if (rb == null) return;

        direction = transform.right * moveInput.x + transform.forward * moveInput.y;
        direction.Normalize();

        if (moveInput.magnitude <= 0.01f)
        {
            if (!isOnIce)
            {
                // Normal ground: stop immediately
                rb.velocity = new Vector3(0, rb.velocity.y, 0);
            }
            return;
        }

        if (isOnIce)
        {
            // ICE MOVEMENT
            rb.AddForce(direction * moveSpeed * 3f, ForceMode.Force);

            // Give the player the ability to steer (bend the velocity vector) while sliding
            if (Mathf.Abs(moveInput.x) > 0.1f)
            {
                Vector3 currentVel = rb.velocity;
                Vector3 newDir = Vector3.Lerp(currentVel.normalized, direction, 0.02f);
                rb.velocity = new Vector3(newDir.x * currentVel.magnitude, rb.velocity.y, newDir.z * currentVel.magnitude);
            }

            // Gentle speed cap so steering isn't deleted
            Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            if (horizontalVelocity.magnitude > moveSpeed * 1.5f)
            {
                Vector3 cappedVelocity = horizontalVelocity.normalized * (moveSpeed * 1.5f);
                rb.velocity = new Vector3(cappedVelocity.x, rb.velocity.y, cappedVelocity.z);
            }
        }
        else
        {
            // NORMAL MOVEMENT
            rb.AddForce(direction * moveSpeed * 6f, ForceMode.Force);
            Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            if (horizontalVelocity.magnitude > moveSpeed)
            {
                Vector3 cappedVelocity = horizontalVelocity.normalized * moveSpeed;
                rb.velocity = new Vector3(cappedVelocity.x, rb.velocity.y, cappedVelocity.z);
            }
        }
    }

    void OnJump()
    {
        if (isGrounded && rb != null)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void CheckGround()
    {
        if (GroundCheck != null)
        {
            isGrounded = Physics.CheckSphere(GroundCheck.position, GroundDistace, groundMask);

            isOnIce = false;
            if (isGrounded)
            {
                RaycastHit hit;
                if (Physics.Raycast(GroundCheck.position + Vector3.up * 0.1f, Vector3.down, out hit, 0.4f, groundMask))
                {
                    // =========================================================
                    // UNCOMMENT THE LINE BELOW TO SEE WHAT MATERIAL YOU ARE HITTING!
                    // Debug.Log($"Standing on: {hit.collider.name} | Material: {(hit.collider.sharedMaterial != null ? hit.collider.sharedMaterial.name : "NONE")}");
                    // =========================================================

                    if (hit.collider.sharedMaterial != null && hit.collider.sharedMaterial.name.Contains("Ice"))
                    {
                        isOnIce = true;
                    }
                }
            }

            // <-- NEW: Manage drag dynamically
            if (rb != null)
            {
                if (isOnIce) rb.drag = 0f;          // Remove air resistance to slide forever
                else rb.drag = defaultDrag;         // Put normal drag back on grass/dirt
            }
        }
    }
}