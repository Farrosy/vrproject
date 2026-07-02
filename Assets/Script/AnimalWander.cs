using UnityEngine;
using UnityEngine.AI;

public class AnimalWander : MonoBehaviour
{
    public float wanderRadius = 10f; 
    public float wanderTimer = 5f;   

    private NavMeshAgent agent;
    private float timer;
    private bool isEating = false; // Status apakah sapi sedang makan

    void OnEnable()
    {
        agent = GetComponent<NavMeshAgent>();
        timer = wanderTimer;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 2.0f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }
    }

    void Update()
    {
        // Jika sedang makan, jangan cari koordinat acak (diam di tempat makan)
        if (isEating) return;

        timer += Time.deltaTime;

        if (timer >= wanderTimer)
        {
            Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1);
            agent.SetDestination(newPos);
            timer = 0;
        }
    }

    // Fungsi baru untuk menyuruh sapi mendatangi tempat makan
    public void GoToFeeder(Vector3 feederPosition)
    {
        isEating = true; 
        agent.SetDestination(feederPosition); // Sapi berjalan ke arah tempat makan
        Debug.Log(gameObject.name + " mencium bau makanan dan mendekat!");
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;

        NavMeshHit navHit;
        NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);

        return navHit.position;
    }
}