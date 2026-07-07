using UnityEngine;
using System.Collections;
using UnityEngine.UI; 
using TMPro; 
using ithappy.Animals_FREE; 

public class FeedingTrough : MonoBehaviour
{
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
    [Tooltip("Masukkan Audio Source raungan harimau marah/kecewa di sini")]
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
    private bool isHoldingWrongFood = false; // Flag status pakan salah

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
        if (activeDroppedFood != null && !isHoldingWrongFood)
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

        // ==================== FIX BUG: PULIHKAN GRAVITASI & FISIKA OBJEK ====================
        // Jika status pakan salah aktif, cek apakah objek tersebut dilepas paksa / di-pickup oleh Player
        if (isHoldingWrongFood && activeDroppedFood != null)
        {
            // Deteksi lewat komponen Grabbable bawaan sistem kamu
            if (activeDroppedFood.TryGetComponent<Grabbable>(out var grabbable) && grabbable.IsGrabbed)
            {
                UnfreezeAndReleaseObject(activeDroppedFood);
                
                activeDroppedFood = null;
                isHoldingWrongFood = false;

                if (TryGetComponent<DropZoneHandler>(out var zone))
                {
                    zone.ResetDropZone(); // Buka kembali sensor DropZone
                }
                UpdateAlertIcon();
            }
        }
        // Kondisi cadangan jika objek hancur mendadak dari sistem luar luar
        else if (isHoldingWrongFood && activeDroppedFood == null)
        {
            isHoldingWrongFood = false;
            if (TryGetComponent<DropZoneHandler>(out var zone))
            {
                zone.ResetDropZone();
            }
            UpdateAlertIcon();
        }
        // ===================================================================================
    }

    // ==================== FUNGSI BARU: MENGEMBALIKAN CONFIG RIGIDBODY & DROPPABLE ====================
    private void UnfreezeAndReleaseObject(GameObject targetFood)
    {
        if (targetFood == null) return;

        // 1. Pulihkan status fungsional Droppable agar bisa diterima di boks pakan lain lagi
        if (targetFood.TryGetComponent<Droppable>(out var droppable))
        {
            // Mengingat script Droppable bawaan menggunakan MarkDropped(), kita harus mereset flag internalnya jika ada fungsi Reset/Unmark.
            // Namun, jika tidak ada fungsi reset bawaan, minimal komponen fisikanya kita normalkan di bawah:
            Debug.Log(targetFood.name + " berhasil dicabut! Mengembalikan setelan fisika.");
        }

        // 2. Mengaktifkan kembali sistem physics gravitasi objek
        if (targetFood.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            
            // Berikan sedikit dorongan mikro ke atas agar objek tidak amblas menembus collider wadah pakan saat di-unfreeze
            rb.AddForce(Vector3.up * 2f, ForceMode.Impulse); 
        }
    }
    // =================================================================================================

    public bool IsFull()
    {
        return activeDroppedFood != null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<FirstPersonController>() != null)
        {
            if (isHoldingWrongFood) return;

            Debug.Log("Player mendekati wadah pakan! Memanggil semua hewan untuk berkumpul.");
            CallAllAnimals();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<FirstPersonController>() != null)
        {
            if (!IsFull() || isHoldingWrongFood)
            {
                Debug.Log("Player menjauh! Membubarkan hewan.");
                DismissAllAnimals();
            }
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
        if (currentHunger > 0f || activeDroppedFood != null) return;

        activeDroppedFood = foodObject;
        isHoldingWrongFood = false;

        currentHunger = maxHunger;
        if (hungerSlider != null) hungerSlider.value = currentHunger;

        CallAllAnimals();

        if (eatingAudioSource != null) eatingAudioSource.Play();

        StartCoroutine(ResetFoodAfterDelay(5f));
        UpdateAlertIcon();
    }

    public void FillWrongFood(GameObject wrongFoodObject)
    {
        activeDroppedFood = wrongFoodObject;
        isHoldingWrongFood = true; 

        DismissAllAnimals(); 

        if (wrongFoodAudioSource != null)
        {
            wrongFoodAudioSource.Play();
        }

        Debug.LogWarning("Makanan salah menempel! Segera ambil kembali jerami untuk menggantinya.");
    }

    private IEnumerator ResetFoodAfterDelay(float delayDuration)
    {
        yield return new WaitForSeconds(delayDuration);

        if (activeDroppedFood != null && !isHoldingWrongFood)
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
        if (emptyAlertIcon != null)
        {
            emptyAlertIcon.SetActive(currentHunger <= 0f && !isHighlighted);
        }

        if (statusText != null)
        {
            statusText.text = isHoldingWrongFood ? "Salah Makanan!" : "Lapar!";
            statusText.gameObject.SetActive(currentHunger <= 0f || isHoldingWrongFood);
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