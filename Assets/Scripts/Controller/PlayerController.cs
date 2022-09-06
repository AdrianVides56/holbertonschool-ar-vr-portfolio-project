using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public InputActionReference jumpActionReference;
    public float jumpForce = 50f;

    private Rigidbody rb;
    //private bool isGrounded = true;
    private bool isGrounded => Physics.Raycast(transform.position, Vector3.down, GetComponent<Collider>().bounds.extents.y + 0.1f);

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        jumpActionReference.action.performed += Jump;
    }

    // Update is called once per frame
    void Update()
    {
        //isGrounded = Physics.Raycast(new Vector2(transform.position.x, transform.position.y + 2f), Vector3.down, 2f);
    }

    void Jump(InputAction.CallbackContext context)
    {
        if (!isGrounded)
            return;
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
}
