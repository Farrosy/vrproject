using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance;

    [Header("Status Kunjungan Kandang")]
    public bool sudahKunjungiSapi = false;
    public bool sudahKunjungiKuda = false;
    public bool sudahKunjungiHarimau = false;

    [Header("Status Memberi Makan")]
    public static bool sudahMakanSapi = false;
    public static bool sudahMakanKuda = false;
    public static bool sudahMakanHarimau = false;

    private void Awake()
    {
        // Sistem Singleton agar script ini mudah diakses dari script mana pun
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Fungsi baru untuk mengecek apakah 3 kandang sudah diberi makan semuanya
    // Fungsi ini mengembalikan nilai true/false yang akan dibaca oleh skrip pintu
    public bool ApakahSemuaSudahDiberiMakan()
    {
        return sudahMakanSapi && sudahMakanKuda && sudahMakanHarimau;
    }
}