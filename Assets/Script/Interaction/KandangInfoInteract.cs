using UnityEngine;

public class KandangInfoInteract : MonoBehaviour
{
    public enum TipeKandang { Sapi, Harimau, Kuda }
    public TipeKandang jenisKandangIni;

    private bool sudahDikunjungi = false;

    // Fungsi ini yang akan dipanggil saat papan info diinteraksi (di-klik/di-trigger)
    public void TriggerKunjunganPapan()
    {
        if (sudahDikunjungi || GameDataManager.Instance == null) return;

        switch (jenisKandangIni)
        {
            case TipeKandang.Sapi:
                GameDataManager.Instance.sudahKunjungiSapi = true;
                Debug.Log("Data Terupdate: Papan Info Sapi telah dibaca.");
                break;
            case TipeKandang.Harimau:
                GameDataManager.Instance.sudahKunjungiHarimau = true;
                Debug.Log("Data Terupdate: Papan Info Harimau telah dibaca.");
                break;
            case TipeKandang.Kuda:
                GameDataManager.Instance.sudahKunjungiKuda = true;
                Debug.Log("Data Terupdate: Papan Info Kuda telah dibaca.");
                break;
        }

        sudahDikunjungi = true;

        // Langsung cek apakah syarat 3 kandang sudah terpenuhi untuk buka pintu exit
        CekLogikaBukaPintu();
    }

    void CekLogikaBukaPintu()
    {
        if (GameDataManager.Instance.CekSemuaKandangSelesai())
        {
            Debug.Log("Syarat Terpenuhi! Membuka Pintu Exit...");
            
            // MENCARI OBJEK PINTU EXIT DAN MEMBUKANYA
            GameObject pintu = GameObject.FindGameObjectWithTag("PintuExit");
            if (pintu != null)
            {
                // Alternatif 1: Matikan objek pintu biar langsung hilang/terbuka
                pintu.SetActive(false); 
                
                // Alternatif 2: Jika pintu ada animasi, kamu bisa panggil trigger animasinya di sini
                // pintu.GetComponent<Animator>().SetTrigger("BukaPintu");
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TriggerKunjunganPapan();
        }
    }
}