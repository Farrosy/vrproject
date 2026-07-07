using UnityEngine;

public class KandangTriggerZone : MonoBehaviour
{
    public enum TipeKandang { Sapi, Kuda, Harimau }
    
    [Header("Pengaturan Area Kandang")]
    public TipeKandang jenisKandangIni;

    private void OnTriggerEnter(Collider other)
    {
        // Deteksi apakah yang masuk ke area trigger adalah Player
        // Kamu bisa sesuaikan tag-nya atau menggunakan pengecekan komponen controller player
        if (other.CompareTag("Player") || other.GetComponent<FirstPersonController>() != null || other.name.Contains("CharacterPlayer"))
        {
            if (GameDataManager.Instance == null) return;

            // Catat data kunjungan ke database global
            switch (jenisKandangIni)
            {
                case TipeKandang.Sapi:
                    GameDataManager.Instance.sudahKunjungiSapi = true;
                    Debug.Log("[Dashboard] Player mengunjungi kandang Sapi.");
                    break;
                case TipeKandang.Kuda:
                    GameDataManager.Instance.sudahKunjungiKuda = true;
                    Debug.Log("[Dashboard] Player mengunjungi kandang Kuda.");
                    break;
                case TipeKandang.Harimau:
                    GameDataManager.Instance.sudahKunjungiHarimau = true;
                    Debug.Log("[Dashboard] Player mengunjungi kandang Harimau.");
                    break;
            }

            // Cari script UI Dashboard melayang untuk memperbarui teksnya seketika
            PlayerDashboardUI dashboardUI = FindObjectOfType<PlayerDashboardUI>();
            if (dashboardUI != null)
            {
                dashboardUI.UpdateDashboardVisual();
            }
        }
    }
}