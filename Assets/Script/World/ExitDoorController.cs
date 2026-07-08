using UnityEngine;
using TMPro;

public class ExitDoorController : MonoBehaviour
{
    [Header("Door Settings")]
    public float lowerDistance = 5f; 
    public float speed = 2f;         
    public float activationDistance = 6f; // Dinaikkan ke 6 agar area deteksi lebih luas

    private TextMeshProUGUI uiNotifText; // Tidak butuh public/slot inspector lagi!
    private Transform playerTransform;
    private Vector3 targetPosition;
    private bool isOpening = false;
    private bool playerDiDekatPintu = false;

    void Start()
    {
        targetPosition = transform.position + (Vector3.down * lowerDistance);
        
        // 1. OTOMATIS MENCARI OBJEK TEKS NOTIFIKASI DI CANVAS KAMU
        // Skrip ini akan mencari objek bernama "info" di dalam Hierarchy kamu
        GameObject textObj = GameObject.Find("info");
        if (textObj != null)
        {
            uiNotifText = textObj.GetComponent<TextMeshProUGUI>();
        }
        else
        {
            // Jika tidak ketemu, coba cari berdasarkan nama objek duplikatmu
            textObj = GameObject.Find("Txt_Notif_PintuExit");
            if (textObj != null) uiNotifText = textObj.GetComponentInChildren<TextMeshProUGUI>();
        }

        // 2. Otomatis mencari objek Player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) playerObj = GameObject.Find("CharacterPlayer"); 
        
        if (playerObj != null) playerTransform = playerObj.transform;

        if (uiNotifText != null) uiNotifText.gameObject.SetActive(false);
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

        // Hitung jarak murni Player ke Pintu
        float jarakKePlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (jarakKePlayer <= activationDistance)
        {
            playerDiDekatPintu = true;
            if (uiNotifText != null) uiNotifText.gameObject.SetActive(true);
            UpdateNotificationText();

            bool semuaSudahMakan = GameDataManager.Instance != null && GameDataManager.Instance.ApakahSemuaSudahDiberiMakan();
            
            if (Input.GetKeyDown(KeyCode.E) && semuaSudahMakan)
            {
                isOpening = true;
                if (uiNotifText != null) uiNotifText.gameObject.SetActive(false);
            }
        }
        else
        {
            if (playerDiDekatPintu)
            {
                playerDiDekatPintu = false;
                if (uiNotifText != null) uiNotifText.gameObject.SetActive(false);
            }
        }
    }

    private void UpdateNotificationText()
    {
        if (uiNotifText == null) return;

        bool semuaSudahMakan = GameDataManager.Instance != null && GameDataManager.Instance.ApakahSemuaSudahDiberiMakan();

        if (semuaSudahMakan)
        {
            uiNotifText.text = "[TEKAN E] untuk Membuka Pintu";
            uiNotifText.color = Color.white; 
        }
        else
        {
            uiNotifText.text = "Selesaikan seluruh tugas memberi makan hewan!";
            uiNotifText.color = Color.red; 
        }
    }
}