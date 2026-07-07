using UnityEngine;

/// <summary>
/// Allows an object with Rigidbody to be grabbed, held at a HoldPoint, and dropped.
/// </summary>
public class Grabbable : MonoBehaviour
{
    [Header("Rigidbody Reference")]
    [SerializeField] private Rigidbody _rigidbody;

    [Header("Grab Settings")]
    [SerializeField] private bool _disableGravityOnGrab = true;
    [SerializeField] private bool _freezeRotationOnGrab = true;

    private Transform holdPoint;
    private bool originalUseGravity = true;  // Default amankan ke true
    private bool originalIsKinematic = false; // Default amankan ke false
    private RigidbodyConstraints originalConstraints;

    private Collider _collider;
    private bool originalIsTrigger;

    /// <summary>
    /// Returns true if this object is currently grabbed.
    /// </summary>
    public bool IsGrabbed { get; private set; }

    private void Awake()
    {
        if (_rigidbody == null)
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        _collider = GetComponent<Collider>();

        if (_rigidbody != null)
        {
            originalUseGravity = _rigidbody.useGravity;
            originalIsKinematic = _rigidbody.isKinematic;
            originalConstraints = _rigidbody.constraints;
        }
    }

    /// <summary>
    /// Grabs this object and moves it toward the assigned hold point.
    /// </summary>
    public void Grab(Transform targetHoldPoint)
    {
        if (_rigidbody == null || targetHoldPoint == null || IsGrabbed)
        {
            return;
        }

        holdPoint = targetHoldPoint;
        IsGrabbed = true;

        // --- FIX DARI KITA: Anti-Getar pakai Parenting ---
        transform.SetParent(holdPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        // -------------------------------------------------

        // ==================== FIX DATA PROTECTION (DARI TEMANMU) ====================
        // Jika objek di-grab saat kondisinya sedang membeku di wadah pakan salah,
        // JANGAN simpan setelan beku tersebut sebagai 'original state'. Paksa ke setelan fisis normal.
        if (_rigidbody.isKinematic && _rigidbody.useGravity == false)
        {
            originalUseGravity = true;
            originalIsKinematic = false;
        }
        else
        {
            originalUseGravity = _rigidbody.useGravity;
            originalIsKinematic = _rigidbody.isKinematic;
        }
        originalConstraints = _rigidbody.constraints;
        // ============================================================================

        // Paksa objek menjadi Kinematic saat digenggam agar gerakannya mulus mengikuti kamera player
        _rigidbody.isKinematic = true; 

        if (_collider != null)
        {
            originalIsTrigger = _collider.isTrigger; 
            _collider.isTrigger = true;              
        }

        if (_disableGravityOnGrab)
        {
            _rigidbody.useGravity = false;
        }

        if (_freezeRotationOnGrab)
        {
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        }

        #if UNITY_2023_1_OR_NEWER
        _rigidbody.linearVelocity = Vector3.zero;
        #else
        _rigidbody.velocity = Vector3.zero;
        #endif
        _rigidbody.angularVelocity = Vector3.zero;
    }

    /// <summary>
    /// Drops this object and restores its Rigidbody settings.
    /// </summary>
    public void Drop()
    {
        if (_rigidbody == null || !IsGrabbed)
        {
            return;
        }

        IsGrabbed = false;
        holdPoint = null;

        // --- FIX DARI KITA: Lepas dari tangan saat di-drop ---
        transform.SetParent(null);
        // -----------------------------------------------------

        // ==================== FIX UTAMA (DARI TEMANMU) ====================
        // Saat dilepas dari tangan player, paksa objek kembali memiliki berat dan jatuh bebas
        _rigidbody.isKinematic = originalIsKinematic; 
        _rigidbody.useGravity = originalUseGravity;
        _rigidbody.constraints = originalConstraints;

        if (_collider != null)
        {
            _collider.isTrigger = originalIsTrigger; 
        }

        #if UNITY_2023_1_OR_NEWER
        _rigidbody.linearVelocity = Vector3.zero;
        #else
        _rigidbody.velocity = Vector3.zero;
        #endif
        _rigidbody.angularVelocity = Vector3.zero;
    }
}