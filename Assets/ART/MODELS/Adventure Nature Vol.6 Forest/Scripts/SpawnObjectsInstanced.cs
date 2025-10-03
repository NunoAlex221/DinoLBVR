using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[ExecuteInEditMode]
public class SpawnObjectsInstanced : MonoBehaviour
{
    public float spawnRadius = 10f;
    public Mesh mesh;
    public Material material;
    public string targetTag = "Ground";

    [Header("Random Y Rotation")]
    public float minYRotation = 0f;
    public float maxYRotation = 360f;

    [Header("Random Scale")]
    public float minScale = 0.8f;
    public float maxScale = 1.2f;

    [Header("Culling")]
    public float cullDistance = 50f;

    [Header("Shadows")]
    public bool castShadows = true;

    [Header("Slope Filtering")]
    [Range(0f, 90f)] public float minSlopeAngle = 0f;
    [Range(0f, 90f)] public float maxSlopeAngle = 30f;

    [Header("Poisson Disc Sampling")]
    public float minDistanceBetweenInstances = 1.5f;
    public int poissonAttempts = 30;

    [Header("Normal Averaging")]
    public bool useNormalAveraging = false;
    public float normalSampleRadius = 0.2f;
    public int normalSampleCount = 5;

    [HideInInspector] public List<Matrix4x4> matrices = new();

    public void Spawn()
    {
        Clear();

        if (mesh == null || material == null) return;

        List<Vector2> points = PoissonDiscSample(spawnRadius * 2, minDistanceBetweenInstances, poissonAttempts);
        Vector3 center = transform.position;

        foreach (var pt in points)
        {
            Vector3 worldPos = center + new Vector3(pt.x - spawnRadius, 0, pt.y - spawnRadius);
            if (Physics.Raycast(worldPos + Vector3.up * 50f, Vector3.down, out RaycastHit hit, 100f))
            {
                if (!hit.collider.CompareTag(targetTag)) continue;

                Vector3 normal = useNormalAveraging
                    ? AverageSurfaceNormal(hit.point, normalSampleRadius, normalSampleCount)
                    : hit.normal;

                float angle = Vector3.Angle(normal, Vector3.up);
                if (angle < minSlopeAngle || angle > maxSlopeAngle) continue;

                Quaternion yRot = Quaternion.Euler(0, Random.Range(minYRotation, maxYRotation), 0);
                Quaternion align = Quaternion.LookRotation(Vector3.Cross(Vector3.right, normal), normal);
                Quaternion finalRot = align * yRot;

                float scale = Random.Range(minScale, maxScale);
                Vector3 offset = normal * 0.00f; // optional offset from surface
                Matrix4x4 matrix = Matrix4x4.TRS(hit.point + offset, finalRot, Vector3.one * scale);
                matrices.Add(matrix);
            }
        }

#if UNITY_EDITOR
        SceneView.RepaintAll();
        EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif
    }

    public void Clear()
    {
        matrices.Clear();
#if UNITY_EDITOR
        SceneView.RepaintAll();
        EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }

    void Update()
    {
        if (matrices.Count == 0 || mesh == null || material == null) return;

        Camera cam = null;

        if (Application.isPlaying)
        {
            cam = Camera.main;
        }
#if UNITY_EDITOR
        else
        {
            cam = SceneView.lastActiveSceneView?.camera;
        }
#endif
        if (cam == null) return;

        int batchSize = 1023;
        List<Matrix4x4> visible = new();

        foreach (var matrix in matrices)
        {
            Vector3 pos = matrix.GetColumn(3);
            if (Vector3.Distance(cam.transform.position, pos) <= cullDistance)
                visible.Add(matrix);
        }

        for (int i = 0; i < visible.Count; i += batchSize)
        {
            int count = Mathf.Min(batchSize, visible.Count - i);
            Graphics.DrawMeshInstanced(
                mesh,
                0,
                material,
                visible.GetRange(i, count),
                null,
                castShadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off,
                true
            );
        }
    }

    List<Vector2> PoissonDiscSample(float size, float radius, int attempts)
    {
        float cellSize = radius / Mathf.Sqrt(2);
        int gridSize = Mathf.CeilToInt(size / cellSize);
        Vector2[,] grid = new Vector2[gridSize, gridSize];
        List<Vector2> points = new();
        List<Vector2> spawnPoints = new();

        Vector2 first = new Vector2(size / 2, size / 2);
        spawnPoints.Add(first);
        points.Add(first);
        grid[(int)(first.x / cellSize), (int)(first.y / cellSize)] = first;

        while (spawnPoints.Count > 0)
        {
            int idx = Random.Range(0, spawnPoints.Count);
            Vector2 center = spawnPoints[idx];
            bool found = false;

            for (int i = 0; i < attempts; i++)
            {
                float angle = Random.value * Mathf.PI * 2;
                float dist = Random.Range(radius, 2 * radius);
                Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Vector2 sample = center + dir * dist;

                if (sample.x < 0 || sample.x >= size || sample.y < 0 || sample.y >= size) continue;

                int sx = (int)(sample.x / cellSize);
                int sy = (int)(sample.y / cellSize);
                bool valid = true;

                for (int x = Mathf.Max(0, sx - 2); x <= Mathf.Min(gridSize - 1, sx + 2); x++)
                {
                    for (int y = Mathf.Max(0, sy - 2); y <= Mathf.Min(gridSize - 1, sy + 2); y++)
                    {
                        if (grid[x, y] != Vector2.zero && Vector2.Distance(grid[x, y], sample) < radius)
                        {
                            valid = false;
                            break;
                        }
                    }
                    if (!valid) break;
                }

                if (valid)
                {
                    grid[sx, sy] = sample;
                    points.Add(sample);
                    spawnPoints.Add(sample);
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                spawnPoints.RemoveAt(idx);
            }
        }

        return points;
    }

    Vector3 AverageSurfaceNormal(Vector3 center, float radius, int sampleCount)
    {
        Vector3 totalNormal = Vector3.zero;
        int hits = 0;

        for (int i = 0; i < sampleCount; i++)
        {
            Vector2 randCircle = Random.insideUnitCircle * radius;
            Vector3 offset = new Vector3(randCircle.x, 0, randCircle.y);
            Vector3 origin = center + offset + Vector3.up * 10f;

            if (Physics.Raycast(origin, Vector3.down, out RaycastHit sampleHit, 20f))
            {
                totalNormal += sampleHit.normal;
                hits++;
            }
        }

        return hits > 0 ? (totalNormal / hits).normalized : Vector3.up;
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(SpawnObjectsInstanced))]
    public class SpawnObjectsInstancedEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var spawner = (SpawnObjectsInstanced)target;

            EditorGUILayout.Space();
            if (GUILayout.Button("Spawn Objects Instanced"))
            {
                Undo.RecordObject(spawner, "Spawn Instanced Objects");
                spawner.Spawn();
            }

            if (GUILayout.Button("Clear"))
            {
                Undo.RecordObject(spawner, "Clear Instanced Objects");
                spawner.Clear();
            }
        }
    }
#endif
}
