using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class PlayerVR : MonoBehaviour
{
    private World world;

    // Movement variables
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public bool isGrounded;
    public bool isRunning;
    public float jumpForce = 5f;
    private bool jumpRequest;
    private float verticalMomentum = 0f;
    public float gravity = -9.81f;
    private Vector3 velocity;

    public float playerWidth = 0.5f;
    public float playerHeight = 1.8f;

    // Input variables
    XRIDefaultInputActions inputActions;
    private float horizontal;
    private float vertical;
    private float mouseX;
    public float mouseY;

    public Transform highlightBlock;
    public Transform placeBlock;
    public float checkIncrement = 0.1f;
    public float reach = 8f;

    public Transform rHand;
    public Transform lHand;

    public byte selectedBlockIndex = 1;

    void Awake()
    {
        inputActions = new XRIDefaultInputActions();

        inputActions.XRILeftHandLocomotion.Move.performed += ctx =>
        {
            horizontal = ctx.ReadValue<Vector2>().x;
            vertical = ctx.ReadValue<Vector2>().y;
        };
        inputActions.XRILeftHandLocomotion.Move.canceled += ctx =>
        {
            horizontal = 0f;
            vertical = 0f;
        };
        inputActions.XRIRightHandLocomotion.Move.performed += ctx => mouseX = ctx.ReadValue<Vector2>().x;
        inputActions.XRIRightHandLocomotion.Move.canceled += ctx => mouseX = 0f;
        inputActions.XRIRightHandLocomotion.Move.performed += ctx => mouseY = ctx.ReadValue<Vector2>().y;
        inputActions.XRIRightHandLocomotion.Move.canceled += ctx => mouseY = 0;
        inputActions.XRIRightHandInteraction.Jump.performed += ctx => 
        {
            if (isGrounded)
                jumpRequest = true;
        };
        inputActions.XRILeftHandInteraction.Run.started += ctx => isRunning = true;
        inputActions.XRILeftHandInteraction.Run.canceled += ctx => isRunning = false;
        inputActions.XRIRightHandInteraction.DebugScreen.started += ctx => world.TriggerDebugScreen();

        inputActions.XRIRightHandInteraction.Activate.performed += ctx =>
        {
            if (highlightBlock.gameObject.activeSelf)
            {
                world.GetChunkFromVector3(highlightBlock.position).EditVoxel(highlightBlock.position, 0);
            }
        };
        inputActions.XRIRightHandInteraction.Select.performed += ctx =>
        {
            if (highlightBlock.gameObject.activeSelf)
            {
                world.GetChunkFromVector3(placeBlock.position).EditVoxel(placeBlock.position, selectedBlockIndex);
            }
        };
    }

    // Start is called before the first frame update
    void Start()
    {
        world = GameObject.Find("World").GetComponent<World>();

    }

    void Update()
    {
        PlaceCursorBlocks();

        // Solution with Raycast
        /* if (highlightBlock.gameObject.activeSelf)
        {
            // Destroy block
            if (inputActions.XRIRightHandInteraction.Activate.triggered)
                world.GetChunkFromVector3(highlightBlock.position).EditVoxel(highlightBlock.position, 0);
            // Place block
            if (inputActions.XRIRightHandInteraction.Select.triggered)
                world.GetChunkFromVector3(placeBlock.position).EditVoxel(placeBlock.position, selectedBlockIndex);
        } */
    }

    void FixedUpdate()
    {
        CalculateVelocity();
        if (jumpRequest)
            Jump();

        transform.Rotate(Vector3.up * mouseX);
        transform.Translate(velocity, Space.World);
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

        if ((velocity.z > 0 && front) || (velocity.z < 0 && back))
            velocity.z = 0;
        if ((velocity.x > 0 && right) || (velocity.x < 0 && left))
            velocity.x = 0;
        if (velocity.y < 0)
            velocity.y = checkDownSpeed(velocity.y);
        else if (velocity.y > 0)
            velocity.y = checkUpSpeed(velocity.y);
    }

    void Jump()
    {
        verticalMomentum = jumpForce;
        isGrounded = false;
        jumpRequest = false;
    }

    private void PlaceCursorBlocks()
    {
        // Solution with Raycast
        /* RaycastHit hit;

        if (Physics.Raycast(rHand.position, rHand.forward, out hit, reach))
        {
            if (world.CheckForVoxel(hit.point))
            {
                highlightBlock.position = new Vector3(Mathf.FloorToInt(hit.point.x), Mathf.FloorToInt(hit.point.y), Mathf.FloorToInt(hit.point.z));
                placeBlock.position = new Vector3(Mathf.FloorToInt(hit.point.x), Mathf.FloorToInt(hit.point.y), Mathf.FloorToInt(hit.point.z));

                highlightBlock.gameObject.SetActive(true);
                placeBlock.gameObject.SetActive(true);
                return;
            }
        }
        highlightBlock.gameObject.SetActive(false);
        placeBlock.gameObject.SetActive(false); */

        // No Raycast
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
            isGrounded = true;
            return 0;
        }
        else
        {
            isGrounded = false;
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

    void OnEnable() => inputActions.Enable();
    void OnDisable() => inputActions.Disable();
}
