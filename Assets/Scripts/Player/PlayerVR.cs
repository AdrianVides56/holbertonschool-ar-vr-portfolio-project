using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit;

public class PlayerVR : MonoBehaviour
{
    private World world;
    public XRIDefaultInputActions inputActions;

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
    [SerializeField] private GameObject locomotionSystem;

    private XROrigin _xrOrigin;
    private CapsuleCollider _capsuleCollider;

    [Header("Joysticks")]
    private float _horizontal;
    private float _vertical;
    private float _turn;
    public float _scrollToolBar;

    [Header("Block Placement")]
    //public byte selectedBlockIndex = 1;
    public Transform highlightBlock;
    public Transform placeBlock;
    public float checkIncrement = 0.1f;
    public float reach = 8f;

    public Toolbar toolbar;

    [Header("")]
    public Transform rHand;
    public Transform lHand;
    public GameObject debugScreen;
    public GameObject creativeInventoryWindow;
    public GameObject cursorSlot;

    [Header("My Actions")]
    [SerializeField] private InputActionReference moveActionReference;
    [SerializeField] private InputActionReference turnActionReference;
    [SerializeField] private InputActionReference scrollActionReference;
    [SerializeField] private InputActionReference jumpActionReference;
    [SerializeField] private InputActionReference runActionReference;
    [SerializeField] private InputActionReference debugScreenActionReference;
    [SerializeField] private InputActionReference removeBlockActionReference;
    [SerializeField] private InputActionReference placeBlockActionReference;
    [SerializeField] private InputActionReference menuButton;

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    private void Awake()
    {
        inputActions = new XRIDefaultInputActions();
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

        runActionReference.action.started += ctx => locomotionSystem.GetComponent<ContinuousMoveProviderBase>().moveSpeed = runSpeed;
        runActionReference.action.canceled += ctx => locomotionSystem.GetComponent<ContinuousMoveProviderBase>().moveSpeed = walkSpeed;

        turnActionReference.action.performed += ctx => _turn = ctx.ReadValue<float>();
        turnActionReference.action.canceled += ctx => _turn = 0f;

        scrollActionReference.action.performed += ctx => _scrollToolBar = ctx.ReadValue<float>();
        scrollActionReference.action.canceled += ctx => _scrollToolBar = 0;

        debugScreenActionReference.action.started += ctx => TriggerDebugScreen();

        jumpActionReference.action.performed += OnJump;

        removeBlockActionReference.action.performed += OnRemoveBlock;
        placeBlockActionReference.action.performed += OnPlaceBlock;

        menuButton.action.performed += ctx =>
        {
            world.inUI = !world.inUI;

            if (world.inUI)
            {
                moveActionReference.action.Disable();
                turnActionReference.action.Disable();
                scrollActionReference.action.Disable();
                jumpActionReference.action.Disable();
                runActionReference.action.Disable();
                debugScreenActionReference.action.Disable();
                removeBlockActionReference.action.Disable();
                placeBlockActionReference.action.Disable();
                locomotionSystem.GetComponent<ContinuousMoveProviderBase>().enabled = false;
                locomotionSystem.GetComponent<SnapTurnProviderBase>().enabled = false;
                creativeInventoryWindow.SetActive(true);
                cursorSlot.SetActive(true);
            }
            else
            {
                moveActionReference.action.Enable();
                turnActionReference.action.Enable();
                scrollActionReference.action.Enable();
                jumpActionReference.action.Enable();
                runActionReference.action.Enable();
                debugScreenActionReference.action.Enable();
                removeBlockActionReference.action.Enable();
                placeBlockActionReference.action.Enable();
                locomotionSystem.GetComponent<ContinuousMoveProviderBase>().enabled = true;
                locomotionSystem.GetComponent<SnapTurnProviderBase>().enabled = true;
                creativeInventoryWindow.SetActive(false);
                cursorSlot.SetActive(false);
            }
        };
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
        if (!world.inUI)
        {
            PlaceCursorBlocks();

            // move the player with the headset
            var center = _xrOrigin.CameraInOriginSpacePos;
            _capsuleCollider.center = new Vector3(center.x, _capsuleCollider.height / 2, center.z);
            _capsuleCollider.height = Mathf.Clamp(_xrOrigin.CameraInOriginSpaceHeight, 1.4f, 1.8f);
        }
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
        {
            if (toolbar.slots[toolbar.slotIndex].HasItem)
            {
                world.GetChunkFromVector3(placeBlock.position).EditVoxel(placeBlock.position, toolbar.slots[toolbar.slotIndex].itemSlot.stack.id);
                toolbar.slots[toolbar.slotIndex].itemSlot.Take(1);
            }
        }
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
