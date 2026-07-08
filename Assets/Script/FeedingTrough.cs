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

    private Collider m_TroughCollider;

    void Start()
    {
        m_TroughCollider = GetComponent<Collider>();

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

        // ==================== FIX SOLUSI AMAN SINKRONISASI GRAB ====================
        if (isHoldingWrongFood && activeDroppedFood != null)
        {
            if (activeDroppedFood.TryGetComponent<Grabbable>(out var grabbable) && grabbable.IsGrabbed)
            {
                // 1. Bersihkan status Droppable secara instan agar boks detektor langsung mengenali pakan ini baru lagi kelak
                if (activeDroppedFood.TryGetComponent<Droppable>(out var droppable))
                {
                    droppable.ResetDroppedStatus();
                }

                // 2. Kembalikan fungsional Collider pakan menjadi solid non-trigger secara instan
                if (activeDroppedFood.TryGetComponent<Collider>(out var col))
                {
                    col.isTrigger = false;
                }

                // 3. PANGGIL BACKUP FORCE: Paksa setelan dasar internal Grabbable kembali ke setelan default normal (Bebas Melayang)
                if (activeDroppedFood.TryGetComponent<Grabbable>(out var wrongGrabbable))
                {
                    // Fungsi pembantu ini kita panggil jika ada di Grabbable, jika tidak kita force lurus via Rigidbody saat dilepas nanti
                    // Kita tidak memaksa rb.useGravity = true di sini karena sedang dipegang tangan player!
                }

                // 4. Bebaskan status wadah pakan
                activeDroppedFood = null;
                isHoldingWrongFood = false;

                if (m_TroughCollider != null) m_TroughCollider.enabled = true;

                if (TryGetComponent<DropZoneHandler>(out var zone))
                {
                    zone.ResetDropZone(); 
                }
                UpdateAlertIcon();
                Debug.Log("[Trough] Pakan salah berhasil diambil. Status zona dibersihkan.");
            }
        }
        else if (isHoldingWrongFood && activeDroppedFood == null)
        {
            isHoldingWrongFood = false;
            if (m_TroughCollider != null) m_TroughCollider.enabled = true;

            if (TryGetComponent<DropZoneHandler>(out var zone))
            {
                zone.ResetDropZone();
            }
            UpdateAlertIcon();
        }
    }

    public bool IsFull()
    {
        return activeDroppedFood != null;
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1. FILTER UTAMA: Cek apakah objek yang masuk (other) adalah AI hewan itu sendiri
        bool apakahIniHewan = other.GetComponentInParent<HorseAI>() != null || 
                            other.GetComponent<HorseAI>() != null || 
                            other.GetComponentInParent<UnityEngine.AI.NavMeshAgent>() != null ||
                            other.name.ToLower().Contains("cow") || 
                            other.name.ToLower().Contains("tiger") || 
                            other.name.ToLower().Contains("horse");

        // 2. JIKALAU YANG MENYENTUH MANGKOK ADALAH HEWAN, LANGSUNG TOLAK DAN ABORKAN!
        // Ini gunanya biar di awal game AI hewan tidak memicu fungsinya sendiri secara tidak sengaja
        if (apakahIniHewan) 
        {
            return; 
        }

        // 3. JIKALAU LOLOS (Artinya yang masuk adalah Player atau Objek Makanan asli), JALANKAN LOGIKA:
        if (isHoldingWrongFood) return;
        
        // AI hewan baru akan dipanggil ke kandang SEKARANG (secara sah saat pakan ditaruh)
        CallAllAnimals();

        // Catat data ke gerbang exit secara sah
        if (gameObject.name == "Prop_AnimalFeeder_Eat_Cow")
        {
            GameDataManager.sudahMakanSapi = true;
            Debug.Log(">>> DATA ABSOLUT: Sapi sukses diberi makan secara sah! <<<");
        }
        else if (gameObject.name == "Prop_AnimalFeeder_Eat_Horse")
        {
            GameDataManager.sudahMakanKuda = true;
            Debug.Log(">>> DATA ABSOLUT: Kuda sukses diberi makan secara sah! <<<");
        }
        else if (gameObject.name == "Prop_AnimalFeeder_Eat_Tiger")
        {
            GameDataManager.sudahMakanHarimau = true;
            Debug.Log(">>> DATA ABSOLUT: Harimau sukses diberi makan secara sah! <<<");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<FirstPersonController>() != null)
        {
            if (!IsFull() || isHoldingWrongFood)
            {
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

        if (m_TroughCollider != null) m_TroughCollider.enabled = false;

        currentHunger = maxHunger;
        if (hungerSlider != null) hungerSlider.value = currentHunger;

        CallAllAnimals();

        if (eatingAudioSource != null) eatingAudioSource.Play();

        StartCoroutine(ResetFoodAfterDelay(5f));
        UpdateAlertIcon();
    }

    public void FillWrongFood(GameObject wrongFoodObject)
    {
        if (activeDroppedFood == wrongFoodObject) return;

        activeDroppedFood = wrongFoodObject;
        isHoldingWrongFood = true; 

        // JANGAN nonaktifkan collider wadah di sini agar update deteksi di atas tetap jalan responsif!
        DismissAllAnimals(); 

        if (wrongFoodAudioSource != null)
        {
            wrongFoodAudioSource.Play();
        }
    }

    private IEnumerator ResetFoodAfterDelay(float delayDuration)
    {
        yield return new WaitForSeconds(delayDuration);

        if (activeDroppedFood != null && !isHoldingWrongFood)
        {
            if (m_TroughCollider != null) m_TroughCollider.enabled = true;

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