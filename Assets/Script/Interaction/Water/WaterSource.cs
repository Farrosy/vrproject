using UnityEngine;

public class WaterSource : MonoBehaviour
{
    [Header("Water Settings")]
    [Tooltip("ID unik untuk memvalidasi bahwa objek ini adalah ember air")]
    [SerializeField] private string _bucketTagKeyword = "Bucket";
    
    [Header("Key Settings")]
    [SerializeField] private KeyCode _takeWaterKey = KeyCode.F; // Tombol F untuk mengambil air

    private bool playerInsideTrigger = false;
    private GameObject bucketInTrigger = null;

    private void Update()
    {
        // Jika player/ember berada di area kolam dan menekan tombol F
        if (playerInsideTrigger && Input.GetKeyDown(_takeWaterKey) && bucketInTrigger != null)
        {
            Transform waterChild = bucketInTrigger.transform.Find("Water");
            if (waterChild != null)
            {
                if (!waterChild.gameObject.activeSelf)
                {
                    waterChild.gameObject.SetActive(true);
                    Debug.Log("Ember berhasil diisi air menggunakan tombol [F]!");
                }
                else
                {
                    Debug.Log("Ember sudah penuh dengan air!");
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Deteksi apakah objek yang mendekat adalah ember (baik dipegang atau dilepas)
        Droppable droppable = other.GetComponentInParent<Droppable>();
        if (droppable != null && droppable.gameObject.name.Contains(_bucketTagKeyword))
        {
            playerInsideTrigger = true;
            bucketInTrigger = droppable.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Droppable droppable = other.GetComponentInParent<Droppable>();
        if (droppable != null && droppable.gameObject.name.Contains(_bucketTagKeyword))
        {
            playerInsideTrigger = false;
            bucketInTrigger = null;
        }
    }
}