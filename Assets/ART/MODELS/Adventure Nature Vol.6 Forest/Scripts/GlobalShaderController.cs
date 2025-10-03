using UnityEngine;

[ExecuteAlways] // makes it run in edit mode too
public class GlobalShaderController : MonoBehaviour
{
    [Header("Shader Graph Controls")]
    public Vector2 GradientTopToBottom = new Vector2(0f, 1f);
    public float WindSpeed = 1f;
    public float WindStrength = 1f;
    public float TrunkCurvature = 0.2f;

    void Update()
    {
        ApplyGlobals();
    }

    void OnValidate()
    {
        // Apply immediately when values change in Inspector
        ApplyGlobals();
    }

    private void ApplyGlobals()
    {
        Shader.SetGlobalVector("_GradientTopToBottom", GradientTopToBottom);
        Shader.SetGlobalFloat("_WindSpeed", WindSpeed);
        Shader.SetGlobalFloat("_WindStrength", WindStrength);
        Shader.SetGlobalFloat("_TrunkCurvature", TrunkCurvature);
    }
}
