using UnityEngine;
using UnityEngine.InputSystem; // Indispensable para que esto funcione

[RequireComponent(typeof(CharacterController))]
public class Controller : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private bool canMove = true;
    [SerializeField, Range(0f, 10f)] private float walkingSpeed = 3.5f;
    [SerializeField, Range(0f, 10f)] private float runningSpeed = 5.0f;
    private float playerCurrentSpeed;

    [Header("Look")]
    [SerializeField] private bool canLook = true;
    [SerializeField, Range(0f, 10f)] private float lookSensitivity = 0.1f;
    private float lookVerticalMaxAngle = 90f;
    private float rotationX = 0;

    [Header("Jump")]
    [SerializeField] private bool canJump = true;
    [SerializeField, Range(0f, 15f)] private float jumpForce = 3f;
    [SerializeField] private float gravityMultiplier = 1f;
    private bool isGrounded;

    [Header("Headbob (Persona Mayor)")]
    [SerializeField] private bool enableHeadbob = true;
    [SerializeField, Range(0f, 30f)] private float bobSpeed = 6f;
    [SerializeField, Range(0f, 0.5f)] private float bobAmount = 0.15f;
    private float defaultYPos = 0;
    private float timer = 0;

    [SerializeField] private Transform cameraContainer;
    private CharacterController characterController;


    private Vector2 inputVectorMovement;
    private Vector2 inputVectorLook;
    private bool isSprinting;
    private Vector3 moveDirection = Vector3.zero;

    void Awake()
    {
     
        characterController = GetComponent<CharacterController>();
        defaultYPos = cameraContainer.localPosition.y;

        // Bloqueo de cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
     

    }


    public void OnMove(InputAction.CallbackContext context)
    {
        inputVectorMovement = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        inputVectorLook = context.ReadValue<Vector2>();
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        // Leemos si el btn esta apretado o se solto
        isSprinting = context.ReadValueAsButton();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started && canJump && isGrounded)
        {
            moveDirection.y = jumpForce;
        }
    }

    void Update()
    {
        isGrounded = characterController.isGrounded;

        if (canMove) CharacterMovement();
        if (canLook) CameraRotation();
        if (enableHeadbob) HandleHeadbob();
    }

    void CharacterMovement()
    {
        playerCurrentSpeed = isSprinting ? runningSpeed : walkingSpeed;

        float movementDirectionY = moveDirection.y;

        Vector3 movement = (transform.forward * inputVectorMovement.y) + (transform.right * inputVectorMovement.x);
        moveDirection = movement * playerCurrentSpeed;

        
        if (isGrounded && movementDirectionY < 0)
        {
            
            moveDirection.y = -2f;
        }
        else
        {
            
            moveDirection.y = movementDirectionY;
            moveDirection.y += gravityMultiplier * Physics.gravity.y * Time.deltaTime;
        }

        characterController.Move(moveDirection * Time.deltaTime);
    }

    void CameraRotation()
    {
        
        rotationX -= inputVectorLook.y * lookSensitivity;
        rotationX = Mathf.Clamp(rotationX, -lookVerticalMaxAngle, lookVerticalMaxAngle);

        cameraContainer.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, inputVectorLook.x * lookSensitivity, 0);
    }

    void HandleHeadbob()
    {
      
        if (inputVectorMovement.sqrMagnitude > 0.1f)
        {
            timer += Time.deltaTime * (isSprinting ? bobSpeed * 1.4f : bobSpeed);
            cameraContainer.localPosition = new Vector3(
                cameraContainer.localPosition.x,
                defaultYPos + Mathf.Sin(timer) * bobAmount,
                cameraContainer.localPosition.z);
        }
        else
        {
            timer = 0;
            cameraContainer.localPosition = new Vector3(
                cameraContainer.localPosition.x,
                Mathf.Lerp(cameraContainer.localPosition.y, defaultYPos, Time.deltaTime * bobSpeed),
                cameraContainer.localPosition.z);
        }
    }

    public void EnableMovement() => canMove = true;
    public void DisableMovement() => canMove = false;
    public void EnableLook() => canLook = true;
    public void DisableLook() => canLook = false;
}