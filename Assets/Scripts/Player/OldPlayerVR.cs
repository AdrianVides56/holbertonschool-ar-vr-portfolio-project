using UnityEngine;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;

public class OldPlayerVR : MonoBehaviour
{
    private World world;

    [Header("Movement")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public bool isGrounded => Physics.OverlapSphere(groundCheckPoint.position, .25f, groundLayer).Length > 0;
    public bool isRunning;
    public float jumpForce = 5f;
    private bool jumpRequest;
    private float verticalMomentum = 0f;
    public float gravity = -9.81f;
    private Vector3 velocity;
    private Rigidbody _body;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheckPoint;
    private XROrigin _xrOrigin;
    private CapsuleCollider _capsuleCollider;

    private float horizontal;
    private float vertical;
    private float turn;
    public float scrollToolBar;

    [Header("")]
    public byte selectedBlockIndex = 1;
    public Transform highlightBlock;
    public Transform placeBlock;
    public float checkIncrement = 0.1f;
    public float reach = 8f;

    public Transform rHand;
    public Transform lHand;

    public float playerWidth = 0.5f;
    public float playerHeight = 1.8f;

    public GameObject debugScreen;

    [Header("My Actions")]
    [SerializeField] private InputActionReference moveActionReference;
    [SerializeField] private InputActionReference turnActionReference;
    [SerializeField] private InputActionReference scrollActionReference;
    [SerializeField] private InputActionReference jumpActionReference;
    [SerializeField] private InputActionReference runActionReference;
    [SerializeField] private InputActionReference debugScreenActionReference;
    [SerializeField] private InputActionReference removeBlockActionReference;
    [SerializeField] private InputActionReference placeBlockActionReference;

    void Awake()
    {
        // Movement - Left Controller
        moveActionReference.action.performed += ctx =>
        {
            horizontal = ctx.ReadValue<Vector2>().x;
            vertical = ctx.ReadValue<Vector2>().y;
        };
        moveActionReference.action.canceled += ctx =>
        {
            horizontal = 0f;
            vertical = 0f;
        };
        runActionReference.action.started += ctx => isRunning = true;
        runActionReference.action.canceled += ctx => isRunning = false;

        // Look - Right Controller
        turnActionReference.action.performed += ctx => turn = ctx.ReadValue<float>();
        turnActionReference.action.canceled += ctx => turn = 0f;

        // Scroll - Right Controller
        scrollActionReference.action.performed += ctx => scrollToolBar = ctx.ReadValue<float>();
        scrollActionReference.action.canceled += ctx => scrollToolBar = 0;

        jumpActionReference.action.performed += ctx =>
        {
            if (isGrounded)
                jumpRequest = true;
        };

        debugScreenActionReference.action.started += ctx => TriggerDebugScreen();

        removeBlockActionReference.action.performed += ctx =>
        {
            if (highlightBlock.gameObject.activeSelf)
            {
                world.GetChunkFromVector3(highlightBlock.position).EditVoxel(highlightBlock.position, 0);
            }
        };
        placeBlockActionReference.action.performed += ctx =>
        {
            if (highlightBlock.gameObject.activeSelf)
            {
                world.GetChunkFromVector3(placeBlock.position).EditVoxel(placeBlock.position, selectedBlockIndex);
            }
        };
    }

    void Start()
    {
        world = GameObject.Find("World").GetComponent<World>();
        _body = GetComponent<Rigidbody>();
        _xrOrigin = GetComponent<XROrigin>();
        _capsuleCollider = GetComponent<CapsuleCollider>();
    }

    void Update()
    {
        PlaceCursorBlocks();

        var center = _xrOrigin.CameraInOriginSpacePos;
        _capsuleCollider.center = new Vector3(center.x, _capsuleCollider.height / 2, center.z);
        _capsuleCollider.height = Mathf.Clamp(_xrOrigin.CameraInOriginSpaceHeight, 1.4f, 1.8f);
    }

    void FixedUpdate()
    {
        if (jumpRequest)
            OnJump();
    }

    private void CalculateVelocity()
    {
        // Affect vertical momentum with gravity
        if (verticalMomentum > gravity)
            verticalMomentum += gravity * Time.fixedDeltaTime;
        else
            verticalMomentum = gravity;
        // Run multiplier
        if (isRunning)
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * runSpeed;
        else
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * walkSpeed;

        velocity += Vector3.up * verticalMomentum * Time.fixedDeltaTime;

        /* if ((velocity.z > 0 && front) || (velocity.z < 0 && back))
            velocity.z = 0;
        if ((velocity.x > 0 && right) || (velocity.x < 0 && left))
            velocity.x = 0;
        if (velocity.y < 0)
            velocity.y = checkDownSpeed(velocity.y);
        else if (velocity.y > 0)
            velocity.y = checkUpSpeed(velocity.y); */
    }

    void OnJump()
    {
        if (isGrounded)
            _body.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        jumpRequest = false;
    }
    void Jump()
    {
        verticalMomentum = jumpForce;
        jumpRequest = false;
    }

    private void PlaceCursorBlocks()
    {
        float step = checkIncrement;
        Vector3 lastPos = new Vector3();

        while (step < reach) {

            Vector3 pos = rHand.position + (rHand.forward * step);

            if (world.CheckForVoxel(pos)) {

                highlightBlock.position = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
                placeBlock.position = lastPos;

                highlightBlock.gameObject.SetActive(true);
                placeBlock.gameObject.SetActive(true);

                return;

            }

            lastPos = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));

            step += checkIncrement;

        }

        highlightBlock.gameObject.SetActive(false);
        placeBlock.gameObject.SetActive(false);
    }

    private float checkDownSpeed(float downSpeed)
    {
        if (
            (world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth)) && (!left && !back))  ||
            (world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth)) && (!right && !back)) ||
            (world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth)) && (!left && !front)) ||
            (world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth)) && (!right && !front))
           )
        {
            //isGrounded = true;
            return 0;
        }
        else
        {
            //isGrounded = false;
            return downSpeed;
        }

    }

    private float checkUpSpeed(float upSpeed)
    {
        if (
            (world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + playerHeight + upSpeed, transform.position.z - playerWidth)) && (!left && !back)) ||
            (world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + playerHeight + upSpeed, transform.position.z - playerWidth)) && (!right && !back)) ||
            (world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + playerHeight + upSpeed, transform.position.z + playerWidth)) && (!left && !front)) ||
            (world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + playerHeight + upSpeed, transform.position.z + playerWidth)) && (!right && !front))
            )
            {
                verticalMomentum = 0;
                return 0;
            }
        else
            return upSpeed;

    }

    public bool front
    {
        get
        {
            if (world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z + playerWidth)) ||
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z + playerWidth)))
                return true;
            else
                return false;
        }
    }

    public bool back
    {
        get
        {
            if (world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z - playerWidth)) ||
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z - playerWidth)))
                return true;
            else
                return false;
        }
    }

    public bool left
    {
        get
        {
            if (world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y, transform.position.z)) ||
                world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + 1f, transform.position.z)))
                return true;
            else
                return false;
        }
    }

    public bool right
    {
        get
        {
            if (world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y, transform.position.z)) ||
                world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + 1f, transform.position.z)))
                return true;
            else
                return false;
        }
    }

    public void TriggerDebugScreen() => debugScreen.SetActive(!debugScreen.activeSelf);
}
