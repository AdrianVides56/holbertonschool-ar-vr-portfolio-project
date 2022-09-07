using UnityEngine;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit;

public class PlayerVR : MonoBehaviour
{
    private World world;

    [Header("Movement")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public bool isRunning;
    public bool isGrounded => Physics.OverlapSphere(groundCheckPoint.position, .25f, groundLayer).Length > 0;
    public float jumpForce = 5f;
    private Rigidbody _body;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private ContinuousMoveProviderBase continuousMoveProvider;

    private XROrigin _xrOrigin;
    private CapsuleCollider _capsuleCollider;

    [Header("Joysticks")]
    private float _horizontal;
    private float _vertical;
    private float _turn;
    public float _scrollToolBar;

    [Header("Block Placement")]
    public byte selectedBlockIndex = 1;
    public Transform highlightBlock;
    public Transform placeBlock;
    public float checkIncrement = 0.1f;
    public float reach = 8f;

    [Header("")]
    public Transform rHand;
    public Transform lHand;
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

    private void Awake()
    {
        // Movement - Left Controller
        moveActionReference.action.performed += ctx =>
        {
            _horizontal = ctx.ReadValue<Vector2>().x;
            _vertical = ctx.ReadValue<Vector2>().y;
        };
        moveActionReference.action.canceled += ctx =>
        {
            _horizontal = 0f;
            _vertical = 0f;
        };

        runActionReference.action.started += ctx => continuousMoveProvider.moveSpeed = runSpeed;
        runActionReference.action.canceled += ctx => continuousMoveProvider.moveSpeed = walkSpeed;

        turnActionReference.action.performed += ctx => _turn = ctx.ReadValue<float>();
        turnActionReference.action.canceled += ctx => _turn = 0f;

        scrollActionReference.action.performed += ctx => _scrollToolBar = ctx.ReadValue<float>();
        scrollActionReference.action.canceled += ctx => _scrollToolBar = 0;

        debugScreenActionReference.action.started += ctx => TriggerDebugScreen();

        jumpActionReference.action.performed += OnJump;

        removeBlockActionReference.action.performed += OnRemoveBlock;
        placeBlockActionReference.action.performed += OnPlaceBlock;
    }

    private void Start()
    {
        world = GameObject.Find("World").GetComponent<World>();
        _body = GetComponent<Rigidbody>();
        _xrOrigin = GetComponent<XROrigin>();
        _capsuleCollider = GetComponent<CapsuleCollider>();
    }

    private void Update()
    {
        PlaceCursorBlocks();

        var center = _xrOrigin.CameraInOriginSpacePos;
        _capsuleCollider.center = new Vector3(center.x, _capsuleCollider.height / 2, center.z);
        _capsuleCollider.height = Mathf.Clamp(_xrOrigin.CameraInOriginSpaceHeight, 1.4f, 1.8f);
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (!isGrounded)
            return;
        _body.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void OnRemoveBlock(InputAction.CallbackContext ctx)
    {
        if (highlightBlock.gameObject.activeSelf)
            world.GetChunkFromVector3(highlightBlock.position).EditVoxel(highlightBlock.position, 0);
    }

    private void OnPlaceBlock(InputAction.CallbackContext ctx)
    {
        if (highlightBlock.gameObject.activeSelf)
            world.GetChunkFromVector3(placeBlock.position).EditVoxel(placeBlock.position, selectedBlockIndex);
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

    public void TriggerDebugScreen() => debugScreen.SetActive(!debugScreen.activeSelf);
}
