using UnityEngine;

public class KandangTrigger : MonoBehaviour
{
    // Pilihan tipe kandang di Inspector Unity
    public enum TipeKandang { Sapi, Harimau, Kuda }
    public TipeKandang jenisKandangIni;

    private bool sudahTercatat = false;

    // Trigger terdeteksi ketika player masuk area kandang
    void OnTriggerEnter(Collider other)
    {
        // Pastikan objek yang masuk memiliki tag "Player"
        if (other.CompareTag("Player") && !sudahTercatat)
        {
            LogikaKunjungan();
        }
    }

    void LogikaKunjungan()
    {
        if (GameDataManager.Instance == null) return;

        // Mengubah data di GameDataManager sesuai jenis kandang ini
        switch (jenisKandangIni)
        {
            case TipeKandang.Sapi:
                GameDataManager.Instance.sudahKunjungiSapi = true;
                Debug.Log("Kandang Sapi berhasil dikunjungi!");
                break;
            case TipeKandang.Harimau:
                GameDataManager.Instance.sudahKunjungiHarimau = true;
                Debug.Log("Kandang Harimau berhasil dikunjungi!");
                break;
            case TipeKandang.Kuda:
                GameDataManager.Instance.sudahKunjungiKuda = true;
                Debug.Log("Kandang Kuda berhasil dikunjungi!");
                break;
        }

        sudahTercatat = true; // Agar tidak trigger berkali-kali

        // CONTOH GAME FLOW: Cek apakah tugas selesai
        if (GameDataManager.Instance.CekSemuaKandangSelesai())
        {
            Debug.Log("Hebat! Semua kandang sudah dikunjungi. Lempar info ini ke Game Flow utama!");
            // Di sini kalian bisa memanggil fungsi Game Selesai atau memunculkan UI tertentu
        }
    }
}