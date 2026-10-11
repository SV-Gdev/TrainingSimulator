using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour, IDataPersistence
{
    [System.Serializable]
    public class FrictionPoint
    {
        [Tooltip("Combined Dynamic Friction value")]
        public float friction;
        [Tooltip("Multiplier applied to base move speed")]
        public float speedMultiplier = 1f;
        [Tooltip("Drag added on top of the Rigidbody's normal drag")]
        public float extraDrag = 0f;
    }


    struct MatValues
    {
        public float dynamicFriction;
        public float staticFriction;
        public float bounciness;
        public PhysicMaterialCombine frictionCombine;
        public PhysicMaterialCombine bounceCombine;
    }

    public float baseMoveSpeed = 5f;
    [HideInInspector] public float currentMoveSpeed;
    public float jumpForce = 5f;

    public Transform GroundCheck;
    public float GroundDistace = 0.03f;
    public LayerMask groundMask;
    public LayerMask weaponCheckMask;

    [Header("Dynamic Friction -> speed, drag, sliding")]
    [Tooltip("Friction treated as 'normal ground' (Unity's default material is 0.6)")]
    public float referenceFriction = 0.6f;
    [Tooltip("At or below this combined dynamic friction the player keeps sliding when no keys are pressed")]
    public float slipperyThreshold = 0.2f;
    public float groundAcceleration = 6f;
    public float slipperyAcceleration = 4f;
    [Tooltip("Sorted automatically by friction. Values between points are interpolated.")]
    public List<FrictionPoint> frictionProfile = new List<FrictionPoint>
    {
        new FrictionPoint { friction = 0f,    speedMultiplier = 1.5f,   extraDrag = 0.3f }, 
        new FrictionPoint { friction = 0.6f, speedMultiplier = 1f,     extraDrag = 0f   }, 
        new FrictionPoint { friction = 1.2f, speedMultiplier = 0.75f, extraDrag = 1.5f }, 
        new FrictionPoint { friction = 2f,   speedMultiplier = 0.4f,   extraDrag = 4f   }, 
    };

    [Header("Static Friction -> getting started")]
    [Tooltip("Below this horizontal speed the player counts as 'standing still' and static friction applies")]
    public float startSpeedThreshold = 0.5f;
    public float minStartMultiplier = 0.25f;
    public float maxStartMultiplier = 1.5f;

    [Header("Bounciness -> jump boost")]
    [Tooltip("Jump force is multiplied by (1 + combined bounciness * this). Set 0 to disable.")]
    public float bounceJumpBoost = 1f;

    [Header("Surface Detection")]
    [Tooltip("How far below the feet the surface ray can reach.")]
    public float surfaceReach = 0.7f;
    public bool debugGround = true;

    private Rigidbody rb;
    private Collider bodyCollider;
    private float horizontalInput;
    private float verticalInput;
    private bool isGrounded;
    private bool isSlippery;
    private float startMultiplier = 1f;
    private float currentBounciness;
    private float defaultDrag;
    private float nextDebugTime;
    public Vector3 direction;

    private readonly Collider[] overlapBuffer = new Collider[16];
    private readonly RaycastHit[] rayBuffer = new RaycastHit[16];

    [SerializeField] Camera cam;
    [SerializeField] InventoryManager inventoryManager;
    [HideInInspector] public Animator anim;

    MovementBase currenttstate;

    public IdleState Idle = new IdleState();
    public WalkState Walk = new WalkState();

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        bodyCollider = GetComponent<Collider>();
        if (rb != null)
        {
            defaultDrag = rb.drag;
        }
        currentMoveSpeed = baseMoveSpeed;

        if (frictionProfile != null)
            frictionProfile.Sort((a, b) => a.friction.CompareTo(b.friction));
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
        if (data.playerPosition != Vector3.zero)
        {
            if (rb != null)
            {
                
                rb.isKinematic = true;
                transform.position = data.playerPosition;
                rb.position = data.playerPosition;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = false;
            }
            else
            {
                transform.position = data.playerPosition;
            }
            Debug.Log($"[PlayerMovement] Loaded position: {data.playerPosition}");
        }
    }

    public void SaveData(ref GameData data)
    {
        data.playerPosition = transform.position;
    }

    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        CheckGround();

        if (currenttstate != null) currenttstate.UpdateState(this);

        if (anim != null)
        {
            anim.SetFloat("vinput", verticalInput);
            anim.SetFloat("hrinput", horizontalInput);
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && rb != null)
        {
           
            float boost = 1f + currentBounciness * bounceJumpBoost;
            rb.AddForce(Vector3.up * jumpForce * boost, ForceMode.Impulse);
        }

        if (Input.GetKeyDown(KeyCode.E))
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


        Vector3 feet = GetFeetPosition();
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(feet, GroundDistace);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(feet + Vector3.up * 0.5f, feet + Vector3.down * surfaceReach);
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

    void MovePlayer()
    {
        if (rb == null) return;

        direction = transform.right * horizontalInput + transform.forward * verticalInput;
        direction.Normalize();

        if (Mathf.Abs(horizontalInput) <= 0.01f && Mathf.Abs(verticalInput) <= 0.01f)
        {
           
            if (!isSlippery)
            {
                rb.velocity = new Vector3(0, rb.velocity.y, 0);
            }
            return;
        }

        float accel = isSlippery ? slipperyAcceleration : groundAcceleration;
        float force = currentMoveSpeed * accel;

       
        Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        if (horizontalVelocity.magnitude < startSpeedThreshold)
        {
            force *= startMultiplier;
        }

        rb.AddForce(direction * force, ForceMode.Force);

        horizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        if (horizontalVelocity.magnitude > currentMoveSpeed)
        {
            Vector3 cappedVelocity = horizontalVelocity.normalized * currentMoveSpeed;
            rb.velocity = new Vector3(cappedVelocity.x, rb.velocity.y, cappedVelocity.z);
        }
    }

    Vector3 GetFeetPosition()
    {
        Collider c = bodyCollider != null ? bodyCollider : GetComponent<Collider>();
        if (c != null)
        {
            Bounds b = c.bounds;
            return new Vector3(b.center.x, b.min.y, b.center.z);
        }
        return GroundCheck != null ? GroundCheck.position : transform.position;
    }

    bool IsSelf(Collider col)
    {
        return col != null && col.transform.IsChildOf(transform);
    }

    static MatValues ReadMaterial(PhysicMaterial m)
    {
        MatValues v;
        if (m == null)
        {
            v.dynamicFriction = 0.6f;
            v.staticFriction = 0.6f;
            v.bounciness = 0f;
            v.frictionCombine = PhysicMaterialCombine.Average;
            v.bounceCombine = PhysicMaterialCombine.Average;
        }
        else
        {
            v.dynamicFriction = m.dynamicFriction;
            v.staticFriction = m.staticFriction;
            v.bounciness = m.bounciness;
            v.frictionCombine = m.frictionCombine;
            v.bounceCombine = m.bounceCombine;
        }
        return v;
    }

    static int Priority(PhysicMaterialCombine mode)
    {
        switch (mode)
        {
            case PhysicMaterialCombine.Average: return 0;
            case PhysicMaterialCombine.Minimum: return 1;
            case PhysicMaterialCombine.Multiply: return 2;
            default: return 3; 
        }
    }

    static PhysicMaterialCombine EffectiveMode(PhysicMaterialCombine a, PhysicMaterialCombine b)
    {
        return Priority(a) >= Priority(b) ? a : b;
    }

    static float Combine(float a, float b, PhysicMaterialCombine mode)
    {
        switch (mode)
        {
            case PhysicMaterialCombine.Minimum: return Mathf.Min(a, b);
            case PhysicMaterialCombine.Multiply: return a * b;
            case PhysicMaterialCombine.Maximum: return Mathf.Max(a, b);
            default: return (a + b) * 0.5f; 
        }
    }

    void CheckGround()
    {
        Vector3 feet = GetFeetPosition();

        isGrounded = false;
        int overlaps = Physics.OverlapSphereNonAlloc(feet, GroundDistace, overlapBuffer, groundMask, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < overlaps; i++)
        {
            if (!IsSelf(overlapBuffer[i])) { isGrounded = true; break; }
        }

        Collider surface = FindSurfaceBelow(feet);

        float speedMultiplier = 1f;
        float extraDrag = 0f;
        float combinedDynamic = referenceFriction;
        float combinedStatic = referenceFriction;
        float combinedBounce = 0f;
        PhysicMaterialCombine fMode = PhysicMaterialCombine.Average;
        PhysicMaterialCombine bMode = PhysicMaterialCombine.Average;
        MatValues ground = ReadMaterial(null);

        if (surface != null)
        {
            ground = ReadMaterial(surface.sharedMaterial);
            MatValues body = ReadMaterial(bodyCollider != null ? bodyCollider.sharedMaterial : null);

            fMode = EffectiveMode(ground.frictionCombine, body.frictionCombine);
            bMode = EffectiveMode(ground.bounceCombine, body.bounceCombine);
            combinedDynamic = Combine(ground.dynamicFriction, body.dynamicFriction, fMode);
            combinedStatic = Combine(ground.staticFriction, body.staticFriction, fMode);
            combinedBounce = Mathf.Clamp01(Combine(ground.bounciness, body.bounciness, bMode));

            EvaluateProfile(combinedDynamic, out speedMultiplier, out extraDrag);
            startMultiplier = Mathf.Clamp(referenceFriction / Mathf.Max(combinedStatic, 0.05f), minStartMultiplier, maxStartMultiplier);
        }
        else
        {
            startMultiplier = 1f;
        }

        isSlippery = surface != null && combinedDynamic <= slipperyThreshold;
        currentBounciness = combinedBounce;
        currentMoveSpeed = baseMoveSpeed * speedMultiplier;
        if (rb != null) rb.drag = defaultDrag + extraDrag;

        if (debugGround && Time.time >= nextDebugTime)
        {
            nextDebugTime = Time.time + 0.5f;
            string matName = (surface != null && surface.sharedMaterial != null) ? surface.sharedMaterial.name : "none (Unity default)";
            
        }
    }

    void EvaluateProfile(float friction, out float speedMultiplier, out float extraDrag)
    {
        speedMultiplier = 1f;
        extraDrag = 0f;

        if (frictionProfile == null || frictionProfile.Count == 0) return;

        FrictionPoint first = frictionProfile[0];
        if (friction <= first.friction)
        {
            speedMultiplier = first.speedMultiplier;
            extraDrag = first.extraDrag;
            return;
        }

        for (int i = 1; i < frictionProfile.Count; i++)
        {
            FrictionPoint a = frictionProfile[i - 1];
            FrictionPoint b = frictionProfile[i];
            if (friction <= b.friction)
            {
                float t = Mathf.InverseLerp(a.friction, b.friction, friction);
                speedMultiplier = Mathf.Lerp(a.speedMultiplier, b.speedMultiplier, t);
                extraDrag = Mathf.Lerp(a.extraDrag, b.extraDrag, t);
                return;
            }
        }

        FrictionPoint last = frictionProfile[frictionProfile.Count - 1];
        speedMultiplier = last.speedMultiplier;
        extraDrag = last.extraDrag;
    }

    Collider FindSurfaceBelow(Vector3 feet)
    {
        Vector3 origin = feet + Vector3.up * 0.5f;
        float length = 0.5f + surfaceReach;

        int hits = Physics.RaycastNonAlloc(origin, Vector3.down, rayBuffer, length, groundMask, QueryTriggerInteraction.Ignore);

        Collider best = null;
        float bestDist = float.MaxValue;
        for (int i = 0; i < hits; i++)
        {
            if (IsSelf(rayBuffer[i].collider)) continue;
            if (rayBuffer[i].distance < bestDist)
            {
                bestDist = rayBuffer[i].distance;
                best = rayBuffer[i].collider;
            }
        }
        return best;
    }
}