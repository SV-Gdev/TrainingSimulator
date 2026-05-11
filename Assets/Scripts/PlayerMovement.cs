using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class PlayerMovement : MonoBehaviour
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
    public Vector3 direction;

    [SerializeField] Camera cam;
    [SerializeField] InventoryManager inventoryManager;


    // public Camera playerCamera;

    //public float lookSpeed = 2f;
    // public float lookXLimit = 45f;

    [HideInInspector] public Animator anim;

    float rotationX = 0;

    MovementBase currenttstate;

    public IdleState Idle = new IdleState();
    public WalkState Walk = new WalkState();

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();

       // Cursor.lockState = CursorLockMode.Locked;
       // Cursor.visible = false;

        SwitchState(Idle);
    }

    void Update()
    {
        CheckGround();
        //HandleLook(); 

        currenttstate.UpdateState(this);

        anim.SetFloat("vinput", moveInput.y);
        anim.SetFloat("hrinput", moveInput.x);

        if (UnityEngine.Input.GetKey(KeyCode.E))
        {

            //Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width/2,Screen.height/2, 0));
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

            RaycastHit hitinfo;
            

            if (Physics.Raycast(ray, out hitinfo, 30, weaponCheckMask,  QueryTriggerInteraction.Collide))
            { 
                
                ItemPickable item = hitinfo.collider.gameObject.GetComponent<ItemPickable>();
                

                if (item != null)
                {
                    
                    inventoryManager.ItemPicked(hitinfo.collider.gameObject);
                }
            }

        }

    }
    public void OnDrawGizmos()
    {

        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * 30, Color.red);
    }

    public void SwitchState(MovementBase state)
    {
        currenttstate = state;
        currenttstate.EnterState(this);
        Debug.Log("Moveed");
        
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    
    void OnMovement(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

   
   // void OnLook(InputValue value)
   // {
   //   //  lookInput = value.Get<Vector2>();
   // }

    void MovePlayer()
    {
        
        direction = transform.right * moveInput.x + transform.forward * moveInput.y;
        direction.Normalize();

        rb.velocity = new Vector3(direction.x * moveSpeed, rb.velocity.y, direction.z * moveSpeed);
    }

    void OnJump()
    {
        if (isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void CheckGround()
    {
        isGrounded = Physics.CheckSphere(GroundCheck.position, GroundDistace, groundMask);
    }

  
    //void HandleLook()
    //{
       // float mouseX = lookInput.x * lookSpeed;
       // float mouseY = lookInput.y * lookSpeed;

       // rotationX -= mouseY;
       // rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);

        
        //playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);

        
       // transform.Rotate(Vector3.up * mouseX);
    }
