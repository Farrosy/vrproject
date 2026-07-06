using UnityEngine;
using System.Collections;
using TMPro;
using ithappy.Animals_FREE;

public class WaterTrough : MonoBehaviour
{
    [Header("Water Mesh Settings")]
    [Tooltip("Masukkan objek anak 'Water' milik TEMPAT MINUM ini ke sini")]
    [SerializeField] private GameObject _troughWaterMesh;

    [Header("Animals Settings")]
    [Tooltip("Masukkan Game Object Sapi (AnimalWander) atau Kuda (HorseAI) ke sini")]
    [SerializeField] private GameObject[] _targetAnimals;

    [Header("Highlight & UI Settings")]
    public MeshRenderer troughRenderer; 
    public Material highlightMaterial; 
    public TextMeshProUGUI interactionText; 
    
    private Material originalMaterial; 
    private bool isHighlighted = false;
    private bool isWaterActive = false;

    private void Start()
    {
        if (_troughWaterMesh != null)
        {
            _troughWaterMesh.SetActive(false); // Awal game tempat minum kosong
        }

        if (troughRenderer != null)
        {
            originalMaterial = troughRenderer.material;
        }

        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(false);
        }
    }

    public bool IsFull()
    {
        return isWaterActive;
    }

    public void ToggleHighlight(bool state)
    {
        isHighlighted = state;
        if (troughRenderer == null || highlightMaterial == null) return;
        troughRenderer.material = isHighlighted ? highlightMaterial : originalMaterial;
    }

    // ==================== LOGIKA UTAMA: TUANG AIR LANGSUNG ====================
    public void PourWater(GameObject bucketObject)
    {
        if (bucketObject == null || isWaterActive) return;

        // [PERBAIKAN UTAMA]: Mencari objek bernama "Water" di seluruh tingkatan anak/cucu (Anti-Null)
        GameObject bucketWaterObject = null;
        Transform[] allChildren = bucketObject.GetComponentsInChildren<Transform>(true);
        
        foreach (Transform child in allChildren)
        {
            if (child.gameObject.name == "Water")
            {
                bucketWaterObject = child.gameObject;
                break;
            }
        }
        
        // Validasi: Pastikan objek air ditemukan dan dalam kondisi aktif (ember ada airnya)
        if (bucketWaterObject == null || !bucketWaterObject.activeSelf)
        {
            Debug.LogWarning("Ember kosong atau objek 'Water' tidak ditemukan pada barang bawaan! Isi air dulu [F].");
            return;
        }

        // 2. JALANKAN LOGIKA: Disable water di bucket, Enable water di tempat minum
        bucketWaterObject.SetActive(false); // Air ember mati

        if (_troughWaterMesh != null)
        {
            _troughWaterMesh.SetActive(true); // Air tempat minum menyala
        }

        isWaterActive = true;

        // 3. Panggil hewan untuk berkumpul minum
        if (_targetAnimals != null)
        {
            foreach (GameObject animal in _targetAnimals)
            {
                if (animal == null) continue;
                if (animal.TryGetComponent<AnimalWander>(out var cow)) cow.GoToFeeder(transform.position);
                else if (animal.TryGetComponent<HorseAI>(out var horse)) horse.GoToFeeder(transform.position);
            }
        }

        // 4. Mulai timer 5 detik air surut/habis
        StartCoroutine(ResetWaterAfterDelay(5f));
        Debug.Log("Air berhasil dituang mendalam! Sapi bergerak mendekat.");
    }

    private IEnumerator ResetWaterAfterDelay(float duration)
    {
        yield return new WaitForSeconds(duration);

        if (_troughWaterMesh != null)
        {
            _troughWaterMesh.SetActive(false);
        }

        isWaterActive = false;

        // Hewan selesai minum dan kembali berpencar secara acak
        if (_targetAnimals != null)
        {
            foreach (GameObject animal in _targetAnimals)
            {
                if (animal == null) continue;
                if (animal.TryGetComponent<AnimalWander>(out var cow)) cow.ResumeWandering();
                else if (animal.TryGetComponent<HorseAI>(out var horse)) horse.ResumeWandering();
            }
        }
        
        Debug.Log("Air minum habis, hewan kembali berpencar.");
    }
}