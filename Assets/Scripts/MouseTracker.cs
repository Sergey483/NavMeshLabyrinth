using UnityEngine;
using UnityEngine.AI;

// Гравець постійно йде за курсором миші по коридорах лабіринту.
[RequireComponent(typeof(NavMeshAgent))]
public class MouseTracker : MonoBehaviour
{
    [SerializeField] private LayerMask groundMask = ~0;
    [SerializeField, Min(0.02f)] private float updateInterval = 0.1f;

    private NavMeshAgent agent;
    private Camera mainCamera;
    private float nextUpdateTime;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (mainCamera == null || Time.time < nextUpdateTime)
            return;

        nextUpdateTime = Time.time + updateInterval;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, 200f, groundMask))
            return;

        // Точка під курсором може бути на стіні — беремо найближчу точку NavMesh.
        if (NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, 2f, NavMesh.AllAreas))
            agent.SetDestination(navHit.position);
    }
}
