using UnityEngine;
using System.Collections;
using TMPro; // Pastikan ini ada untuk TextMeshPro

public class FeedingTrough : MonoBehaviour
{
    [Header("Food Settings")]
    public GameObject[] foodMeshes; 
    public AnimalWander[] targetAnimals; 
    
    [Header("Highlight & UI Settings")]
    public MeshRenderer troughRenderer; 
    public Material highlightMaterial; 
    public TextMeshProUGUI interactionText; // TAMBAHAN: Tarik UI Text World Space milik wadah ini ke sini

    private int currentFoodIndex = 0; 
    private Material originalMaterial; 
    private bool isHighlighted = false;

    void Start()
    {
        if (troughRenderer != null)
        {
            originalMaterial = troughRenderer.material;
        }

        // Sembunyikan text saat game dimulai
        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(false);
        }
    }

    // Mengaktifkan efek cahaya DAN teks interaksi sekaligus
    public void ToggleHighlight(bool state)
    {
        // 1. Logika Teks Interaksi
        if (interactionText != null)
        {
            // Cek jumlah foodMeshes untuk menentukan teks yang sesuai
            if (foodMeshes != null && foodMeshes.Length == 1)
            {
                interactionText.text = "Tekan [E] untuk Memberi Minuman";
            }
            else
            {
                interactionText.text = "Tekan [E] untuk Memberi Makan";
            }

            // Teks hanya muncul jika diaktifkan (true) DAN wadah belum penuh
            interactionText.gameObject.SetActive(state && !IsFull());
        }

        // 2. Logika Material Glow
        if (troughRenderer == null || highlightMaterial == null) return;
        if (isHighlighted == state) return; 

        isHighlighted = state;
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

        StartCoroutine(ResetFoodAfterDelay(foodIndexToFill));
        currentFoodIndex++;

        // Update status UI/Highlight setelah makanan bertambah (jika penuh, teks langsung hilang)
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

        if (currentFoodIndex == 0 && targetAnimals != null)
        {
            foreach (AnimalWander cow in targetAnimals)
            {
                if (cow != null) cow.ResumeWandering();
            }
        }
    }

    public bool IsFull()
    {
        return currentFoodIndex >= foodMeshes.Length;
    }
}