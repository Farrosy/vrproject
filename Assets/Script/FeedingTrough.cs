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

    public void FillOneFood(GameObject foodObject)
    {
        if (currentHunger > 0f || activeDroppedFood != null) return;

        activeDroppedFood = foodObject;

        currentHunger = maxHunger;
        if (hungerSlider != null) hungerSlider.value = currentHunger;

        if (targetAnimals != null)
        {
            foreach (GameObject animal in targetAnimals)
            {
                if (animal == null) continue;

                // 1. Cek Komponen Sapi
                if (animal.TryGetComponent<AnimalWander>(out var cow))
                {
                    cow.GoToFeeder(transform.position);
                }
                // 2. Cek Komponen Kuda
                else if (animal.TryGetComponent<HorseAI>(out var horse))
                {
                    horse.GoToFeeder(transform.position);
                }
                // ==================== FIX: PANGGIL HARIMAU KETIKA MAKANAN ADA ====================
                else if (animal.TryGetComponent<TigerAI>(out var tiger))
                {
                    tiger.GoToFeeder(transform.position);
                }
                // =================================================================================
            }
        }

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

        if (targetAnimals != null)
        {
            foreach (GameObject animal in targetAnimals)
            {
                if (animal == null) continue;

                // 1. Bubarkan Sapi
                if (animal.TryGetComponent<AnimalWander>(out var cow))
                {
                    cow.ResumeWandering();
                }
                // 2. Bubarkan Kuda
                else if (animal.TryGetComponent<HorseAI>(out var horse))
                {
                    horse.ResumeWandering();
                }
                // ==================== FIX: BUBARKAN HARIMAU KETIKA MAKANAN HABIS ====================
                else if (animal.TryGetComponent<TigerAI>(out var tiger))
                {
                    tiger.ResumeWandering();
                }
                // ====================================================================================
            }
        }

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