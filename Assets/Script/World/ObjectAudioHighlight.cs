using UnityEngine;
using System.Collections;
using TMPro;

public class ObjectMaterialHighlight : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private float _cooldownDuration = 1.5f;

    [Header("Material Settings (Multi-Object)")]
    [SerializeField] private Material _highlightMaterial;

    [Header("UI Guide Settings")]
    [SerializeField] private TextMeshProUGUI _guideTextComponent;

    private Renderer[] _allRenderers;
    private Material[] _originalMaterials;
    private bool _isCooldown = false;

    private void Start()
    {
        _allRenderers = GetComponentsInChildren<Renderer>();

        if (_allRenderers != null && _allRenderers.Length > 0)
        {
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

        if (_guideTextComponent != null)
        {
            _guideTextComponent.gameObject.SetActive(false);
        }
    }

    private void OnMouseEnter()
    {
        if (_guideTextComponent != null)
        {
            _guideTextComponent.gameObject.SetActive(true);
        }

        if (_allRenderers == null || _highlightMaterial == null) return;

        for (int i = 0; i < _allRenderers.Length; i++)
        {
            if (_allRenderers[i] != null)
            {
                _allRenderers[i].material = _highlightMaterial;
            }
        }
    }

    private void OnMouseExit()
    {
        if (_guideTextComponent != null)
        {
            _guideTextComponent.gameObject.SetActive(false);
        }

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