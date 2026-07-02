using UnityEngine;

public class FeedingTrough : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject feedCanvas; 

    [Header("Food & Animals Settings")]
    public GameObject[] foodMeshes; 
    public AnimalWander[] targetAnimals; // Kembali pakai AnimalWander
    
    private bool isPlayerNearby = false;
    private int currentFoodIndex = 0; 

    void Start()
    {
        if (feedCanvas == null)
        {
            Canvas childCanvas = GetComponentInChildren<Canvas>(true);
            if (childCanvas != null)
            {
                feedCanvas = childCanvas.gameObject;
            }
        }

        if (feedCanvas != null)
        {
            feedCanvas.SetActive(false);
        }
    }

    void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E) && currentFoodIndex < foodMeshes.Length)
        {
            FillOneFood();
        }

        if (isPlayerNearby && feedCanvas != null)
        {
            feedCanvas.transform.LookAt(feedCanvas.transform.position + Camera.main.transform.forward);
        }
    }

    void FillOneFood()
    {
        foodMeshes[currentFoodIndex].SetActive(true);
        Debug.Log("Mengisi jerami ke-" + (currentFoodIndex + 1));
        
        if (currentFoodIndex == 0 && targetAnimals != null)
        {
            foreach (AnimalWander cow in targetAnimals)
            {
                if (cow != null)
                {
                    cow.GoToFeeder(transform.position);
                }
            }
        }

        currentFoodIndex++;

        if (currentFoodIndex >= foodMeshes.Length)
        {
            Debug.Log("Tempat makanan sudah penuh!");
            if (feedCanvas != null) feedCanvas.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && currentFoodIndex < foodMeshes.Length)
        {
            isPlayerNearby = true;
            if (feedCanvas != null) feedCanvas.SetActive(true); 
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if (feedCanvas != null) feedCanvas.SetActive(false); 
        }
    }
}