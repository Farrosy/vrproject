using UnityEngine;
using System.Collections;

public class ZoneTriggerDetector : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Masukkan objek yang ingin di-aktifkan/matikan (bisa objek itu sendiri atau parent terluarnya)")]
    public GameObject targetObjectToToggle;

    private void Start()
    {
        // [FIX UTAMA]: Gunakan Coroutine untuk memastikan objek dimatikan SETELAH seluruh script hewan selesai melakukan Setup awal
        if (targetObjectToToggle != null)
        {
            StartCoroutine(InitialDisableRoutine());
        }
    }

    private IEnumerator InitialDisableRoutine()
    {
        // Tunggu hingga akhir frame pertama selesai diproses oleh Unity
        yield return new WaitForEndOfFrame();
        
        // Eksekusi pematikan mutlak dari luar boks collider
        if (targetObjectToToggle != null)
        {
            targetObjectToToggle.SetActive(false);
            Debug.Log("[Zone Trigger] Inisialisasi Awal: Memaksa " + targetObjectToToggle.name + " untuk non-aktif.");
        }
    }

    // Fungsi saat objek masuk ke dalam Collider Zona
    private void OnTriggerEnter(Collider other)
    {
        // Memastikan yang masuk adalah Player (sesuaikan dengan tag player kamu)
        if (other.CompareTag("Player") || other.GetComponent<FirstPersonController>() != null)
        {
            if (targetObjectToToggle != null)
            {
                targetObjectToToggle.SetActive(true);
                Debug.Log(targetObjectToToggle.name + " AKTIF karena Player masuk zona.");
            }
        }
    }

    // Fungsi saat objek keluar dari Collider Zona
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<FirstPersonController>() != null)
        {
            if (targetObjectToToggle != null)
            {
                targetObjectToToggle.SetActive(false);
                Debug.Log(targetObjectToToggle.name + " NON-AKTIF karena Player keluar zona.");
            }
        }
    }
}