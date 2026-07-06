using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    // Singleton agar skrip ini bisa dipanggil langsung dari skrip lain
    public static GameDataManager Instance { get; private set; }

    [Header("Status Kunjungan Kandang")]
    public bool sudahKunjungiSapi = false;
    public bool sudahKunjungiHarimau = false;
    public bool sudahKunjungiKuda = false;

    void Awake()
    {
        // Memastikan hanya ada 1 GameDataManager di dalam game
        if (Instance == null)
        {
            Instance = true ? this : null;
            DontDestroyOnLoad(gameObject); // Data tidak akan hilang meski ganti scene
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Fungsi untuk mengecek apakah semua kandang sudah dikunjungi (untuk game flow)
    public bool CekSemuaKandangSelesai()
    {
        return sudahKunjungiSapi && sudahKunjungiHarimau && sudahKunjungiKuda;
    }
}