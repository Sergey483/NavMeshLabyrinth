using UnityEngine;

// Рухає стіну туди й назад. NavMesh Obstacle з Carve вирізає її з NavMesh у реальному часі.
public class MovingObstacle : MonoBehaviour
{
    [SerializeField] private Vector3 offset = new Vector3(8f, 0f, 0f);
    [SerializeField, Min(0.1f)] private float cycleTime = 6f;

    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        float t = Mathf.PingPong(Time.time / cycleTime * 2f, 1f);
        transform.position = startPosition + offset * Mathf.SmoothStep(0f, 1f, t);
    }
}
