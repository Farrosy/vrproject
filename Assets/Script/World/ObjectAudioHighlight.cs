using UnityEngine;
using System.Collections;

public class ObjectMaterialHighlight : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private float _cooldownDuration = 1.5f;

    [Header("Material Settings (Multi-Object)")]
    [Tooltip("Tarik file Material CERAH/GLOW kamu ke sini")]
    [SerializeField] private Material _highlightMaterial;

    private Renderer[] _allRenderers;    // Menyimpan semua renderer objek anak
    private Material[] _originalMaterials; // Menyimpan semua material asli masing-masing objek anak
    private bool _isCooldown = false;

    private void Start()
    {
        // [PERBAIKAN UTAMA]: Ambil semua komponen Mesh Renderer dari seluruh kubus di bawah parent ini
        _allRenderers = GetComponentsInChildren<Renderer>();

        if (_allRenderers != null && _allRenderers.Length > 0)
        {
            // Amankan data material asli dari masing-masing kubus
            _originalMaterials = new Material[_allRenderers.Length];
            for (int i = 0; i < _allRenderers.Length; i++)
            {
                _originalMaterials[i] = _allRenderers[i].material;
            }
        }

        if (_audioSource != null)
        {
            _audioSource.loop = false;
        }
    }

    // 1. Saat kursor masuk: Ubah SEMUA material kubus anak menjadi Highlight Material
    private void OnMouseEnter()
    {
        if (_allRenderers == null || _highlightMaterial == null) return;

        for (int i = 0; i < _allRenderers.Length; i++)
        {
            if (_allRenderers[i] != null)
            {
                _allRenderers[i].material = _highlightMaterial;
            }
        }
    }

    // 2. Saat kursor keluar: Kembalikan SEMUA material kubus anak ke versi aslinya
    private void OnMouseExit()
    {
        if (_allRenderers == null || _originalMaterials == null) return;

        for (int i = 0; i < _allRenderers.Length; i++)
        {
            if (_allRenderers[i] != null && _originalMaterials[i] != null)
            {
                _allRenderers[i].material = _originalMaterials[i];
            }
        }
    }

    private void OnMouseDown()
    {
        if (_isCooldown || _audioSource == null) return;
        StartCoroutine(PlayAudioWithCooldown());
    }

    private IEnumerator PlayAudioWithCooldown()
    {
        _isCooldown = true;
        _audioSource.Play();
        yield return new WaitForSeconds(_cooldownDuration);
        _isCooldown = false;
    }
}