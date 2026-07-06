using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionDistance = 3f; 
    public LayerMask interactableLayer;    

    private Transform cameraTransform;
    private FeedingTrough lastTargetTrough; 
    private BasicVRInteractionController vrController; // Referensi ke controller tangan/grab

    void Start()
    {
        vrController = GetComponent<BasicVRInteractionController>();

        FirstPersonController fpsController = GetComponent<FirstPersonController>();
        if (fpsController != null && fpsController.cameraTransform != null)
        {
            cameraTransform = fpsController.cameraTransform;
        }
        else
        {
            cameraTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        RaycastHit hit;
        
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, interactionDistance, interactableLayer))
        {
            // Ambil FeedingTrough baik di objek langsung atau di parent-nya
            FeedingTrough trough = hit.collider.GetComponentInParent<FeedingTrough>();

            if (trough != null)
            {
                if (trough != lastTargetTrough)
                {
                    ClearLastHighlight(); 
                    lastTargetTrough = trough;
                }

                // Cek apakah player saat ini sedang memegang sesuatu lewat VR Controller
                bool isHoldingSomething = (vrController != null && vrController.transform.Find("HoldPoint") != null && vrController.transform.Find("HoldPoint").childCount > 0);

                if (!trough.IsFull())
                {
                    // Nyalakan highlight material kuning pada wadah
                    trough.ToggleHighlight(true);

                    // ATUR NOTIFIKASI TEKS DINAMIS
                    if (trough.interactionText != null)
                    {
                        if (isHoldingSomething)
                        {
                            // Jika sedang bawa makanan, beri tahu tombol drop (sesuai KeyCode_dropKey di controller-mu yaitu Q)
                            trough.interactionText.text = "Tekan [Q] untuk Menjatuhkan Makanan";
                        }
                        else
                        {
                            // Jika tangan kosong, ingatkan player untuk ambil jerami dulu
                            trough.interactionText.text = "Bawa Hay Bale ke Sini!";
                        }
                        trough.interactionText.gameObject.SetActive(true);
                    }
                }
                else
                {
                    // Jika wadah penuh, matikan teks instruksi pengisian
                    if (trough.interactionText != null) trough.interactionText.gameObject.SetActive(false);
                    trough.ToggleHighlight(false);
                }
                
                return; 
            }
        }

        ClearLastHighlight();
    }

    private void ClearLastHighlight()
    {
        if (lastTargetTrough != null)
        {
            lastTargetTrough.ToggleHighlight(false);
            if (lastTargetTrough.interactionText != null)
            {
                lastTargetTrough.interactionText.gameObject.SetActive(false);
            }
            lastTargetTrough = null;
        }
    }
}