using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance;

    [Header("Status Kunjungan Kandang")]
    public bool sudahKunjungiSapi = false;
    public bool sudahKunjungiKuda = false;
    public bool sudahKunjungiHarimau = false;

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
}