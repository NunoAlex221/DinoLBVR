using System.Collections;
using UnityEngine;

public class GlowBlink : MonoBehaviour
{
    [SerializeField] Renderer myRenderer;
    [SerializeField] Color emissionColor = Color.blue;
    [SerializeField] float minIntensity = 0f;
    [SerializeField] float maxIntensity = 10f;
    [SerializeField] float speed = 2f;

    private Material matInstance;

    void Awake()
    {
        if (myRenderer == null) myRenderer = GetComponent<Renderer>();
        if (myRenderer == null)
        {
            Debug.LogError("GlowBlink: no Renderer assigned or found on the GameObject.");
            enabled = false;
            return;
        }

        // This creates a runtime instance of the material (so only this object changes)
        matInstance = myRenderer.material;
        matInstance.EnableKeyword("_EMISSION"); // make sure emission is active

        Color initialEmission = emissionColor * minIntensity;
        matInstance.SetColor("_EmissionColor", initialEmission);

        StartCoroutine(FadeIn());
    }

    public IEnumerator FadeIn()
    {
        for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / speed)
        {
            // Interpolate between max and min
            float intensity = Mathf.Lerp(minIntensity, maxIntensity, t);

            // If intensity is negative, flip its sign but also tint the color darker
            float safeIntensity = Mathf.Abs(intensity);

            // Optional: if you want the negative phase to fade towards black
            // instead of flipping bright, multiply by 0 instead of Abs
            // float safeIntensity = Mathf.Max(0f, intensity);

            Color finalEmission = emissionColor * safeIntensity;
            matInstance.SetColor("_EmissionColor", finalEmission);
            yield return null;
        }
        StartCoroutine(FadeOut());
    }

    public IEnumerator FadeOut()
    {
        for (float t = 1.0f; t > 0.0f; t -= Time.deltaTime / speed)
        {
            // Interpolate between max and min
            float intensity = Mathf.Lerp(minIntensity, maxIntensity, t);

            // If intensity is negative, flip its sign but also tint the color darker
            float safeIntensity = Mathf.Abs(intensity);

            // Optional: if you want the negative phase to fade towards black
            // instead of flipping bright, multiply by 0 instead of Abs
            // float safeIntensity = Mathf.Max(0f, intensity);

            Color finalEmission = emissionColor * safeIntensity;
            matInstance.SetColor("_EmissionColor", finalEmission);
            yield return null;
        }

        StartCoroutine(FadeIn());
    }
}
