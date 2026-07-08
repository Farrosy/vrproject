using UnityEngine;
using TMPro;

public class DoorPushable : Pushable
{
    [Header("Door Custom Settings")]
    public float lowerDistance = 5f; 
    public float speed = 2f;         
    public float interactionDistance = 3.5f; // Jarak aman player ke pintu

    private TextMeshProUGUI uiNotifText;
    private Vector3 targetPosition;
    private bool isOpening = false;
    private Transform playerTransform;

    void Start()
    {
        targetPosition = transform.position + (Vector3.down * lowerDistance);

        // Mencari teks info otomatis
        GameObject textObj = GameObject.Find("info");
        if (textObj != null) uiNotifText = textObj.GetComponent<TextMeshProUGUI>();

        // Mencari Player otomatis
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) playerObj = GameObject.Find("CharacterPlayer");
        if (playerObj != null) playerTransform = playerObj.transform;
    }

    void Update()
    {
        if (isOpening)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            if (transform.position == targetPosition)
            {
                enabled = false; 
            }
            return;
        }

        if (playerTransform == null) return;

        // HITUNG JARAK AMAN (Jalur bypass jika Raycast kelompokmu memblokir objek)
        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance <= interactionDistance)
        {
            // Munculkan teks notifikasi status misi saat player mendekat
            if (uiNotifText != null && !uiNotifText.gameObject.activeSelf)
            {
                uiNotifText.gameObject.SetActive(true);
            }
            
            UpdateTextStatus();

            // BYPASS INPUT: Jika player menekan E atau Klik Kiri di dekat pintu
            if (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0))
            {
                TryOpenDoor();
            }
        }
        else
        {
            // Matikan teks jika player menjauh
            if (uiNotifText != null && uiNotifText.gameObject.activeSelf)
            {
                uiNotifText.gameObject.SetActive(false);
            }
        }
    }

    // Fungsi utama yang dipicu Raycast Klik Kiri kelompokmu (Opsi A)
    public new void Push(Vector3 direction, float force)
    {
        TryOpenDoor();
    }

    private void TryOpenDoor()
    {
        bool semuaSudahMakan = GameDataManager.sudahMakanSapi && GameDataManager.sudahMakanKuda && GameDataManager.sudahMakanHarimau;

        if (semuaSudahMakan)
        {
            isOpening = true;
            if (uiNotifText != null) uiNotifText.gameObject.SetActive(false);
            Debug.Log("Pintu sukses terbuka!");
        }
        else
        {
            if (uiNotifText != null)
            {
                uiNotifText.gameObject.SetActive(true);
                uiNotifText.text = "Selesaikan seluruh tugas memberi makan hewan!";
                uiNotifText.color = Color.red;
            }
        }
    }

    private void UpdateTextStatus()
    {
        if (uiNotifText == null) return;
        
        // Membaca ulang status data terbaru
        bool semuaSudahMakan = GameDataManager.sudahMakanSapi && GameDataManager.sudahMakanKuda && GameDataManager.sudahMakanHarimau;
        
        if (semuaSudahMakan)
        {
            uiNotifText.text = "[KLIK / TEKAN E] Untuk Keluar";
            uiNotifText.color = Color.white; // Berubah jadi putih tanda siap dibuka
        }
        else
        {
            uiNotifText.text = "Selesaikan seluruh tugas memberi makan hewan!";
            uiNotifText.color = Color.red;
        }
    }
}