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
    
    // ==================== TAMBAHAN UNTUK TANDA SERU ====================
    [Header("Alert Settings")]
    public GameObject emptyAlertIcon; // Tarik objek Tanda Seru (UI/Sprite) dari wadah ini ke sini
    // ===================================================================

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

        // TAMBAHAN: Saat start, karena wadah masih kosong, pastikan tanda seru menyala
        UpdateAlertIcon();
    }

    public void ToggleHighlight(bool state)
    {
        if (interactionText != null)
        {
            if (foodMeshes != null && foodMeshes.Length == 1)
            {
                interactionText.text = "Tekan [E] untuk Memberi Minuman";
            }
            else
            {
                interactionText.text = "Tekan [E] untuk Memberi Makan";
            }

            interactionText.gameObject.SetActive(state && !IsFull());
        }

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

        // TAMBAHAN: Update status tanda seru setelah diisi
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

        // TAMBAHAN: Update status tanda seru setelah makanan berkurang/habis
        UpdateAlertIcon();

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

    // ==================== FUNGSI BARU UNTUK ATUR TANDA SERU ====================
    private void UpdateAlertIcon()
    {
        if (emptyAlertIcon != null)
        {
            // Tanda seru AKTIF hanya jika wadah benar-benar kosong (currentFoodIndex == 0)
            emptyAlertIcon.SetActive(currentFoodIndex == 0);
        }
    }
    // ===========================================================================
}