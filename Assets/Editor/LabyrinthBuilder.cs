using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

// Рівень 1: лабіринт зі стін, запечений NavMesh і гравець, що йде за курсором.
public static class LabyrinthBuilder
{
    internal const string ScenePath = "Assets/Scenes/Labyrinth.unity";

    // 1 — стіна, 0 — коридор. Кожна клітинка 4 × 4 метри.
    internal static readonly string[] Maze =
    {
        "1111111111111",
        "1000001000001",
        "1011101011101",
        "1010001010001",
        "1010111010111",
        "1000000000001",
        "1110101111101",
        "1000100000101",
        "1011111110101",
        "1000000010001",
        "1111111111111",
    };

    internal const float Cell = 4f;

    internal static Vector3 Origin => new Vector3(-(Maze[0].Length - 1) * Cell * 0.5f, 0f, (Maze.Length - 1) * Cell * 0.5f);

    [MenuItem("Labyrinth/1. Build Level 1 (maze + mouse)")]
    public static void BuildLevel1()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        Material floorMat = MakeMaterial("Floor", new Color(0.55f, 0.6f, 0.55f));
        Material wallMat = MakeMaterial("Wall", new Color(0.35f, 0.38f, 0.45f));
        Material playerMat = MakeMaterial("Player", new Color(0.2f, 0.55f, 0.95f));

        int rows = Maze.Length;
        int columns = Maze[0].Length;

        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Floor";
        floor.transform.localScale = new Vector3(columns * Cell / 10f, 1f, rows * Cell / 10f);
        floor.GetComponent<Renderer>().sharedMaterial = floorMat;
        floor.isStatic = true;

        Transform walls = new GameObject("Walls").transform;
        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                if (Maze[row][column] != '1')
                    continue;

                GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall.name = $"Wall_{row}_{column}";
                wall.transform.SetParent(walls, false);
                wall.transform.position = CellPosition(row, column) + Vector3.up * 1.5f;
                wall.transform.localScale = new Vector3(Cell, 3f, Cell);
                wall.GetComponent<Renderer>().sharedMaterial = wallMat;
                wall.isStatic = true;
            }
        }

        GameObject navigation = new GameObject("Navigation");
        NavMeshSurface surface = navigation.AddComponent<NavMeshSurface>();
        surface.collectObjects = CollectObjects.All;
        surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
        BakeAndSave(surface, "Assets/Scenes/Labyrinth_NavMesh.asset");

        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.tag = "Player";
        player.transform.position = CellPosition(1, 1) + Vector3.up;
        player.GetComponent<Renderer>().sharedMaterial = playerMat;
        Object.DestroyImmediate(player.GetComponent<Collider>());
        NavMeshAgent agent = player.AddComponent<NavMeshAgent>();
        agent.speed = 5f;
        agent.angularSpeed = 720f;
        agent.acceleration = 20f;
        agent.radius = 0.5f;
        agent.stoppingDistance = 0.1f;
        player.AddComponent<MouseTracker>();

        Camera camera = Camera.main;
        camera.transform.position = new Vector3(0f, 40f, -14f);
        camera.transform.rotation = Quaternion.Euler(70f, 0f, 0f);

        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        Debug.Log("[LabyrinthBuilder] Level 1 built");
    }

    internal static Vector3 CellPosition(int row, int column)
    {
        return Origin + new Vector3(column * Cell, 0f, -row * Cell);
    }

    internal static void BakeAndSave(NavMeshSurface surface, string path)
    {
        surface.BuildNavMesh();
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(surface.navMeshData, path);
    }

    internal static Material MakeMaterial(string name, Color color)
    {
        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            AssetDatabase.CreateFolder("Assets", "Materials");

        string path = "Assets/Materials/" + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("Standard"));
            AssetDatabase.CreateAsset(material, path);
        }

        material.color = color;
        EditorUtility.SetDirty(material);
        return material;
    }
}
