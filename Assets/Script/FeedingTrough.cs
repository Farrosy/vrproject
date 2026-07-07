using UnityEngine;
using System.Collections;
using UnityEngine.UI; 
using TMPro; 
using ithappy.Animals_FREE; 

public class FeedingTrough : MonoBehaviour
{
    public enum TroughType { Herbivore, Carnivore }

    [Header("Trough Target Type")]
    [Tooltip("Pilih tipe tempat makan ini (Herbivore untuk Sapi/Kuda, Carnivore untuk Harimau)")]
    public TroughType troughType = TroughType.Herbivore;

    [Header("Animals Settings")]
    public GameObject[] targetAnimals; 
    
    [Header("Highlight & UI Settings")]
    public MeshRenderer troughRenderer; 
    public Material highlightMaterial; 
    public TextMeshProUGUI interactionText; 
    
    [Header("Alert Settings")]
    public GameObject emptyAlertIcon;

    [Header("Audio Settings")]
    public AudioSource eatingAudioSource; 
    [Tooltip("Masukkan Audio Source untuk efek suara jika hewan diberi makanan yang salah")]
    public AudioSource wrongFoodAudioSource; 

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
        if (troughRenderer != null) originalMaterial = troughRenderer.material;
        if (interactionText != null) interactionText.gameObject.SetActive(false);

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
        if (activeDroppedFood != null && currentHunger > 0f)
        {
            // Jika makanan yang valid sedang aktif, tahan bar hunger agar tetap penuh
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<FirstPersonController>() != null)
        {
            CallAllAnimals();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if ((other.CompareTag("Player") || other.GetComponent<FirstPersonController>() != null) && !IsFull())
        {
            DismissAllAnimals();
        }
    }

    private void CallAllAnimals()
    {
        if (targetAnimals == null) return;
        foreach (GameObject animal in targetAnimals)
        {
            if (animal == null) continue;
            if (animal.TryGetComponent<AnimalWander>(out var cow)) cow.GoToFeeder(transform.position);
            else if (animal.TryGetComponent<HorseAI>(out var horse)) horse.GoToFeeder(transform.position);
            else if (animal.TryGetComponent<TigerAI>(out var tiger)) tiger.GoToFeeder(transform.position);
        }
    }

    private void DismissAllAnimals()
    {
        if (targetAnimals == null) return;
        foreach (GameObject animal in targetAnimals)
        {
            if (animal == null) continue;
            if (animal.TryGetComponent<AnimalWander>(out var cow)) cow.ResumeWandering();
            else if (animal.TryGetComponent<HorseAI>(out var horse)) horse.ResumeWandering();
            else if (animal.TryGetComponent<TigerAI>(out var tiger)) tiger.ResumeWandering();
        }
    }

    public void FillOneFood(GameObject foodObject)
    {
        if (foodObject == null || activeDroppedFood != null) return;

        // Cek ID makanan dari komponen Droppable
        string foodId = "";
        if (foodObject.TryGetComponent<Droppable>(out var droppable))
        {
            foodId = droppable.DropId;
        }

        // ==================== LOGIKA VALIDASI MAKANAN SALAH ====================
        if (troughType == TroughType.Carnivore && foodId != "Meat")
        {
            Debug.LogWarning("Harimau menolak makanan ini! Memutar suara salah makan.");
            if (wrongFoodAudioSource != null) wrongFoodAudioSource.Play();
            
            // 1. Makanan tetap berada di situ (tidak di-destroy)
            // 2. Bar hungry tidak bertambah, hewan tidak dipanggil makan
            return; 
        }
        // =======================================================================

        // Jika makanan benar (atau untuk tempat makan Herbivore biasa)
        activeDroppedFood = foodObject;
        currentHunger = maxHunger;
        if (hungerSlider != null) hungerSlider.value = currentHunger;

        CallAllAnimals();

        if (eatingAudioSource != null) eatingAudioSource.Play();

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
        DismissAllAnimals();

        if (eatingAudioSource != null) eatingAudioSource.Stop();
    }

    public void ToggleHighlight(bool state)
    {
        isHighlighted = state;
        if (troughRenderer == null || highlightMaterial == null) return;
        troughRenderer.material = isHighlighted ? highlightMaterial : originalMaterial;
    }

    private void UpdateAlertIcon()
    {
        if (emptyAlertIcon != null) emptyAlertIcon.SetActive(currentHunger <= 0f && !isHighlighted);
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