using UnityEngine;
using System.Collections;
using UnityEngine.UI; 
using TMPro; 
using ithappy.Animals_FREE; 

public class FeedingTrough : MonoBehaviour
{
    [Header("Animals Settings")]
    [Tooltip("Masukkan Game Object Sapi (AnimalWander), Kuda (HorseAI), atau Harimau (TigerAI) ke sini")]
    public GameObject[] targetAnimals; 
    
    [Header("Highlight & UI Settings")]
    public MeshRenderer troughRenderer; 
    public Material highlightMaterial; 
    public TextMeshProUGUI interactionText; 
    
    [Header("Alert Settings")]
    public GameObject emptyAlertIcon;

    [Header("Audio Settings")]
    public AudioSource eatingAudioSource; 

    [Header("Hunger System Settings")]
    public Slider hungerSlider;
    public TextMeshProUGUI statusText;

    private float maxHunger = 10f;
    private float currentHunger = 0f; 
    private float hungerDecreaseTimer = 0f;
    private const float DECREASE_INTERVAL = 5f; 

    private Material originalMaterial; 
    private bool isHighlighted = false;
    
    private GameObject activeDroppedFood = null;

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

        currentHunger = 0f; 
        if (hungerSlider != null)
        {
            hungerSlider.maxValue = maxHunger;
            hungerSlider.value = currentHunger;
        }

        UpdateAlertIcon();
    }

    void Update()
    {
        if (activeDroppedFood != null)
        {
            currentHunger = maxHunger;
            if (hungerSlider != null) hungerSlider.value = currentHunger;
            hungerDecreaseTimer = 0f; 
            return;
        }

        if (currentHunger > 0f)
        {
            hungerDecreaseTimer += Time.deltaTime;
            if (hungerDecreaseTimer >= DECREASE_INTERVAL)
            {
                currentHunger = Mathf.Max(0f, currentHunger - 1f);
                if (hungerSlider != null) hungerSlider.value = currentHunger;
                
                hungerDecreaseTimer = 0f;
                UpdateAlertIcon();
            }
        }
    }

    public bool IsFull()
    {
        return activeDroppedFood != null;
    }

    // ==================== FIX BARU: DETEKSI PLAYER MENDEKAT ====================
    private void OnTriggerEnter(Collider other)
    {
        // Cek apakah yang mendekat adalah Player
        if (other.CompareTag("Player") || other.GetComponent<FirstPersonController>() != null)
        {
            Debug.Log("Player mendekati wadah pakan! Memanggil semua hewan untuk berkumpul.");
            CallAllAnimals();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Cek jika Player menjauh dan wadah dalam kondisi KOSONG, bubarkan hewan kembali keliling
        if ((other.CompareTag("Player") || other.GetComponent<FirstPersonController>() != null) && !IsFull())
        {
            Debug.Log("Player menjauh dan wadah kosong! Membubarkan hewan.");
            DismissAllAnimals();
        }
    }

    private void CallAllAnimals()
    {
        if (targetAnimals == null) return;

        foreach (GameObject animal in targetAnimals)
        {
            if (animal == null) continue;

            if (animal.TryGetComponent<AnimalWander>(out var cow))
            {
                cow.GoToFeeder(transform.position);
            }
            else if (animal.TryGetComponent<HorseAI>(out var horse))
            {
                horse.GoToFeeder(transform.position);
            }
            else if (animal.TryGetComponent<TigerAI>(out var tiger))
            {
                tiger.GoToFeeder(transform.position);
            }
        }
    }

    private void DismissAllAnimals()
    {
        if (targetAnimals == null) return;

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
            else if (animal.TryGetComponent<TigerAI>(out var tiger))
            {
                tiger.ResumeWandering();
            }
        }
    }
    // ===========================================================================

    public void FillOneFood(GameObject foodObject)
    {
        if (currentHunger > 0f || activeDroppedFood != null) return;

        activeDroppedFood = foodObject;

        currentHunger = maxHunger;
        if (hungerSlider != null) hungerSlider.value = currentHunger;

        // Panggil fungsi kumpul semua hewan
        CallAllAnimals();

        if (eatingAudioSource != null)
        {
            eatingAudioSource.Play();
        }

        StartCoroutine(ResetFoodAfterDelay(5f));
        UpdateAlertIcon();
    }

    private IEnumerator ResetFoodAfterDelay(float delayDuration)
    {
        yield return new WaitForSeconds(delayDuration);

        if (activeDroppedFood != null)
        {
            if (TryGetComponent<DropZoneHandler>(out var zone))
            {
                zone.ResetDropZone();
            }

            Destroy(activeDroppedFood);
            activeDroppedFood = null;
        }

        UpdateAlertIcon();

        // Bubarkan semua hewan jika makanan habis
        DismissAllAnimals();

        if (eatingAudioSource != null)
        {
            eatingAudioSource.Stop();
        }
    }

    public void ToggleHighlight(bool state)
    {
        isHighlighted = state;
        if (troughRenderer == null || highlightMaterial == null) return;
        troughRenderer.material = isHighlighted ? highlightMaterial : originalMaterial;
    }

    private void UpdateAlertIcon()
    {
        if (emptyAlertIcon != null)
        {
            emptyAlertIcon.SetActive(currentHunger <= 0f && !isHighlighted);
        }

        if (statusText != null)
        {
            statusText.text = "Lapar!";
            statusText.gameObject.SetActive(currentHunger <= 0f);
        }

        if (hungerSlider != null && hungerSlider.fillRect != null)
        {
            Image fillImage = hungerSlider.fillRect.GetComponent<Image>();
            if (fillImage != null)
            {
                float hungerPercentage = currentHunger / maxHunger;
                fillImage.color = Color.Lerp(Color.red, Color.green, hungerPercentage);
            }
        }
    }
}