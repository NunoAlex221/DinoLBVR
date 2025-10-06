namespace AdventureNature.Rendering
{

    using UnityEngine;
    using UnityEditor;

    public class HierarchicalObjectSpawner : MonoBehaviour
    {
        [Header("Spawn Area")]
        public float spawnRadius = 10f;
        public float spawnRadiusLargeObject = 10f;
        public float spawnRadiusMediumObject = 10f;

        [Header("Spawn Tag Settings")]
        public string spawnTag = "Terrain";

        [Header("Slope Filtering")]
        [Range(0f, 90f)] public float minSlopeAngle = 0f;
        [Range(0f, 90f)] public float maxSlopeAngle = 30f;

        [Header("Large Object Prefabs")]
        public GameObject[] largeObjectPrefabs;
        [Range(0f, 100f)]
        public float[] largeObjectWeights;

        [Header("Medium Object Prefabs")]
        public GameObject[] mediumObjectPrefabs;
        [Range(0f, 100f)]
        public float[] mediumObjectWeights;

        [Header("Small Object Prefabs")]
        public GameObject[] smallObjectPrefabs;
        [Range(0f, 100f)]
        public float[] smallObjectWeights;

        [Header("Object Spawn Settings")]
        public int largeObjectCount = 5;
        public int mediumObjectCountPerLarge = 3;
        public int smallObjectCountPerMedium = 5;

        [Header("Size Ranges")]
        public Vector2 largeObjectSizeRange = new Vector2(1f, 2f);
        public Vector2 mediumObjectSizeRange = new Vector2(0.5f, 1f);
        public Vector2 smallObjectSizeRange = new Vector2(0.2f, 0.5f);

        [Header("Rotation Options")]
        public bool alignToGround = false;
        public Vector2 rotationYRange = new Vector2(0f, 360f);
        public bool randomFullRotation = false;
        public Vector2 rotationXRange = new Vector2(0f, 360f);
        public Vector2 rotationZRange = new Vector2(0f, 360f);

        [Header("Random Seed")]
        [Range(0, 100)]
        public int seed = 0;
        private int lastSeed;

        [Header("Gizmo Settings")]
        public bool drawGizmo = true;

        private Vector3 lastPosition;
        private Quaternion lastRotation;

        private void OnValidate()
        {
            // Ensure weight arrays match prefab arrays
            ValidateWeightArrays();
            NormalizeWeights();

            if (transform.position != lastPosition || transform.rotation != lastRotation || seed != lastSeed)
            {
                lastPosition = transform.position;
                lastRotation = transform.rotation;
                lastSeed = seed;
            }
        }

        private void ValidateWeightArrays()
        {
            if (largeObjectPrefabs != null)
            {
                if (largeObjectWeights == null || largeObjectWeights.Length != largeObjectPrefabs.Length)
                {
                    float[] newWeights = new float[largeObjectPrefabs.Length];
                    for (int i = 0; i < newWeights.Length; i++)
                    {
                        newWeights[i] = (largeObjectWeights != null && i < largeObjectWeights.Length) ? largeObjectWeights[i] : 100f / largeObjectPrefabs.Length;
                    }
                    largeObjectWeights = newWeights;
                }
            }

            if (mediumObjectPrefabs != null)
            {
                if (mediumObjectWeights == null || mediumObjectWeights.Length != mediumObjectPrefabs.Length)
                {
                    float[] newWeights = new float[mediumObjectPrefabs.Length];
                    for (int i = 0; i < newWeights.Length; i++)
                    {
                        newWeights[i] = (mediumObjectWeights != null && i < mediumObjectWeights.Length) ? mediumObjectWeights[i] : 100f / mediumObjectPrefabs.Length;
                    }
                    mediumObjectWeights = newWeights;
                }
            }

            if (smallObjectPrefabs != null)
            {
                if (smallObjectWeights == null || smallObjectWeights.Length != smallObjectPrefabs.Length)
                {
                    float[] newWeights = new float[smallObjectPrefabs.Length];
                    for (int i = 0; i < newWeights.Length; i++)
                    {
                        newWeights[i] = (smallObjectWeights != null && i < smallObjectWeights.Length) ? smallObjectWeights[i] : 100f / smallObjectPrefabs.Length;
                    }
                    smallObjectWeights = newWeights;
                }
            }
        }

        private void NormalizeWeights()
        {
            NormalizeWeightArray(largeObjectWeights);
            NormalizeWeightArray(mediumObjectWeights);
            NormalizeWeightArray(smallObjectWeights);
        }

        private void NormalizeWeightArray(float[] weights)
        {
            if (weights == null || weights.Length == 0) return;

            float totalWeight = 0f;
            foreach (float weight in weights)
            {
                totalWeight += weight;
            }

            if (totalWeight > 0f)
            {
                float normalizedTotal = 0f;
                for (int i = 0; i < weights.Length - 1; i++)
                {
                    weights[i] = (weights[i] / totalWeight) * 100f;
                    normalizedTotal += weights[i];
                }
                // Ensure the last element gets the remaining percentage to avoid floating point errors
                if (weights.Length > 0)
                {
                    weights[weights.Length - 1] = 100f - normalizedTotal;
                }
            }
        }

        public void SpawnObjects()
        {
            ClearObjects();
            UnityEngine.Random.InitState(seed);

            for (int i = 0; i < largeObjectCount; i++)
            {
                Vector3 largeObjectPosition = GetSpawnPositionOnTerrain(transform.position, spawnRadius, out Vector3 groundNormalLarge);
                if (largeObjectPosition == Vector3.zero) continue;

                GameObject largePrefab = GetRandomWeightedPrefab(largeObjectPrefabs, largeObjectWeights);
                if (!largePrefab) continue;

                GameObject largeObject = InstantiateObject(largePrefab, largeObjectPosition, largeObjectSizeRange,
                    alignToGround ? groundNormalLarge : (Vector3?)null);

#if UNITY_EDITOR
                Undo.RegisterCreatedObjectUndo(largeObject, "Spawn Large Object");
#endif

                for (int j = 0; j < mediumObjectCountPerLarge; j++)
                {
                    Vector3 mediumObjectPosition = GetSpawnPositionOnTerrain(largeObject.transform.position, spawnRadiusLargeObject, out Vector3 groundNormalMed);
                    if (mediumObjectPosition == Vector3.zero) continue;

                    GameObject mediumPrefab = GetRandomWeightedPrefab(mediumObjectPrefabs, mediumObjectWeights);
                    if (!mediumPrefab) continue;

                    GameObject mediumObject = InstantiateObject(mediumPrefab, mediumObjectPosition, mediumObjectSizeRange,
                        alignToGround ? groundNormalMed : (Vector3?)null);

#if UNITY_EDITOR
                    Undo.RegisterCreatedObjectUndo(mediumObject, "Spawn Medium Object");
#endif

                    for (int k = 0; k < smallObjectCountPerMedium; k++)
                    {
                        Vector3 smallObjectPosition = GetSpawnPositionOnTerrain(mediumObject.transform.position, spawnRadiusMediumObject, out Vector3 groundNormalSmall);
                        if (smallObjectPosition == Vector3.zero) continue;

                        GameObject smallPrefab = GetRandomWeightedPrefab(smallObjectPrefabs, smallObjectWeights);
                        if (!smallPrefab) continue;

                        GameObject smallObject = InstantiateObject(smallPrefab, smallObjectPosition, smallObjectSizeRange,
                            alignToGround ? groundNormalSmall : (Vector3?)null);

#if UNITY_EDITOR
                        Undo.RegisterCreatedObjectUndo(smallObject, "Spawn Small Object");
#endif
                    }
                }
            }
        }

        public void ClearObjects()
        {
#if UNITY_EDITOR
            Undo.RecordObject(this, "Clear Objects");
#endif
            while (transform.childCount > 0)
            {
#if UNITY_EDITOR
                Undo.DestroyObjectImmediate(transform.GetChild(0).gameObject);
#else
            DestroyImmediate(transform.GetChild(0).gameObject);
#endif
            }
        }

        private GameObject GetRandomWeightedPrefab(GameObject[] prefabs, float[] weights)
        {
            if (prefabs == null || prefabs.Length == 0 || weights == null || weights.Length == 0) return null;
            if (prefabs.Length != weights.Length) return null;

            float totalWeight = 0f;
            for (int i = 0; i < prefabs.Length; i++)
            {
                if (prefabs[i] != null)
                    totalWeight += weights[i];
            }

            if (totalWeight <= 0f) return null;

            float randomValue = UnityEngine.Random.Range(0f, totalWeight);
            float currentWeight = 0f;

            for (int i = 0; i < prefabs.Length; i++)
            {
                if (prefabs[i] != null)
                {
                    currentWeight += weights[i];
                    if (randomValue <= currentWeight)
                        return prefabs[i];
                }
            }

            // Fallback to first valid prefab
            for (int i = 0; i < prefabs.Length; i++)
            {
                if (prefabs[i] != null)
                    return prefabs[i];
            }

            return null;
        }

        private Vector3 GetSpawnPositionOnTerrain(Vector3 center, float radius, out Vector3 normal)
        {
            normal = Vector3.up;
            const int maxAttempts = 5;
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                Vector3 randomPosition = center + UnityEngine.Random.insideUnitSphere * radius;
                randomPosition.y = 1000f; // start high above
                if (Physics.Raycast(randomPosition, Vector3.down, out RaycastHit hit, Mathf.Infinity))
                {
                    // Use the specified spawn tag instead of hardcoded "Terrain"
                    if (hit.collider.CompareTag(spawnTag))
                    {
                        normal = hit.normal;

                        // Check slope angle
                        float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
                        if (slopeAngle >= minSlopeAngle && slopeAngle <= maxSlopeAngle)
                        {
                            return hit.point;
                        }
                    }
                }
            }
            return Vector3.zero;
        }

        private GameObject InstantiateObject(GameObject prefab, Vector3 position, Vector2 sizeRange, Vector3? groundNormal)
        {
            Quaternion rotation;
            if (alignToGround && groundNormal.HasValue)
            {
                // Align to ground normal but only allow Y-axis rotation
                Vector3 forward = Vector3.ProjectOnPlane(transform.forward, groundNormal.Value).normalized;
                if (forward == Vector3.zero)
                    forward = Vector3.ProjectOnPlane(Vector3.forward, groundNormal.Value).normalized;

                rotation = Quaternion.LookRotation(forward, groundNormal.Value);

                // Apply random Y rotation in local space
                float yaw = UnityEngine.Random.Range(rotationYRange.x, rotationYRange.y);
                rotation = rotation * Quaternion.Euler(0f, yaw, 0f);
            }
            else
            {
                if (randomFullRotation)
                {
                    float xRot = UnityEngine.Random.Range(rotationXRange.x, rotationXRange.y);
                    float yRot = UnityEngine.Random.Range(rotationYRange.x, rotationYRange.y);
                    float zRot = UnityEngine.Random.Range(rotationZRange.x, rotationZRange.y);
                    rotation = Quaternion.Euler(xRot, yRot, zRot);
                }
                else
                {
                    rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(rotationYRange.x, rotationYRange.y), 0f);
                }
            }

            GameObject obj = Instantiate(prefab, position, rotation);
            obj.transform.localScale = Vector3.one * UnityEngine.Random.Range(sizeRange.x, sizeRange.y);
            obj.transform.parent = transform;
            return obj;
        }

        private void OnDrawGizmos()
        {
            if (drawGizmo)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(transform.position, spawnRadius);
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(HierarchicalObjectSpawner))]
    public class HierarchicalObjectSpawnerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            HierarchicalObjectSpawner spawner = (HierarchicalObjectSpawner)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Controls", EditorStyles.boldLabel);

            if (GUILayout.Button("Spawn Objects"))
            {
                spawner.SpawnObjects();
            }

            if (GUILayout.Button("Clear Objects"))
            {
                spawner.ClearObjects();
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Weight arrays automatically match prefab arrays. " +
                                  "Weights are normalized to total 100%. Higher values = higher spawn probability.", MessageType.Info);

            EditorGUILayout.HelpBox("Make sure your terrain objects are tagged with the specified 'Spawn Tag'.", MessageType.Info);

            EditorGUILayout.HelpBox("Slope filtering: Objects will only spawn on surfaces with slope angles between the min and max values (0° = flat, 90° = vertical).", MessageType.Info);
        }
    }
#endif
}