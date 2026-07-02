using UnityEngine;

public class FeedingTrough : MonoBehaviour
{
    public GameObject[] foodMeshes; 
    // SEKARANG MENGGUNAKAN [] AGAR BISA MENAMPUNG BANYAK SAPI SEKALIGUS
    public AnimalWander[] targetAnimals; 
    
    private bool isPlayerNearby = false;
    private int currentFoodIndex = 0; 

    void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E) && currentFoodIndex < foodMeshes.Length)
        {
            FillOneFood();
        }
    }

    void FillOneFood()
    {
        foodMeshes[currentFoodIndex].SetActive(true);
        Debug.Log("Mengisi jerami ke-" + (currentFoodIndex + 1));
        
        // Saat makanan pertama kali diisi, panggil SEMUA sapi di dalam daftar
        if (currentFoodIndex == 0 && targetAnimals != null)
        {
            // Melakukan perulangan (loop) untuk menyuruh setiap sapi bergerak
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
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
        }
    }
}