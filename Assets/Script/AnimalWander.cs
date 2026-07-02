using UnityEngine;
using UnityEngine.AI;

public class AnimalWander : MonoBehaviour
{
    public float wanderRadius = 10f; 
    public float wanderTimer = 5f;   

    private NavMeshAgent agent;
    private float timer;
    private bool isEating = false; 

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
        if (isEating) return;

        timer += Time.deltaTime;

        if (timer >= wanderTimer)
        {
            Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1);
            agent.SetDestination(newPos);
            timer = 0;
        }
    }

    public void GoToFeeder(Vector3 feederPosition)
    {
        isEating = true; 
        agent.SetDestination(feederPosition); 
        Debug.Log(gameObject.name + " mencium bau makanan dan mendekat!");
    }

    // ==================== FUNGSI BARU UNTUK SAPI BISA JALAN LAGI ====================
    public void ResumeWandering()
    {
        isEating = false;
        timer = wanderTimer; // Paksa timer langsung penuh agar sapi langsung mencari koordinat wander baru
        Debug.Log(gameObject.name + " makanan habis, kembali berkeliling!");
    }
    // ================================================================================

    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;

        NavMeshHit navHit;
        NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);

        return navHit.position;
    }
}