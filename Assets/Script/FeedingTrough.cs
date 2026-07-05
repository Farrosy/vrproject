using UnityEngine;
using System.Collections;
using TMPro; 

public class FeedingTrough : MonoBehaviour
{
    [Header("Food Settings")]
    public GameObject[] foodMeshes; 
    public AnimalWander[] targetAnimals; 
    
    [Header("Highlight & UI Settings")]
    public MeshRenderer troughRenderer; 
    public Material highlightMaterial; 
    public TextMeshProUGUI interactionText; 
    
    [Header("Alert Settings")]
    public GameObject emptyAlertIcon;

    // ==================== TAMBAHAN UNTUK AUDIO (3D SOUND) ====================
    [Header("Audio Settings")]
    [Tooltip("Masukkan komponen AudioSource yang ada di GameObject tempat makan ini")]
    public AudioSource eatingAudioSource; 
    // =========================================================================

    private int currentFoodIndex = 0; 
    private Material originalMaterial; 
    private bool isHighlighted = false;

    void Start()
    {
        if (troughRenderer != null)
        {
            originalMaterial = troughRenderer.material;
        }

        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(false);
        }

        UpdateAlertIcon();
    }

    public void ToggleHighlight(bool state)
    {
        isHighlighted = state; // Simpan status highlight saat ini

        if (interactionText != null)
        {
            if (isHighlighted)
            {
                // JIKA DEKAT (DI-HIGHLIGHT): Tampilkan instruksi tombol E
                if (foodMeshes != null && foodMeshes.Length == 1)
                {
                    interactionText.text = "Tekan [E] untuk Memberi Minuman";
                }
                else
                {
                    interactionText.text = "Tekan [E] untuk Memberi Makan";
                }
                
                // Aktifkan teks hanya jika belum penuh
                interactionText.gameObject.SetActive(!IsFull());
            }
            else
            {
                // JIKA JAUH (TIDAK DI-HIGHLIGHT): Panggil fungsi alert untuk cek tanda seru
                UpdateAlertIcon();
            }
        }

        // Efek Highlight Material
        if (troughRenderer == null || highlightMaterial == null) return;
        troughRenderer.material = isHighlighted ? highlightMaterial : originalMaterial;
    }

    public void FillOneFood()
    {
        if (currentFoodIndex >= foodMeshes.Length) return;

        int foodIndexToFill = currentFoodIndex;
        foodMeshes[foodIndexToFill].SetActive(true);
        
        if (targetAnimals != null)
        {
            foreach (AnimalWander cow in targetAnimals)
            {
                if (cow != null) cow.GoToFeeder(transform.position);
            }
        }

        // ==================== TAMBAHAN: MAINKAN SUARA MAKAN 3D ====================
        if (eatingAudioSource != null)
        {
            eatingAudioSource.Play();
        }
        // =========================================================================

        StartCoroutine(ResetFoodAfterDelay(foodIndexToFill));
        currentFoodIndex++;

        UpdateAlertIcon();
        ToggleHighlight(isHighlighted);
    }

    private IEnumerator ResetFoodAfterDelay(int index)
    {
        yield return new WaitForSeconds(7f);

        if (foodMeshes[index] != null)
        {
            foodMeshes[index].SetActive(false);
        }

        currentFoodIndex = Mathf.Max(0, currentFoodIndex - 1);

        UpdateAlertIcon();

        if (currentFoodIndex == 0 && targetAnimals != null)
        {
            foreach (AnimalWander cow in targetAnimals)
            {
                if (cow != null) cow.ResumeWandering();
            }

            // ==================== TAMBAHAN: HENTIKAN SUARA JIKA MAKANAN HABIS ====================
            if (eatingAudioSource != null)
            {
                eatingAudioSource.Stop();
            }
            // ====================================================================================
        }
    }

    public bool IsFull()
    {
        return currentFoodIndex >= foodMeshes.Length;
    }

    private void UpdateAlertIcon()
    {
        // Pengecekan untuk tanda seru objek 3D/Sprite terpisah (jika kamu pakai emptyAlertIcon)
        if (emptyAlertIcon != null)
        {
            emptyAlertIcon.SetActive(currentFoodIndex == 0 && !isHighlighted);
        }

        // Pengecekan untuk text UI (interactionText) saat player berada di luar jangkauan (jauh)
        if (interactionText != null && !isHighlighted)
        {
            if (currentFoodIndex == 0)
            {
                // Kalau kosong, tampilkan tanda seru "!" dan nyalakan text-nya
                interactionText.text = "!";
                interactionText.gameObject.SetActive(true);
            }
            else
            {
                // Kalau sudah diberi makan, biarkan kosong/matikan teksnya
                interactionText.text = "";
                interactionText.gameObject.SetActive(false);
            }
        }
    }
}