using TMPro;
using UnityEngine;
using UnityEngine.Events;

// ==================== TAMBAHAN: DYNAMIC EVENT CLASS ====================
[System.Serializable]
public class DropEvent : UnityEvent<GameObject> { }
// =======================================================================

/// <summary>
/// Handles drop-zone validation for Droppable objects.
/// Accepts the correct object only after it has been released by the player.
/// </summary>
public class DropZoneHandler : MonoBehaviour
{
    [Header("Drop Requirement")]
    [SerializeField] private string _requiredDropId = "Default";

    [Header("Snap Settings")]
    [SerializeField] private bool _snapObjectToPoint = true;
    [SerializeField] private Transform _snapPoint;
    [SerializeField] private bool _freezeObjectAfterDrop = true;

    [Header("Feedback")]
    [SerializeField] private TextMeshProUGUI _statusText;
    [SerializeField] private GameObject _objectToActivateAfterDrop;

    [Header("Events")]
    // [FIXED]: Menggunakan DropEvent kustom agar bisa mengirimkan GameObject ke FeedingTrough
    [SerializeField] private DropEvent _onCorrectDrop;
    [SerializeField] private UnityEvent _onWrongDrop;

    private bool isCompleted;
    private Droppable lastWrongDroppable;

    private void OnTriggerEnter(Collider other)
    {
        TryAcceptDroppable(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryAcceptDroppable(other);
    }

    private void TryAcceptDroppable(Collider other)
    {
        if (isCompleted)
        {
            return;
        }

        Droppable droppable = other.GetComponentInParent<Droppable>();

        if (droppable == null || droppable.IsDropped)
        {
            return;
        }

        Grabbable grabbable = droppable.GetComponent<Grabbable>();

        if (grabbable != null && grabbable.IsGrabbed)
        {
            SetStatus("Drop the object first");
            return;
        }

        if (droppable.DropId != _requiredDropId)
        {
            HandleWrongDrop(droppable);
            return;
        }

        HandleCorrectDrop(droppable);
    }

    private void HandleWrongDrop(Droppable droppable)
    {
        if (lastWrongDroppable == droppable)
        {
            SetStatus("Wrong object: " + droppable.DropId);
            return;
        }

        lastWrongDroppable = droppable;
        SetStatus("Wrong object: " + droppable.DropId);
        _onWrongDrop?.Invoke();
    }

    private void HandleCorrectDrop(Droppable droppable)
    {
        isCompleted = true;

        SnapObjectIfNeeded(droppable.transform);
        FreezeObjectIfNeeded(droppable);

        droppable.MarkDropped();

        if (_objectToActivateAfterDrop != null)
        {
            _objectToActivateAfterDrop.SetActive(true);
        }

        SetStatus("Correct object dropped. Objective complete.");
        
        // [FIXED]: Mengirimkan parameter objek yang di-drop (Hay Bale) ke fungsi target
        _onCorrectDrop?.Invoke(droppable.gameObject);
    }

    private void SnapObjectIfNeeded(Transform targetObject)
    {
        if (!_snapObjectToPoint || _snapPoint == null || targetObject == null)
        {
            return;
        }

        targetObject.position = _snapPoint.position;
        targetObject.rotation = _snapPoint.rotation;
    }

    private void FreezeObjectIfNeeded(Droppable droppable)
    {
        if (!_freezeObjectAfterDrop || droppable == null)
        {
            return;
        }

        Rigidbody rigidbody = droppable.GetComponent<Rigidbody>();

        if (rigidbody == null)
        {
            return;
        }

        // Kompatibilitas Unity 2023/2026 untuk menghentikan laju fisik objek
        #if UNITY_2023_1_OR_NEWER
        rigidbody.linearVelocity = Vector3.zero;
        #else
        rigidbody.velocity = Vector3.zero;
        #endif
        
        rigidbody.angularVelocity = Vector3.zero;
        rigidbody.useGravity = false;
        rigidbody.isKinematic = true;
    }

    private void SetStatus(string message)
    {
        if (_statusText != null)
        {
            _statusText.text = message;
        }

        Debug.Log(message);
    }

    // ==================== TAMBAHAN: FITUR RESET AREA PAKAN ====================
    /// <summary>
    /// Dipanggil saat makanan hancur setelah 5 detik agar DropZone bisa menerima makanan baru lagi.
    /// </summary>
    public void ResetDropZone()
    {
        isCompleted = false;
        lastWrongDroppable = null;
        SetStatus("Ready for next food delivery.");
    }
    // ==========================================================================
}