using UnityEngine;
using System.Collections;
using TMPro; 
using ithappy.Animals_FREE; // TAMBAHAN: Panggil namespace HorseAI milik asset pack

public class FeedingTrough : MonoBehaviour
{
    [Header("Food Settings")]
    public GameObject[] foodMeshes; 
    
    // [MODIFIKASI AMAN]: Ubah tipe menjadi GameObject agar bisa menampung Sapi maupun Kuda sekaligus
    [Tooltip("Masukkan Game Object Sapi (AnimalWander) atau Kuda (HorseAI) ke sini")]
    public GameObject[] targetAnimals; 
    
    [Header("Highlight & UI Settings")]
    public MeshRenderer troughRenderer; 
    public Material highlightMaterial; 
    public TextMeshProUGUI interactionText; 
    
    [Header("Alert Settings")]
    public GameObject emptyAlertIcon;

    [Header("Audio Settings")]
    public AudioSource eatingAudioSource; 

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
        isHighlighted = state;

        if (interactionText != null)
        {
            if (isHighlighted)
            {
                if (foodMeshes != null && foodMeshes.Length == 1)
                {
                    interactionText.text = "Tekan [E] untuk Memberi Minuman";
                }
                else
                {
                    interactionText.text = "Tekan [E] untuk Memberi Makan";
                }
                interactionText.gameObject.SetActive(!IsFull());
            }
            else
            {
                UpdateAlertIcon();
            }
        }

        if (troughRenderer == null || highlightMaterial == null) return;
        troughRenderer.material = isHighlighted ? highlightMaterial : originalMaterial;
    }

    public void FillOneFood()
    {
        if (currentFoodIndex >= foodMeshes.Length) return;

        int foodIndexToFill = currentFoodIndex;
        foodMeshes[foodIndexToFill].SetActive(true);
        
        // [MODIFIKASI AMAN]: Cek otomatis apakah target itu Sapi atau Kuda
        if (targetAnimals != null)
        {
            foreach (GameObject animal in targetAnimals)
            {
                if (animal == null) continue;

                // 1. Cek apakah ini Sapi (punya script AnimalWander)
                if (animal.TryGetComponent<AnimalWander>(out var cow))
                {
                    cow.GoToFeeder(transform.position);
                }
                // 2. Cek apakah ini Kuda (punya script HorseAI yang baru kita gabung)
                else if (animal.TryGetComponent<HorseAI>(out var horse))
                {
                    horse.GoToFeeder(transform.position);
                }
            }
        }

        if (eatingAudioSource != null)
        {
            eatingAudioSource.Play();
        }

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
            // [MODIFIKASI AMAN]: Kembalikan semua jenis hewan berkeliling
            foreach (GameObject animal in targetAnimals)
            {
                if (animal == null) continue;

                if (animal.TryGetComponent<AnimalWander>(out var cow))
                {
                    cow.ResumeWandering();
                }
                else if (animal.TryGetComponent<HorseAI>(out var horse))
                {
                    horse.ResumeWandering();
                }
            }

            if (eatingAudioSource != null)
            {
                eatingAudioSource.Stop();
            }
        }
    }

    public bool IsFull()
    {
        return currentFoodIndex >= foodMeshes.Length;
    }

    private void UpdateAlertIcon()
    {
        if (emptyAlertIcon != null)
        {
            emptyAlertIcon.SetActive(currentFoodIndex == 0 && !isHighlighted);
        }

        if (interactionText != null && !isHighlighted)
        {
            if (currentFoodIndex == 0)
            {
                interactionText.text = "!";
                interactionText.gameObject.SetActive(true);
            }
            else
            {
                interactionText.text = "";
                interactionText.gameObject.SetActive(false);
            }
        }
    }
}