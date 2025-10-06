using UnityEngine;

public enum AmbientModeOption
{
    ProceduralSkybox,
    GradientColor
}

[System.Serializable]
public class TimeOfDaySettings
{
    public Gradient sunColorGradient = new Gradient();
    public AnimationCurve sunIntensityCurve = AnimationCurve.EaseInOut(0f, 0.1f, 1f, 1.5f);
    public Gradient fogColorGradient = new Gradient();

    // Fog density overwrite
    public AnimationCurve fogDensityCurve = AnimationCurve.Linear(0f, 0.01f, 1f, 0.01f);

    // Environment curves
    public AnimationCurve environmentIndirectIntensityCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);
    public AnimationCurve environmentReflectionIntensityCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);

    // Ambient color gradient (used if GradientColor mode is selected)
    public Gradient ambientColorGradient = new Gradient();
}

[ExecuteAlways]
public class URPWorldLightingController : MonoBehaviour
{
    [SerializeField] private Light sunLight;
    [SerializeField] private TimeOfDaySettings timeSettings = new TimeOfDaySettings();
    [SerializeField] private bool showDebugGUI = false;

    [Header("Ambient Lighting Mode (choose here)")]
    [SerializeField] private AmbientModeOption ambientLightingMode = AmbientModeOption.ProceduralSkybox;

    private void OnEnable()
    {
        if (!sunLight) sunLight = FindDirectionalLight();
        SetupDefaultGradients();
        UpdateLighting();
    }

    private void Update()
    {
        if (sunLight) UpdateLighting();
    }

    private void OnValidate()
    {
        if (!sunLight) sunLight = FindDirectionalLight();
        if (sunLight) UpdateLighting();
    }

    private Light FindDirectionalLight()
    {
        foreach (var l in Object.FindObjectsOfType<Light>(true))
            if (l.type == LightType.Directional) return l;
        return null;
    }

    private void SetupDefaultGradients()
    {
        void SetDefault(Gradient g, Color[] cols)
        {
            if (g.colorKeys.Length <= 2)
            {
                var ck = new GradientColorKey[cols.Length];
                var ak = new GradientAlphaKey[cols.Length];
                for (int i = 0; i < cols.Length; i++)
                {
                    float t = i / (float)(cols.Length - 1);
                    ck[i] = new GradientColorKey(cols[i], t);
                    ak[i] = new GradientAlphaKey(1f, t);
                }
                g.SetKeys(ck, ak);
            }
        }

        SetDefault(timeSettings.sunColorGradient, new[] {
            new Color(1f,0.3f,0.1f), new Color(1f,0.7f,0.3f),
            new Color(1f,0.95f,0.8f), new Color(1f,0.7f,0.3f),
            new Color(1f,0.3f,0.1f)
        });

        SetDefault(timeSettings.fogColorGradient, new[] {
            new Color(0.5f,0.2f,0.1f), new Color(0.8f,0.5f,0.3f),
            new Color(0.7f,0.8f,0.9f), new Color(0.8f,0.5f,0.3f),
            new Color(0.3f,0.1f,0.2f)
        });

        SetDefault(timeSettings.ambientColorGradient, new[] {
            new Color(0.1f,0.1f,0.2f), new Color(0.3f,0.3f,0.5f),
            new Color(0.8f,0.8f,0.9f), new Color(0.5f,0.4f,0.3f),
            new Color(0.1f,0.1f,0.2f)
        });
    }

    private void UpdateLighting()
    {
        if (!sunLight) return;

        // Map sun forward.y [-1..1] to gradient time [0..1]; 0/1 = horizon, 0.5 = zenith
        float sunY = sunLight.transform.forward.y;
        float t = Mathf.Clamp01(-sunY * 0.5f + 0.5f);

        // Sun color + intensity
        sunLight.color = timeSettings.sunColorGradient.Evaluate(t);
        sunLight.intensity = timeSettings.sunIntensityCurve.Evaluate(t);

        // Fog color
        RenderSettings.fogColor = timeSettings.fogColorGradient.Evaluate(t);

        // Fog density overwrite
        if (timeSettings.fogDensityCurve != null)
            RenderSettings.fogDensity = Mathf.Max(0f, timeSettings.fogDensityCurve.Evaluate(t));

        // Environment Indirect Intensity (clamped 0–1)
        if (timeSettings.environmentIndirectIntensityCurve != null)
            RenderSettings.ambientIntensity = Mathf.Clamp01(timeSettings.environmentIndirectIntensityCurve.Evaluate(t));

        // Environment Reflections Intensity (clamped 0–1)
        if (timeSettings.environmentReflectionIntensityCurve != null)
            RenderSettings.reflectionIntensity = Mathf.Clamp01(timeSettings.environmentReflectionIntensityCurve.Evaluate(t));

        // --- Ambient Lighting Mode (controlled by inspector enum, not Lighting panel) ---
        if (ambientLightingMode == AmbientModeOption.ProceduralSkybox)
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
        }
        else if (ambientLightingMode == AmbientModeOption.GradientColor)
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = timeSettings.ambientColorGradient.Evaluate(t);
        }
    }

    private void OnGUI()
    {
        if (!showDebugGUI || !sunLight) return;
        GUILayout.BeginArea(new Rect(10, 10, 320, 150));
        GUILayout.BeginVertical("box");
        GUILayout.Label("SunY: " + sunLight.transform.forward.y.ToString("F2"));
        GUILayout.Label("FogDensity: " + RenderSettings.fogDensity.ToString("F5"));
        GUILayout.Label("AmbientIntensity: " + RenderSettings.ambientIntensity.ToString("F2"));
        GUILayout.Label("ReflectionIntensity: " + RenderSettings.reflectionIntensity.ToString("F2"));
        GUILayout.Label("AmbientMode: " + ambientLightingMode.ToString());
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}
