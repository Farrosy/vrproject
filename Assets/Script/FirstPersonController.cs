using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement Speeds")]
    public float moveSpeed = 5f;
    public float gravity = -9.81f;

    [Header("Look Sensitivity")]
    public float mouseSensitivity = 10f; 
    public Transform cameraTransform;
    public float upperLookLimit = -40f; // Sudah disesuaikan agar tidak nembus badan
    public float lowerLookLimit = 60f;

    private CharacterController characterController;
    private Animator animator; // TAMBAHAN: Variabel untuk menyimpan komponen Animator
    private Vector3 velocity;
    private float verticalRotation = 0f;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>(); // TAMBAHAN: Mengambil komponen Animator dari objek ini
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMovement();
        HandleLook();
    }

    private void HandleMovement()
    {
        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float moveX = Input.GetAxis("Horizontal"); 
        float moveZ = Input.GetAxis("Vertical");   

        Vector3 moveDirection = transform.right * moveX + transform.forward * moveZ;
        characterController.Move(moveDirection * moveSpeed * Time.deltaTime);

        // ==================== LOGIKA BARU UNTUK ANIMASI MUNDUR ====================
        if (animator != null)
        {
            // Jika moveZ lebih dari 0, artinya tekan W (Maju)
            bool isMovingForward = (moveZ > 0.1f || (moveX != 0 && moveZ >= 0));
            
            // Jika moveZ kurang dari 0, artinya tekan S (Mundur)
            bool isMovingBackward = (moveZ < -0.1f);

            // Kirim data ke Animator
            animator.SetBool("IsWalking", isMovingForward);
            animator.SetBool("IsWalkingBackward", isMovingBackward);
        }
        // ==========================================================================

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    private void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, upperLookLimit, lowerLookLimit);

        if (cameraTransform != null)
        {
            cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        }
    }
}