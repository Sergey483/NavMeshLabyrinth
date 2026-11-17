using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

// Рівень 2: охоронець з патрулем по 4 кутах і рухома стіна з NavMesh Obstacle (Carve).
public static class DynamicObstacleBuilder
{
    [MenuItem("Labyrinth/2. Add Guard + Dynamic Obstacle")]
    public static void AddDynamicObstacle()
    {
        Scene scene = EditorSceneManager.OpenScene(LabyrinthBuilder.ScenePath);

        Material guardMat = LabyrinthBuilder.MakeMaterial("Guard", new Color(0.9f, 0.25f, 0.2f));
        Material obstacleMat = LabyrinthBuilder.MakeMaterial("MovingWall", new Color(0.95f, 0.75f, 0.15f));

        Transform route = new GameObject("Waypoints").transform;
        Vector2Int[] corners = { new Vector2Int(1, 1), new Vector2Int(1, 11), new Vector2Int(9, 11), new Vector2Int(9, 1) };
        Transform[] points = new Transform[corners.Length];
        for (int i = 0; i < corners.Length; i++)
        {
            points[i] = new GameObject("Waypoint_" + (i + 1)).transform;
            points[i].SetParent(route, false);
            points[i].position = LabyrinthBuilder.CellPosition(corners[i].x, corners[i].y);
        }

        GameObject guard = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        guard.name = "Guard";
        guard.transform.position = points[1].position + Vector3.up;
        guard.GetComponent<Renderer>().sharedMaterial = guardMat;
        Object.DestroyImmediate(guard.GetComponent<Collider>());
        NavMeshAgent agent = guard.AddComponent<NavMeshAgent>();
        agent.speed = 3.5f;
        agent.radius = 0.5f;

        WaypointPatrol patrol = guard.AddComponent<WaypointPatrol>();
        SerializedObject patrolObject = new SerializedObject(patrol);
        SerializedProperty waypoints = patrolObject.FindProperty("waypoints");
        waypoints.arraySize = points.Length;
        for (int i = 0; i < points.Length; i++)
            waypoints.GetArrayElementAtIndex(i).objectReferenceValue = points[i];
        patrolObject.ApplyModifiedPropertiesWithoutUndo();

        // Стіна ходить уздовж центрального коридору й перекриває його.
        GameObject movingWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        movingWall.name = "MovingWall";
        movingWall.transform.position = LabyrinthBuilder.CellPosition(5, 6) + Vector3.up * 1.5f;
        movingWall.transform.localScale = new Vector3(0.8f, 3f, LabyrinthBuilder.Cell);
        movingWall.GetComponent<Renderer>().sharedMaterial = obstacleMat;

        NavMeshObstacle obstacle = movingWall.AddComponent<NavMeshObstacle>();
        obstacle.shape = NavMeshObstacleShape.Box;
        obstacle.carving = true;
        obstacle.carveOnlyStationary = false;

        MovingObstacle mover = movingWall.AddComponent<MovingObstacle>();
        SerializedObject moverObject = new SerializedObject(mover);
        moverObject.FindProperty("offset").vector3Value = new Vector3(LabyrinthBuilder.Cell * 3f, 0f, 0f);
        moverObject.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.SaveScene(scene);
        Debug.Log("[LabyrinthBuilder] Guard and dynamic obstacle added");
    }
}
