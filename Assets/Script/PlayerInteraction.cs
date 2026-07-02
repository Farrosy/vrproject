using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionDistance = 3f; 
    public LayerMask interactableLayer;    
    
    // UI Reference dihapus dari sini karena sudah dihandle oleh wadah masing-masing

    private Transform cameraTransform;
    private FeedingTrough lastTargetTrough; 

    void Start()
    {
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
            FeedingTrough trough = hit.collider.GetComponent<FeedingTrough>();

            if (trough != null)
            {
                if (trough != lastTargetTrough)
                {
                    ClearLastHighlight(); 
                    lastTargetTrough = trough;
                }

                if (!trough.IsFull())
                {
                    // Wadah ini yang akan menyalakan material GLOW dan TEXT miliknya sendiri
                    trough.ToggleHighlight(true);

                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        trough.FillOneFood();
                    }
                }
                else
                {
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
            // Mematikan visual & teks pada wadah yang ditinggalkan
            lastTargetTrough.ToggleHighlight(false);
            lastTargetTrough = null;
        }
    }
}