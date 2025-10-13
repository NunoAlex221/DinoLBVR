using UnityEngine;

public class SceneFogController : MonoBehaviour
{
    [Header("Fog Settings")]
    public bool enableFog = true;
    public FogMode fogMode = FogMode.Exponential;
    [Range(0f, 1f)] public float density = 0.2f;
    public Color fogColor = new Color(0.0f, 0.3f, 0.6f); // deep blue

    void OnEnable()
    {
        RenderSettings.fog = enableFog;
        RenderSettings.fogMode = fogMode;
        RenderSettings.fogDensity = density;
        RenderSettings.fogColor = fogColor;
    }

    void OnDisable()
    {
        // reset when leaving scene
        RenderSettings.fog = false;
    }
}
