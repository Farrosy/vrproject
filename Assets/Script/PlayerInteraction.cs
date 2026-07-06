using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionDistance = 3f; 
    public LayerMask interactableLayer;    

    private Transform cameraTransform;
    private FeedingTrough lastTargetTrough; 
    private WaterTrough lastWaterTrough; 
    private BasicVRInteractionController vrController; 

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
            // 1. CEK JIKA MENGARAH KE TEMPAT MAKAN (JERAMI)
            FeedingTrough trough = hit.collider.GetComponentInParent<FeedingTrough>();
            if (trough != null)
            {
                if (trough != lastTargetTrough) { ClearLastHighlight(); lastTargetTrough = trough; }

                bool isHoldingSomething = IsPlayerHoldingObject();

                if (!trough.IsFull())
                {
                    trough.ToggleHighlight(true);
                    if (trough.interactionText != null)
                    {
                        trough.interactionText.text = isHoldingSomething ? "Tekan [E] untuk Menjatuhkan Makanan" : "Bawa Hay Bale ke Sini!";
                        trough.interactionText.gameObject.SetActive(true);
                    }
                }
                return; 
            }

            // 2. CEK JIKA MENGARAH KE TEMPAT MINUM (EMBER AIR)
            WaterTrough waterTrough = hit.collider.GetComponentInParent<WaterTrough>();
            if (waterTrough != null)
            {
                if (waterTrough != lastWaterTrough) { ClearLastHighlight(); lastWaterTrough = waterTrough; }

                bool isHoldingSomething = IsPlayerHoldingObject();

                if (!waterTrough.IsFull())
                {
                    waterTrough.ToggleHighlight(true);
                    if (waterTrough.interactionText != null)
                    {
                        waterTrough.interactionText.text = isHoldingSomething ? "Tekan [F] untuk Menuangkan Air" : "Bawa Ember Berisi Air ke Sini!";
                        waterTrough.interactionText.gameObject.SetActive(true);
                    }

                    // [FIXED]: Ambil objek langsung dari VR Controller (Anti-Gagal Struktur Hierarchy)
                    if (isHoldingSomething && Input.GetKeyDown(KeyCode.F))
                    {
                        if (vrController != null)
                        {
                            GameObject heldBucket = vrController.GetHeldGameObject();
                            if (heldBucket != null)
                            {
                                waterTrough.PourWater(heldBucket);
                            }
                        }
                    }
                }
                return;
            }
        }

        ClearLastHighlight();
    }

    // ==================== FIX: SINKRONISASI CEK BARANG GENGGAMAN ====================
    private bool IsPlayerHoldingObject()
    {
        if (vrController == null) return false;
        return vrController.GetHeldGameObject() != null;
    }
    // ================================================================================

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

        if (lastWaterTrough != null)
        {
            lastWaterTrough.ToggleHighlight(false);
            if (lastWaterTrough.interactionText != null)
            {
                lastWaterTrough.interactionText.gameObject.SetActive(false);
            }
            lastWaterTrough = null;
        }
    }
}