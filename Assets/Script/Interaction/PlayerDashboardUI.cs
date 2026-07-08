using UnityEngine;
using TMPro;

public class PlayerDashboardUI : MonoBehaviour
{
    public static PlayerDashboardUI Instance;

    [Header("Komponen UI TextMeshPro")]
    public TextMeshProUGUI txtSapi;
    public TextMeshProUGUI txtKuda;
    public TextMeshProUGUI txtHarimau;

    [Header("Pengaturan Tombol Menu")]
    [Tooltip("Tombol yang digunakan untuk membuka/menutup Dashboard Canvas")]
    public KeyCode tombolDashboard = KeyCode.Tab;

    private Canvas canvasKomponen;
    private bool isDashboardOpen = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        
        // Mengambil komponen Canvas yang ada di objek ini
        canvasKomponen = GetComponent<Canvas>();
    }

    void Start()
    {
        // Matikan Canvas di awal game biar bersih dan tidak menghalangi layar
        if (canvasKomponen != null)
        {
            canvasKomponen.enabled = false;
        }

        // Sinkronisasi teks visual checklist
        UpdateDashboardVisual();
    }

    void Update()
    {
        // Deteksi input tombol dari player (Menekan tombol TAB)
        if (Input.GetKeyDown(tombolDashboard))
        {
            isDashboardOpen = !isDashboardOpen;
            
            if (canvasKomponen != null)
            {
                canvasKomponen.enabled = isDashboardOpen;
            }
        }
    }

    // Fungsi utama untuk mencoret teks misi
    public void UpdateDashboardVisual()
    {
        if (GameDataManager.Instance == null) return;

        // Cek status Sapi
        if (txtSapi != null)
        {
            txtSapi.text = GameDataManager.Instance.sudahKunjungiSapi ? "<s>[✓] Kunjungi Kandang Sapi (Selesai)</s>" : "[ ] Kunjungi Kandang Sapi";
            txtSapi.color = GameDataManager.Instance.sudahKunjungiSapi ? Color.green : Color.white;
        }

        // Cek status Kuda
        if (txtKuda != null)
        {
            txtKuda.text = GameDataManager.Instance.sudahKunjungiKuda ? "<s>[✓] Kunjungi Kandang Kuda (Selesai)</s>" : "[ ] Kunjungi Kandang Kuda";
            txtKuda.color = GameDataManager.Instance.sudahKunjungiKuda ? Color.green : Color.white;
        }

        // Cek status Harimau
        if (txtHarimau != null)
        {
            txtHarimau.text = GameDataManager.Instance.sudahKunjungiHarimau ? "<s>[✓] Kunjungi Kandang Harimau (Selesai)</s>" : "[ ] Kunjungi Kandang Harimau";
            txtHarimau.color = GameDataManager.Instance.sudahKunjungiHarimau ? Color.green : Color.white;
        }
    }
}