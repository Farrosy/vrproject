using UnityEngine;

public class SapiFeeding : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject feedCanvas; // Tarik objek FeedCanvas ke sini nanti

    [Header("Feeding Settings")]
    public float interactionDistance = 3f; // Jarak maksimal untuk interaksi
    
    private Transform playerTransform;
    private bool isPlayerNearby = false;

    void Start()
    {
        // Mencari objek Player di dalam scene berdasarkan Tag
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // Pastikan canvas mati di awal game
        if (feedCanvas != null)
        {
            feedCanvas.SetActive(false);
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        // Hitung jarak antara Sapi dan Player
        float distance = Vector3.Distance(transform.position, playerTransform.position);

        // Jika player dekat dengan sapi
        if (distance <= interactionDistance)
        {
            if (!isPlayerNearby)
            {
                isPlayerNearby = true;
                if (feedCanvas != null) feedCanvas.SetActive(true); // Munculkan teks
            }

            // Jalankan fungsi kasih makan kalau player pencet tombol E
            if (Input.GetKeyDown(KeyCode.E))
            {
                KasihMakanSapi();
            }
        }
        else
        {
            // Jika player menjauh
            if (isPlayerNearby)
            {
                isPlayerNearby = false;
                if (feedCanvas != null) feedCanvas.SetActive(false); // Sembunyikan teks
            }
        }

        // Opsional: Membuat Canvas selalu menghadap ke arah kamera player (Billboard effect)
        if (isPlayerNearby && feedCanvas != null)
        {
            feedCanvas.transform.LookAt(feedCanvas.transform.position + Camera.main.transform.forward);
        }
    }

    void KasihMakanSapi()
    {
        Debug.Log("Sapi telah diberi makan jerami!");
        
        // TODO: Masukkan logika animasi sapi makan atau trigger tumpukan jerami berkurang di sini
    }
}