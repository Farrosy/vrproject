using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement Speeds")]
    public float moveSpeed = 5f;
    public float gravity = -9.81f;

    [Header("Look Sensitivity")]
    public float mouseSensitivity = 10f; // Nilai sensitivitas disesuaikan untuk sistem lama
    public Transform cameraTransform;
    public float upperLookLimit = -80f;
    public float lowerLookLimit = 80f;

    private CharacterController characterController;
    private Vector3 velocity;
    private float verticalRotation = 0f;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        
        // Mengunci kursor mouse di tengah layar
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
        // Memastikan karakter menyentuh tanah
        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // Mengambil input WASD / Arrow Keys menggunakan metode lama
        float moveX = Input.GetAxis("Horizontal"); // A/D atau Kiri/Kanan
        float moveZ = Input.GetAxis("Vertical");   // W/S atau Atas/Bawah

        // Menghitung arah jalan berdasarkan arah hadap karakter
        Vector3 moveDirection = transform.right * moveX + transform.forward * moveZ;
        characterController.Move(moveDirection * moveSpeed * Time.deltaTime);

        // Efek Gravitasi
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    private void HandleLook()
    {
        // Mengambil input pergerakan Mouse menggunakan metode lama
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // 1. Rotasi Horizontal (Karakter berputar ke kiri/kanan bersama kamera)
        transform.Rotate(Vector3.up * mouseX);

        // 2. Rotasi Vertical (Kamera mendongak ke atas/bawah)
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, upperLookLimit, lowerLookLimit);

        if (cameraTransform != null)
        {
            cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        }
    }
}