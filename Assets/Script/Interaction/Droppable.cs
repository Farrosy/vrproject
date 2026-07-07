using UnityEngine;

/// <summary>
/// Stores drop data for an object that can be accepted by a DropZoneHandler.
/// </summary>
public class Droppable : MonoBehaviour
{
    [Header("Drop Data")]
    [SerializeField] private string _dropId = "Default";
    [SerializeField] private bool _disableAfterDropped;

    private Rigidbody _rb;

    public string DropId => _dropId;
    public bool IsDropped { get; private set; }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public void MarkDropped()
    {
        IsDropped = true;

        if (_disableAfterDropped)
        {
            gameObject.SetActive(false);
        }
    }

    public void ResetDroppedStatus()
    {
        IsDropped = false;
        Debug.Log("[Droppable] Status IsDropped di-reset menjadi false untuk " + gameObject.name);
    }

    // ==================== AUTO-FIX BACKUP FOR GRAVITY ====================
    // Fungsi ini berjalan otomatis di Unity jika player mengangkat objek ini kembali.
    // Ini menjamin useGravity akan dipaksa menyala kembali secara independen dari script manapun.
    private void OnTransformParentChanged()
    {
        // Jika objek diangkat oleh player (punya parent baru berupa tangan controller)
        if (transform.parent != null)
        {
            ResetDroppedStatus();
            if (_rb != null)
            {
                _rb.isKinematic = false;
                _rb.useGravity = true; // Paksa mencentang ulang di inspector secara internal
            }
        }
    }
}