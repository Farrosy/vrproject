using UnityEngine;
using System.Collections;

public class GateAudioTrigger : MonoBehaviour
{
    [Header("Audio Settings")]
    [Tooltip("Tarik komponen Audio Source yang menempel di objek ini")]
    [SerializeField] private AudioSource _gateAudioSource;
    
    [Tooltip("Durasi berapa detik audio akan menyala sebelum dimatikan (Set ke 10)")]
    [SerializeField] private float _playDuration = 10f;

    [Header("Trigger Settings")]
    [Tooltip("Jika dicentang, audio hanya akan berputar 1 kali saja seumur hidup game")]
    [SerializeField] private bool _triggerOnlyOnce = true;

    private bool _hasTriggered = false;

    private void Start()
    {
        // Pastikan di awal game audio source dalam kondisi mati
        if (_gateAudioSource != null)
        {
            _gateAudioSource.Stop();
            _gateAudioSource.loop = false; // Matikan loop agar tidak mengulang sendiri
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Pastikan objek yang masuk adalah Player (sesuaikan tag dengan player kamu)
        if (other.CompareTag("Player") || other.GetComponent<FirstPersonController>() != null)
        {
            // Proteksi jika di-set hanya boleh bunyi sekali
            if (_triggerOnlyOnce && _hasTriggered) return;

            if (_gateAudioSource != null)
            {
                _hasTriggered = true;
                // Mulai jalankan audio beserta timer 10 detik
                StartCoroutine(PlayAudioWithTimer());
            }
        }
    }

    private IEnumerator PlayAudioWithTimer()
    {
        Debug.Log("Player masuk gate! Audio mulai berputar.");
        _gateAudioSource.Play();

        // Tunggu selama 10 detik sesuai instruksi di Inspector
        yield return new WaitForSeconds(_playDuration);

        // Setelah 10 detik, matikan audionya
        _gateAudioSource.Stop();
        Debug.Log("10 detik selesai, audio dimatikan.");
    }
}